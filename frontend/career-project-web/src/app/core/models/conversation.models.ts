export interface ConversationSummary {
  id: string;
  applicationId: string;
  jobId: string;
  jobTitle: string;
  otherPartyUserId: string;
  otherPartyName: string;
  otherPartyImageUrl: string | null;
  lastMessagePreview: string | null;
  lastMessageAt: string | null;
  unreadCount: number;
}

export interface DirectMessage {
  id: string;
  conversationId: string;
  senderUserId: string;
  content: string;
  createdAt: string;
  isRead: boolean;
}
