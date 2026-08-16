import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api-config';
import { JobRecommendation } from '../models/recommendation.models';

@Injectable({ providedIn: 'root' })
export class RecommendationService {
  private readonly http = inject(HttpClient);

  getRecommendedJobs(): Observable<JobRecommendation[]> {
    return this.http.get<JobRecommendation[]>(`${API_BASE_URL}/api/recommendations/jobs`);
  }
}
