import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api-config';
import { ConversationSummary, DirectMessage } from '../models/conversation.models';

@Injectable({ providedIn: 'root' })
export class ConversationService {
  private readonly http = inject(HttpClient);

  getConversations(): Observable<ConversationSummary[]> {
    return this.http.get<ConversationSummary[]>(`${API_BASE_URL}/api/conversations`);
  }

  getMessages(conversationId: string): Observable<DirectMessage[]> {
    return this.http.get<DirectMessage[]>(`${API_BASE_URL}/api/conversations/${conversationId}/messages`);
  }

  sendMessage(conversationId: string, content: string): Observable<DirectMessage> {
    return this.http.post<DirectMessage>(`${API_BASE_URL}/api/conversations/${conversationId}/messages`, { content });
  }
}
