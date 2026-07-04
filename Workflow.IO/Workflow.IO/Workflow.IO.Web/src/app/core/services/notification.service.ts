import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { Notification } from '../models/notification.models';
import { ApiHttpService } from './api-http.service';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly api = inject(ApiHttpService);

  getMyNotifications(unreadOnly = false): Observable<Notification[]> {
    return this.api.getList<Notification>('/notifications/me', { unreadOnly });
  }

  getUnreadCount(): Observable<number> {
    return this.api
      .get<{ count: number }>('/notifications/me/unread-count')
      .pipe(map((payload) => payload.count ?? 0));
  }

  markAsRead(notificationId: string): Observable<void> {
    return this.api.patchCommand(`/notifications/${notificationId}/read`, {});
  }

  markAllAsRead(): Observable<void> {
    return this.api.patchCommand('/notifications/me/read-all', {});
  }
}
