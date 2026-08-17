import { defineConfig, devices } from '@playwright/test';

// E2E-ს სჭირდება მთელი რეალური სტეკი (Docker infra + 5 backend service), არა მხოლოდ Angular:
// გაუშვი ../../start-dev.bat (ან ხელით ყველა service) სანამ `npm run test:e2e`-ს გაუშვებ.
export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  workers: 1,
  retries: 0,
  timeout: 30_000,
  reporter: 'list',
  use: {
    baseURL: 'http://localhost:4200',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  webServer: {
    command: 'npx ng serve',
    url: 'http://localhost:4200',
    reuseExistingServer: true,
    timeout: 120_000,
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
});
