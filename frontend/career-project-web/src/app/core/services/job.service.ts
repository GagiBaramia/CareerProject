import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api-config';
import { CreateJobRequest, JobResponse } from '../models/job.models';

@Injectable({ providedIn: 'root' })
export class JobService {
  private readonly http = inject(HttpClient);

  createJob(request: CreateJobRequest): Observable<JobResponse> {
    return this.http.post<JobResponse>(`${API_BASE_URL}/api/jobs`, request);
  }

  getJobs(): Observable<JobResponse[]> {
    return this.http.get<JobResponse[]>(`${API_BASE_URL}/api/jobs`);
  }

  applyToJob(jobId: string): Observable<unknown> {
    return this.http.post(`${API_BASE_URL}/api/jobs/${jobId}/apply`, {});
  }
}
