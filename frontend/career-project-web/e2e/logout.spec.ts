import { test, expect } from '@playwright/test';
import { registerPersonViaApi, uniqueEmail, injectAuth } from './helpers';

test('Logout clears session and blocks returning to a protected page via browser back', async ({ page, request }) => {
  const email = uniqueEmail('e2e-logout');
  const auth = await registerPersonViaApi(request, email, 'E2E Logout Person');
  await injectAuth(page, auth);

  await page.goto('/dashboard/person');
  await expect(page.getByText('E2E Logout Person')).toBeVisible();

  await page.getByRole('button', { name: 'გასვლა' }).click();
  await expect(page).toHaveURL(/\/login/);

  const stored = await page.evaluate(() => window.localStorage.getItem('career_project_auth'));
  expect(stored).toBeNull();

  // Browser back should not resurrect the authenticated view - authGuard re-checks on navigation.
  await page.goBack();
  await expect(page).toHaveURL(/\/login/);
});

test('An expired/invalid token triggers an automatic logout on the next API call', async ({ page, request }) => {
  const email = uniqueEmail('e2e-expired-token');
  const auth = await registerPersonViaApi(request, email, 'E2E Expired Token Person');

  // A syntactically-valid-looking but unverifiable token - the backend will reject it with 401.
  const tamperedAuth = { ...auth, token: auth.token.slice(0, -10) + 'tampered00' };
  await injectAuth(page, tamperedAuth);

  await page.goto('/dashboard/person');
  // The dashboard's own API calls (profile/recommendations/etc.) will 401 - the interceptor
  // should detect this and force a redirect to /login rather than leaving a broken dashboard.
  await expect(page).toHaveURL(/\/login/, { timeout: 10000 });

  const stored = await page.evaluate(() => window.localStorage.getItem('career_project_auth'));
  expect(stored).toBeNull();
});
