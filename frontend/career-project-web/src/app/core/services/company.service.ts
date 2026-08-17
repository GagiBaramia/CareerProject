import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api-config';
import { CompanyProfile, UpdateCompanyProfileRequest } from '../models/company.models';

@Injectable({ providedIn: 'root' })
export class CompanyService {
  private readonly http = inject(HttpClient);

  getMyCompany(): Observable<CompanyProfile> {
    return this.http.get<CompanyProfile>(`${API_BASE_URL}/api/company/me`);
  }

  updateMyCompany(request: UpdateCompanyProfileRequest): Observable<CompanyProfile> {
    return this.http.put<CompanyProfile>(`${API_BASE_URL}/api/company/me`, request);
  }
}
