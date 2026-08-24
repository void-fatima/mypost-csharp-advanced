import { expect, test } from '@playwright/test';

test('landing page preserves the primary tracking task without horizontal overflow', async ({ page }) => {
  await page.goto('/');
  await expect(page.getByRole('heading', { level: 1 })).toContainText('every handoff stays visible');
  await expect(page.getByRole('button', { name: 'Track shipment' })).toBeVisible();
  const overflow = await page.evaluate(() => document.documentElement.scrollWidth > document.documentElement.clientWidth);
  expect(overflow).toBe(false);
  await page.getByLabel('Tracking code').fill('MP-DEMO-100004');
  await page.getByRole('button', { name: 'Track shipment' }).click();
  await expect(page).toHaveURL(/\/track\/MP-DEMO-100004$/);
});
