import { APIRequestContext, Page } from '@playwright/test';

// The API Gateway - Angular talks to this, not the individual backend services directly.
export const API_BASE_URL = 'http://localhost:5178';

// Every generated email/title starts with "e2e-" (or "E2E ..." for job titles) precisely so
// throwaway data from repeated test runs is trivial to find and purge from the shared dev DB,
// e.g. DELETE FROM "Users" WHERE "Email" LIKE 'e2e-%' (cascades to profiles/companies/jobs).
export function uniqueEmail(prefix: string): string {
  return `${prefix}-${Date.now()}-${Math.random().toString(36).slice(2, 8)}@example.com`;
}

export interface AuthResult {
  token: string;
  userId: string;
  email: string;
  role: string;
  displayName: string;
}

// Registers a Person directly against the real API (bypassing the registration UI) - used by
// specs whose scenario is something *downstream* of registration (profile, recommendations,
// applying), so each spec stays focused on the one flow it's actually testing.
export async function registerPersonViaApi(
  request: APIRequestContext,
  email: string,
  fullName: string
): Promise<AuthResult> {
  const response = await request.post(`${API_BASE_URL}/api/auth/register/person`, {
    data: { email, password: 'correct-password', fullName }
  });
  return response.json();
}

export async function registerCompanyViaApi(
  request: APIRequestContext,
  email: string,
  companyName: string
): Promise<AuthResult> {
  const response = await request.post(`${API_BASE_URL}/api/auth/register/company`, {
    data: { email, password: 'correct-password', companyName }
  });
  return response.json();
}

export async function setPersonProfileViaApi(
  request: APIRequestContext,
  token: string,
  body: { fullName: string; headline?: string; cvSummary?: string; location?: string }
): Promise<void> {
  await request.put(`${API_BASE_URL}/api/profile/me`, {
    headers: { Authorization: `Bearer ${token}` },
    data: body
  });
}

// Seeds localStorage before any app script runs (via addInitScript, not page.evaluate after
// goto) so AuthService picks up the session on its very first read, same key/shape it writes
// itself (see AuthService's STORAGE_KEY).
export async function injectAuth(page: Page, auth: AuthResult): Promise<void> {
  await page.addInitScript((authData) => {
    window.localStorage.setItem('career_project_auth', JSON.stringify(authData));
  }, auth);
}
