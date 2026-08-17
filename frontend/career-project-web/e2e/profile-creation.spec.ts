import { test, expect } from '@playwright/test';
import { uniqueEmail, registerPersonViaApi, injectAuth } from './helpers';

test('Person can create their profile and see it persist after a reload', async ({ page, request }) => {
  const email = uniqueEmail('e2e-profile');
  const auth = await registerPersonViaApi(request, email, 'E2E Profile Person');
  await injectAuth(page, auth);

  await page.goto('/profile/edit');

  await page.getByLabel('პროფესიული სათაური (Headline)').fill('Junior Backend Developer');
  await page.getByLabel('მოგვიყევით თქვენს შესახებ (Summary)').fill('E2E ტესტით შექმნილი პროფილი.');
  await page.getByLabel('მდებარეობა').fill('თბილისი');

  await page.getByPlaceholder('მოძებნეთ და დაამატეთ უნარი...').fill('C#');
  await page.getByRole('button', { name: 'C#' }).click();

  await page.getByRole('button', { name: 'შემდეგი →' }).click();

  // Successful save advances the wizard to step 2 (a stub) - confirms the PUT round-tripped.
  await expect(page.getByText('ეს ეტაპი მალე დაემატება.')).toBeVisible();

  await page.reload();

  await expect(page.getByLabel('პროფესიული სათაური (Headline)')).toHaveValue('Junior Backend Developer');
  await expect(page.getByLabel('მდებარეობა')).toHaveValue('თბილისი');
  // "C#" legitimately appears twice (skill list + live preview panel) - scope to the skill list.
  await expect(page.locator('.selected-skill-name')).toHaveText('C#');
});
