import { Injectable, inject, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { API_BASE_URL } from '../config/api-config';
import { DirectMessage } from '../models/conversation.models';
import { AuthService } from './auth.service';

// Realtime delivery only - REST (ConversationService) still owns history/loading. One shared
// connection per session; components join/leave conversation groups as they open/close them.
@Injectable({ providedIn: 'root' })
export class ChatHubService {
  private readonly auth = inject(AuthService);
  private connection: signalR.HubConnection | null = null;

  // Tracks the in-flight start() call so joinConversation() can wait on the *same* connection
  // attempt instead of racing it - invoking a hub method before start() resolves throws, and a
  // caller that swallows that error (e.g. .catch(() => {})) would silently never actually join.
  private connectPromise: Promise<void> | null = null;

  readonly messageReceived = signal<DirectMessage | null>(null);
  readonly connectionState = signal<'disconnected' | 'connecting' | 'connected'>('disconnected');

  connect(): Promise<void> {
    if (this.connectPromise) {
      return this.connectPromise;
    }

    this.connectionState.set('connecting');

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hub/chat`, {
        accessTokenFactory: () => this.auth.getToken() ?? ''
      })
      .withAutomaticReconnect()
      .build();

    this.connection.on('ReceiveMessage', (message: DirectMessage) => {
      this.messageReceived.set(message);
    });

    this.connection.onreconnected(() => this.connectionState.set('connected'));
    this.connection.onreconnecting(() => this.connectionState.set('connecting'));
    this.connection.onclose(() => this.connectionState.set('disconnected'));

    this.connectPromise = this.connection.start().then(() => {
      this.connectionState.set('connected');
    });

    return this.connectPromise;
  }

  async joinConversation(conversationId: string): Promise<void> {
    if (this.connectPromise) {
      await this.connectPromise;
    }

    await this.connection?.invoke('JoinConversation', conversationId);
  }

  async disconnect(): Promise<void> {
    await this.connection?.stop();
    this.connection = null;
    this.connectPromise = null;
    this.connectionState.set('disconnected');
  }
}
