import { test, expect } from '@playwright/test';
import { API_BASE_URL, registerCompanyViaApi, uniqueEmail, injectAuth } from './helpers';

test('Company can publish a vacancy and it is actually persisted', async ({ page, request }) => {
  const email = uniqueEmail('e2e-vacancy');
  const auth = await registerCompanyViaApi(request, email, 'E2E Vacancy Co');
  await injectAuth(page, auth);

  const jobTitle = `E2E Test Vacancy ${Date.now()}`;

  await page.goto('/jobs/new');

  await page.getByLabel('ვაკანსიის სათაური *').fill(jobTitle);
  await page.getByLabel('ვაკანსიის აღწერა *').fill('E2E ტესტით შექმნილი ვაკანსია.');
  await page.getByLabel('ადგილმდებარეობა *').fill('თბილისი');
  await page.getByLabel('სამუშაო გრაფიკი *').selectOption('FullTime');
  await page.getByLabel('სამუშაო ფორმატი *').selectOption('Remote');

  await page.getByPlaceholder('მოძებნეთ და დაამატეთ უნარი...').fill('C#');
  await page.getByRole('button', { name: 'C#' }).click();

  await page.getByRole('button', { name: 'ვაკანსიის გამოქვეყნება' }).click();

  await expect(page).toHaveURL(/\/dashboard\/company/);

  // The UI navigating away isn't proof of anything by itself - confirm against the real API
  // that the job actually made it into the database.
  const jobsResponse = await request.get(`${API_BASE_URL}/api/jobs`, {
    headers: { Authorization: `Bearer ${auth.token}` }
  });
  const jobs = (await jobsResponse.json()) as Array<{ title: string }>;
  expect(jobs.some((j) => j.title === jobTitle)).toBe(true);
});
