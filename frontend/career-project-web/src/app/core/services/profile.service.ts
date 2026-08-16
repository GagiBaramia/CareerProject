import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api-config';
import { ProfileResponse, UpdateProfileRequest, UpdateProfileSkillsRequest } from '../models/profile.models';

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private readonly http = inject(HttpClient);

  getMyProfile(): Observable<ProfileResponse> {
    return this.http.get<ProfileResponse>(`${API_BASE_URL}/api/profile/me`);
  }

  updateMyProfile(request: UpdateProfileRequest): Observable<ProfileResponse> {
    return this.http.put<ProfileResponse>(`${API_BASE_URL}/api/profile/me`, request);
  }

  updateMySkills(request: UpdateProfileSkillsRequest): Observable<ProfileResponse> {
    return this.http.put<ProfileResponse>(`${API_BASE_URL}/api/profile/me/skills`, request);
  }
}
