import { Injectable, inject, signal } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';

import { environment } from '../../../environments/environment';
import { RealtimeEvent } from '../models/realtime.models';
import { TokenStorageService } from './token-storage.service';

@Injectable({ providedIn: 'root' })
export class RealtimeService {
  private readonly tokenStorage = inject(TokenStorageService);

  private connection: HubConnection | null = null;

  readonly connected = signal(false);
  readonly latestEvent = signal<RealtimeEvent | null>(null);
  readonly events = signal<RealtimeEvent[]>([]);

  async connect(): Promise<void> {
    if (
      this.connection &&
      this.connection.state !== HubConnectionState.Disconnected
    ) {
      return;
    }

    this.connection = new HubConnectionBuilder()
      .withUrl(`${environment.apiGatewayUrl}/hubs/workflow.io`, {
        accessTokenFactory: () => this.tokenStorage.getAccessToken() ?? '',
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    const receive = (event: RealtimeEvent) => this.addEvent(event);

    this.connection.on('EventReceived', receive);
    this.connection.on('ProjectEventReceived', receive);
    this.connection.on('TaskEventReceived', receive);
    this.connection.on('NotificationReceived', receive);
    this.connection.onreconnected(() => this.connected.set(true));
    this.connection.onclose(() => this.connected.set(false));

    await this.connection.start();
    this.connected.set(true);
  }

  async joinProject(projectId: string): Promise<void> {
    await this.connect();
    await this.connection?.invoke('JoinProject', projectId);
  }

  async joinTask(taskId: string): Promise<void> {
    await this.connect();
    await this.connection?.invoke('JoinTask', taskId);
  }

  async disconnect(): Promise<void> {
    if (!this.connection) {
      return;
    }

    await this.connection.stop();
    this.connected.set(false);
  }

  private addEvent(event: RealtimeEvent): void {
    this.latestEvent.set(event);
    this.events.update((events) => [event, ...events].slice(0, 20));
  }
}
