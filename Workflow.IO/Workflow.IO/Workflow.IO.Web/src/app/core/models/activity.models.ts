export interface ActivityRecord {
  activityRecordId: string;
  eventType: string;
  entityType: string;
  entityId: string;
  actorId: string | null;
  description: string | null;
  payloadJson: string | null;
  createdAt: string;
}
