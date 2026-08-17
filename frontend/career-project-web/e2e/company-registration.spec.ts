import { test, expect } from '@playwright/test';
import { uniqueEmail } from './helpers';

test('Company registration redirects to the company dashboard', async ({ page }) => {
  const email = uniqueEmail('e2e-company-reg');
  const companyName = 'E2E Test Company';

  await page.goto('/register');
  await page.getByRole('button', { name: 'კომპანია' }).click();

  await page.getByLabel('კომპანიის დასახელება').fill(companyName);
  await page.getByLabel('ელ.ფოსტა').fill(email);
  await page.getByLabel('პაროლი').fill('correct-password');
  await page.getByRole('button', { name: 'რეგისტრაცია' }).click();

  await expect(page).toHaveURL(/\/dashboard\/company/);
});
