import { test, expect } from '@playwright/test';
import { registerPersonViaApi, setPersonProfileViaApi, uniqueEmail, injectAuth } from './helpers';

test('Person can submit an application to a recommended vacancy', async ({ page, request }) => {
  const email = uniqueEmail('e2e-apply');
  const auth = await registerPersonViaApi(request, email, 'E2E Apply Person');
  await setPersonProfileViaApi(request, auth.token, {
    fullName: 'E2E Apply Person',
    headline: 'Backend Developer',
    location: 'თბილისი'
  });
  await injectAuth(page, auth);

  await page.goto('/jobs/recommended');
  await expect(page.locator('.job-card').first()).toBeVisible();

  const applyButton = page.locator('.apply-btn').first();
  await applyButton.click();

  await expect(applyButton).toHaveText(/გაგზავნილია/);
  await expect(applyButton).toBeDisabled();
});
