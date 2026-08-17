export interface AiChatJobReference {
  jobId: string;
  title: string;
  companyName: string;
}

export interface AiChatResponse {
  reply: string;
  jobIds: string[];
  referencedJobs: AiChatJobReference[];
}

export interface AiChatMessage {
  role: 'user' | 'assistant';
  content: string;
  referencedJobs?: AiChatJobReference[];
}
