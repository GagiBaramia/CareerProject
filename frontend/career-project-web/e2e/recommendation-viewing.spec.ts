import { test, expect } from '@playwright/test';
import { registerPersonViaApi, setPersonProfileViaApi, uniqueEmail, injectAuth } from './helpers';

test('Person sees recommended vacancies with a matching percentage', async ({ page, request }) => {
  const email = uniqueEmail('e2e-recommend');
  const auth = await registerPersonViaApi(request, email, 'E2E Recommend Person');
  await setPersonProfileViaApi(request, auth.token, {
    fullName: 'E2E Recommend Person',
    headline: 'Backend Developer',
    cvSummary: 'გამოცდილი Backend დეველოპერი, ვიცი C# და PostgreSQL.',
    location: 'თბილისი'
  });
  await injectAuth(page, auth);

  await page.goto('/jobs/recommended');

  await expect(page.locator('.job-card').first()).toBeVisible();
  await expect(page.locator('.match-badge').first()).toContainText('%');
});
