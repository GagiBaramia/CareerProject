import { test, expect } from '@playwright/test';
import { uniqueEmail } from './helpers';

test('Person registration redirects to the person dashboard with the display name', async ({ page }) => {
  const email = uniqueEmail('e2e-person-reg');
  const fullName = 'E2E Person Reg';

  await page.goto('/register');
  await page.getByRole('button', { name: 'კანდიდატი' }).click();

  await page.getByLabel('სრული სახელი').fill(fullName);
  await page.getByLabel('ელ.ფოსტა').fill(email);
  await page.getByLabel('პაროლი').fill('correct-password');
  await page.getByRole('button', { name: 'რეგისტრაცია' }).click();

  await expect(page).toHaveURL(/\/dashboard\/person/);
  await expect(page.getByText(fullName)).toBeVisible();
});
