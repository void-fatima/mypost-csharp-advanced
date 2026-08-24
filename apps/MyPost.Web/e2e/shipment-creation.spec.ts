import { expect, test } from '@playwright/test';

const session = { accessToken: 'test-access-token', expiresAtUtc: '2026-08-24T20:00:00Z', user: { id: '11111111-1111-1111-1111-111111111111', email: 'customer@mypost.local', displayName: 'Sara Ahmadi', role: 'Customer' } };
const address = { id: '22222222-2222-2222-2222-222222222222', label: 'Home', line1: '12 Valiasr Street', city: 'Tehran', province: 'Tehran', postalCode: '1599911111', isDefault: true };
const shipment = { id: '33333333-3333-3333-3333-333333333333', trackingCode: 'MP-260824-ABC12345', senderUserId: session.user.id, recipientName: 'Jane Smith', recipientPhone: '09123456789', senderAddress: { ...address, country: 'Iran' }, destinationAddress: { label: 'Office', line1: '24 Demo Avenue', city: 'Shiraz', province: 'Fars', postalCode: '7188811111', country: 'Iran' }, type: 'Letter', weightGrams: 100, serviceLevel: 'Standard', calculatedPrice: 53000, status: 'AwaitingPickup', createdAtUtc: '2026-08-24T19:00:00Z', updatedAtUtc: '2026-08-24T19:00:00Z', history: [{ status: 'Created', description: 'Shipment created', occurredAtUtc: '2026-08-24T19:00:00Z' }, { status: 'AwaitingPickup', description: 'Shipment is awaiting postal acceptance', occurredAtUtc: '2026-08-24T19:00:01Z' }] };

test('customer creates a shipment and opens the public timeline', async ({ page }) => {
  await page.route('**/api/v1/auth/refresh', (route) => route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(session) }));
  await page.route('**/api/v1/customer/addresses', async (route) => {
    if (route.request().method() === 'GET') await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify([address]) });
    else await route.continue();
  });
  await page.route('**/api/v1/customer/shipments', (route) => route.fulfill({ status: 201, contentType: 'application/json', body: JSON.stringify(shipment) }));
  await page.route('**/api/v1/tracking/MP-260824-ABC12345', (route) => route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify({ trackingCode: shipment.trackingCode, recipient: 'J. S.', destination: 'Shiraz, Fars', type: shipment.type, serviceLevel: shipment.serviceLevel, status: shipment.status, createdAtUtc: shipment.createdAtUtc, history: shipment.history }) }));

  await page.goto('/customer/create-shipment');
  await page.getByText('Home', { exact: true }).click();
  await page.getByRole('button', { name: /continue/i }).click();
  await page.getByLabel('Recipient name').fill('Jane Smith');
  await page.getByLabel('Recipient phone').fill('09123456789');
  await page.getByLabel('Address label').fill('Office');
  await page.getByLabel('Street address').fill('24 Demo Avenue');
  await page.getByLabel('City').fill('Shiraz');
  await page.getByLabel('Province').fill('Fars');
  await page.getByLabel('Postal code').fill('7188811111');
  await page.getByRole('button', { name: /continue/i }).click();
  await page.getByRole('button', { name: /continue/i }).click();
  await expect(page.getByText('MP-260824-ABC12345')).toBeVisible();
  await page.goto('/track/MP-260824-ABC12345');
  await expect(page.getByRole('heading', { name: 'Shipment timeline' })).toBeVisible();
  await expect(page.getByText('J. S.')).toBeVisible();
});
