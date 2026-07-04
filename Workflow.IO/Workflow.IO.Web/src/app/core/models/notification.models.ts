export interface Notification {
  notificationId: string;
  recipientId: string | null;
  eventType: string;
  entityType: string;
  entityId: string;
  message: string;
  isRead: boolean;
  createdAt: string;
}

export interface UnreadNotificationCount {
  count: number;
}

export interface MarkAllNotificationsReadResponse {
  updated: number;
}
