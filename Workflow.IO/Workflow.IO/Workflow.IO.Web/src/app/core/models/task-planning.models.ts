import {
  IssueType,
  SprintResponse,
  TaskResolution,
  TaskResponse,
  TaskStatus,
} from './task.models';

/** Mirrors TaskApi.Model.Domain.Enums.TaskLinkType */
export enum TaskLinkType {
  Blocks = 1,
  IsBlockedBy = 2,
  RelatesTo = 3,
  Duplicates = 4,
  IsDuplicatedBy = 5,
}

export interface CreateComponentRequest {
  name: string;
  description?: string | null;
}

export interface ComponentResponse {
  componentId: string;
  projectId: string;
  name: string;
  description: string | null;
}

export interface CreateReleaseVersionRequest {
  name: string;
  description?: string | null;
  releaseDate?: string | null;
}

export interface ReleaseVersionResponse {
  releaseVersionId: string;
  projectId: string;
  name: string;
  isReleased: boolean;
  releaseDate: string | null;
}

export interface CreateTaskLinkRequest {
  targetTaskId: string;
  linkType: TaskLinkType;
}

export interface TaskLinkResponse {
  taskLinkId: string;
  sourceTaskId: string;
  targetTaskId: string;
  linkType: TaskLinkType;
  targetIssueKey: string | null;
}

export interface CreateWorkLogRequest {
  timeSpentMinutes: number;
  comment?: string | null;
  startedAt?: string | null;
}

export interface WorkLogResponse {
  workLogId: string;
  taskId: string;
  userId: string;
  timeSpentMinutes: number;
  comment: string | null;
  startedAt: string;
}

export interface UpdateBacklogRankRequest {
  backlogRank: number;
}

export interface BulkTaskUpdateRequest {
  taskIds: string[];
  status?: TaskStatus | null;
  resolution?: TaskResolution | null;
  assigneeId?: string | null;
  sprintId?: string | null;
  moveToBacklog?: boolean;
}

export interface BulkTaskUpdateResponse {
  updatedCount: number;
}

export interface CreateSavedFilterRequest {
  name: string;
  jqlQuery: string;
}

export interface SavedFilterResponse {
  savedFilterId: string;
  projectId: string | null;
  name: string;
  jqlQuery: string;
}

export interface UpdateSprintRequest {
  name?: string | null;
  startDate?: string | null;
  endDate?: string | null;
}

export type { SprintResponse, TaskResponse, IssueType };
