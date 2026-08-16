import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api-config';
import { Skill } from '../models/profile.models';

@Injectable({ providedIn: 'root' })
export class SkillsService {
  private readonly http = inject(HttpClient);

  search(query: string): Observable<Skill[]> {
    const params = query ? new HttpParams().set('search', query) : undefined;
    return this.http.get<Skill[]>(`${API_BASE_URL}/api/skills`, { params });
  }
}
