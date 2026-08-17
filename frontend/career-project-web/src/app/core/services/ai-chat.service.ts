import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api-config';
import { AiChatResponse } from '../models/ai-chat.models';

@Injectable({ providedIn: 'root' })
export class AiChatService {
  private readonly http = inject(HttpClient);

  sendMessage(message: string): Observable<AiChatResponse> {
    return this.http.post<AiChatResponse>(`${API_BASE_URL}/api/ai/chat`, { message });
  }
}
