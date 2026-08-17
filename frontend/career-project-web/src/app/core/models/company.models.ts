export interface CompanyProfile {
  id: string;
  name: string;
  description: string | null;
  industry: string | null;
  logoUrl: string | null;
}

export interface UpdateCompanyProfileRequest {
  name: string;
  description: string | null;
  industry: string | null;
  logoUrl: string | null;
}
