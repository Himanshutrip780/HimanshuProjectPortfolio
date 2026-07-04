export interface RealtimeEvent {
  eventType: string;
  entityType: string;
  entityId: string;
  projectId: string | null;
  taskId: string | null;
  actorId: string | null;
  recipientId: string | null;
  description: string | null;
  payloadJson: string | null;
  occurredAt: string;
}
