import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api-config';
import { AppNotification } from '../models/notification.models';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly http = inject(HttpClient);

  getMyNotifications(): Observable<AppNotification[]> {
    return this.http.get<AppNotification[]>(`${API_BASE_URL}/api/notifications`);
  }

  markAsRead(id: string): Observable<AppNotification> {
    return this.http.patch<AppNotification>(`${API_BASE_URL}/api/notifications/${id}/read`, {});
  }
}
