import { DatePipe } from '@angular/common';
import { Component, OnDestroy, OnInit, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ConversationService } from '../../core/services/conversation.service';
import { ChatHubService } from '../../core/services/chat-hub.service';
import { ConversationSummary, DirectMessage } from '../../core/models/conversation.models';
import { AvatarComponent } from '../../shared/components/avatar/avatar.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { SidebarNavComponent } from '../../shared/components/sidebar-nav/sidebar-nav.component';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [DatePipe, FormsModule, AvatarComponent, EmptyStateComponent, SidebarNavComponent],
  templateUrl: './messages.component.html',
  styleUrl: './messages.component.css'
})
export class MessagesComponent implements OnInit, OnDestroy {
  private readonly conversationService = inject(ConversationService);
  private readonly chatHub = inject(ChatHubService);
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);

  readonly isLoadingConversations = signal(true);
  readonly conversationsError = signal<string | null>(null);
  readonly conversations = signal<ConversationSummary[]>([]);

  readonly selectedConversationId = signal<string | null>(null);
  readonly messages = signal<DirectMessage[]>([]);
  readonly isLoadingMessages = signal(false);
  readonly messagesError = signal<string | null>(null);

  readonly messageInput = signal('');
  readonly isSending = signal(false);

  readonly currentUserId = computed(() => this.auth.currentUser()?.userId ?? '');

  readonly selectedConversation = computed(
    () => this.conversations().find((c) => c.id === this.selectedConversationId()) ?? null
  );

  constructor() {
    // Own messages are already appended synchronously from the REST response - only append
    // incoming realtime messages sent by the *other* party, or we'd see our own message twice.
    effect(() => {
      const message = this.chatHub.messageReceived();
      if (!message) {
        return;
      }

      const isOwnMessage = message.senderUserId === this.currentUserId();

      if (message.conversationId === this.selectedConversationId() && !isOwnMessage) {
        this.messages.update((msgs) => [...msgs, message]);
      }

      this.conversations.update((convs) =>
        convs.map((c) =>
          c.id === message.conversationId
            ? {
                ...c,
                lastMessagePreview: message.content,
                lastMessageAt: message.createdAt,
                unreadCount:
                  isOwnMessage || c.id === this.selectedConversationId() ? c.unreadCount : c.unreadCount + 1
              }
            : c
        )
      );
    });
  }

  ngOnInit(): void {
    this.chatHub.connect().catch(() => {
      /* connectionState signal already reflects the failure - UI shows it. */
    });

    this.conversationService.getConversations().subscribe({
      next: (conversations) => {
        this.conversations.set(conversations);
        this.isLoadingConversations.set(false);

        const requestedId = this.route.snapshot.queryParamMap.get('conversationId');
        // On mobile, list and chat are separate panes (see .mobile-show-chat) - auto-opening
        // the first conversation would land the user straight in a chat with the list hidden.
        // Desktop shows both panes at once, so auto-selecting there is still helpful.
        const isMobileViewport = window.innerWidth <= 768;
        const initialId =
          requestedId && conversations.some((c) => c.id === requestedId)
            ? requestedId
            : isMobileViewport
              ? null
              : (conversations[0]?.id ?? null);

        if (initialId) {
          this.selectConversation(initialId);
        }
      },
      error: () => {
        this.conversationsError.set('შეტყობინებების ჩატვირთვისას მოხდა შეცდომა.');
        this.isLoadingConversations.set(false);
      }
    });
  }

  ngOnDestroy(): void {
    void this.chatHub.disconnect();
  }

  selectConversation(conversationId: string): void {
    this.selectedConversationId.set(conversationId);
    this.isLoadingMessages.set(true);
    this.messagesError.set(null);

    this.chatHub.joinConversation(conversationId).catch(() => {});

    this.conversationService.getMessages(conversationId).subscribe({
      next: (messages) => {
        this.messages.set(messages);
        this.isLoadingMessages.set(false);
        // Loading history marks the other party's messages read server-side - mirror it locally.
        this.conversations.update((convs) =>
          convs.map((c) => (c.id === conversationId ? { ...c, unreadCount: 0 } : c))
        );
      },
      error: () => {
        this.messagesError.set('შეტყობინებების ჩატვირთვისას მოხდა შეცდომა.');
        this.isLoadingMessages.set(false);
      }
    });
  }

  // Mobile-only "← back" button in the chat header - on desktop both panes are always
  // visible side by side, so this is unreachable there (see .back-to-list-btn CSS).
  deselectConversation(): void {
    this.selectedConversationId.set(null);
  }

  sendMessage(): void {
    const content = this.messageInput().trim();
    const conversationId = this.selectedConversationId();
    if (!content || !conversationId || this.isSending()) {
      return;
    }

    this.isSending.set(true);

    this.conversationService.sendMessage(conversationId, content).subscribe({
      next: (message) => {
        this.messages.update((msgs) => [...msgs, message]);
        this.messageInput.set('');
        this.isSending.set(false);
      },
      error: () => {
        this.isSending.set(false);
      }
    });
  }
}
