import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api-config';
import { ApplicationStatus, JobApplication } from '../models/application.models';

@Injectable({ providedIn: 'root' })
export class ApplicationService {
  private readonly http = inject(HttpClient);

  getMyApplications(): Observable<JobApplication[]> {
    return this.http.get<JobApplication[]>(`${API_BASE_URL}/api/applications/mine`);
  }

  getCompanyApplications(): Observable<JobApplication[]> {
    return this.http.get<JobApplication[]>(`${API_BASE_URL}/api/company/applications`);
  }

  getApplicationsForJob(jobId: string): Observable<JobApplication[]> {
    return this.http.get<JobApplication[]>(`${API_BASE_URL}/api/company/jobs/${jobId}/applications`);
  }

  updateStatus(applicationId: string, status: ApplicationStatus): Observable<JobApplication> {
    return this.http.patch<JobApplication>(`${API_BASE_URL}/api/applications/${applicationId}/status`, { status });
  }
}
