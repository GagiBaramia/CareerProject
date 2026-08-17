export interface AppNotification {
  id: string;
  type: string;
  message: string;
  relatedEntityId: string | null;
  isRead: boolean;
  createdAt: string;
}
