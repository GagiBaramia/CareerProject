import { test, expect } from '@playwright/test';
import { uniqueEmail } from './helpers';

// End-to-end "Definition of Done" scenario: Company creates a vacancy, a Person applies,
// the Company accepts (which must auto-create a private chat), both sides exchange messages
// in realtime (two separate browser contexts = two separate SignalR connections, proving this
// isn't just "persisted then reloaded"), and finally both log out.
test('full hire flow: vacancy -> apply -> accept -> realtime chat -> logout', async ({ browser }) => {
  const companyContext = await browser.newContext();
  const personContext = await browser.newContext();
  const companyPage = await companyContext.newPage();
  const personPage = await personContext.newPage();

  const companyEmail = uniqueEmail('e2e-hireflow-company');
  const personEmail = uniqueEmail('e2e-hireflow-person');
  const jobTitle = `E2E Hire Flow Job ${Date.now()}`;

  // --- Company: register, create vacancy ---
  await companyPage.goto('/register');
  await companyPage.getByRole('button', { name: 'კომპანია' }).click();
  await companyPage.getByLabel('კომპანიის დასახელება').fill('E2E Hire Flow Co');
  await companyPage.getByLabel('ელ.ფოსტა').fill(companyEmail);
  await companyPage.getByLabel('პაროლი').fill('correct-password');
  await companyPage.getByRole('button', { name: 'რეგისტრაცია' }).click();
  await expect(companyPage).toHaveURL(/\/dashboard\/company/);

  await companyPage.goto('/jobs/new');
  await companyPage.getByLabel('ვაკანსიის სათაური *').fill(jobTitle);
  await companyPage.getByLabel('ვაკანსიის აღწერა *').fill('E2E hire-flow ტესტისთვის შექმნილი ვაკანსია.');
  await companyPage.getByLabel('ადგილმდებარეობა *').fill('თბილისი');
  await companyPage.getByLabel('სამუშაო გრაფიკი *').selectOption('FullTime');
  await companyPage.getByLabel('სამუშაო ფორმატი *').selectOption('Remote');
  await companyPage.getByRole('button', { name: 'ვაკანსიის გამოქვეყნება' }).click();
  await expect(companyPage).toHaveURL(/\/dashboard\/company/);

  // --- Person: register, apply to that exact job via the recommendations page ---
  await personPage.goto('/register');
  await personPage.getByLabel('სრული სახელი').fill('E2E Hire Flow Person');
  await personPage.getByLabel('ელ.ფოსტა').fill(personEmail);
  await personPage.getByLabel('პაროლი').fill('correct-password');
  await personPage.getByRole('button', { name: 'რეგისტრაცია' }).click();
  await expect(personPage).toHaveURL(/\/dashboard\/person/);

  await personPage.goto('/jobs/recommended');
  const jobCard = personPage.locator('.job-card', { hasText: jobTitle });
  await expect(jobCard).toBeVisible();
  await jobCard.locator('.apply-btn').click();
  await expect(jobCard.locator('.apply-btn')).toHaveText(/გაგზავნილია/);

  // --- Company: find the applicant, accept ---
  await companyPage.goto('/company/applicants');
  const applicantRow = companyPage.locator('.applicant-card', { hasText: 'E2E Hire Flow Person' });
  await expect(applicantRow).toBeVisible();
  await applicantRow.locator('.status-select').selectOption('Accepted');
  await expect(applicantRow.locator('.status-badge')).toHaveText(/მიღებულია/);

  // --- Candidate: sees the chat appear, opens /messages ---
  await personPage.goto('/messages');
  const personConversation = personPage.locator('.conversation-item', { hasText: 'E2E Hire Flow Co' });
  await expect(personConversation).toBeVisible();
  await personConversation.click();

  // --- Company: opens /messages too, same conversation ---
  await companyPage.goto('/messages');
  const companyConversation = companyPage.locator('.conversation-item', { hasText: 'E2E Hire Flow Person' });
  await expect(companyConversation).toBeVisible();
  await companyConversation.click();

  // --- Candidate sends a message; Company must receive it in realtime (no reload) ---
  const candidateMessage = 'გამარჯობა, მაინტერესებს ვაკანსია!';
  await personPage.getByPlaceholder('დაწერეთ შეტყობინება...').fill(candidateMessage);
  await personPage.getByRole('button', { name: 'გაგზავნა' }).click();
  await expect(companyPage.locator('.message-text', { hasText: candidateMessage })).toBeVisible({ timeout: 10000 });

  // --- Company replies; Candidate must receive it in realtime ---
  const companyReply = 'გამარჯობა! მოხარული ვართ, დაგვიკავშირდით.';
  await companyPage.getByPlaceholder('დაწერეთ შეტყობინება...').fill(companyReply);
  await companyPage.getByRole('button', { name: 'გაგზავნა' }).click();
  await expect(personPage.locator('.message-text', { hasText: companyReply })).toBeVisible({ timeout: 10000 });

  // --- Both log out ---
  await personPage.goto('/dashboard/person');
  await personPage.getByRole('button', { name: 'გასვლა' }).click();
  await expect(personPage).toHaveURL(/\/login/);

  await companyPage.goto('/dashboard/company');
  await companyPage.getByRole('button', { name: 'გასვლა' }).click();
  await expect(companyPage).toHaveURL(/\/login/);

  // --- Candidate logs back in - application + chat history must still be there ---
  await personPage.getByLabel('ელ.ფოსტა').fill(personEmail);
  await personPage.getByLabel('პაროლი').fill('correct-password');
  await personPage.getByRole('button', { name: 'შესვლა' }).click();
  await expect(personPage).toHaveURL(/\/dashboard\/person/);

  await personPage.goto('/applications');
  await expect(personPage.locator('.application-card', { hasText: jobTitle })).toBeVisible();

  await personPage.goto('/messages');
  await expect(personPage.locator('.conversation-item', { hasText: 'E2E Hire Flow Co' })).toBeVisible();
  await personPage.locator('.conversation-item', { hasText: 'E2E Hire Flow Co' }).click();
  await expect(personPage.locator('.message-text', { hasText: companyReply })).toBeVisible();

  await companyContext.close();
  await personContext.close();

  // No user-delete API exists (by design - see CLAUDE.md) - throwaway users are prefixed
  // e2e-hireflow-*/"E2E Hire Flow *" precisely so they're trivial to purge afterwards via psql,
  // same convention as every other *.spec.ts in this folder.
});
