import { defineConfig, devices } from '@playwright/test';

const externalServer = process.env.PLAYWRIGHT_EXTERNAL_SERVER === '1';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  forbidOnly: true,
  retries: 0,
  reporter: 'list',
  use: {
    baseURL: 'http://127.0.0.1:5173',
    trace: 'retain-on-failure',
  },
  webServer: externalServer ? undefined : {
    command: 'node ./node_modules/vite/bin/vite.js --host 127.0.0.1',
    url: 'http://127.0.0.1:5173',
    reuseExistingServer: true,
    timeout: 120_000,
  },
  projects: [
    { name: 'mobile-375', use: { ...devices['Desktop Chrome'], channel: 'chrome', viewport: { width: 375, height: 812 } } },
    { name: 'tablet-768', use: { ...devices['Desktop Chrome'], channel: 'chrome', viewport: { width: 768, height: 1024 } } },
    { name: 'desktop-1024', use: { ...devices['Desktop Chrome'], channel: 'chrome', viewport: { width: 1024, height: 900 } } },
    { name: 'wide-1440', use: { ...devices['Desktop Chrome'], channel: 'chrome', viewport: { width: 1440, height: 1000 } } },
  ],
});
