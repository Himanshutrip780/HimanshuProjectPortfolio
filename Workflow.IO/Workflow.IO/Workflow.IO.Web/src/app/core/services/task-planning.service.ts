import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import {
  BulkTaskUpdateRequest,
  BulkTaskUpdateResponse,
  ComponentResponse,
  CreateComponentRequest,
  CreateReleaseVersionRequest,
  CreateSavedFilterRequest,
  CreateTaskLinkRequest,
  CreateWorkLogRequest,
  ReleaseVersionResponse,
  SavedFilterResponse,
  TaskLinkResponse,
  UpdateBacklogRankRequest,
  UpdateSprintRequest,
  WorkLogResponse,
} from '../models/task-planning.models';
import { SprintResponse, TaskResponse } from '../models/task.models';
import { ApiHttpService } from './api-http.service';

@Injectable({ providedIn: 'root' })
export class TaskPlanningService {
  private readonly api = inject(ApiHttpService);

  getByIssueKey(issueKey: string): Observable<TaskResponse> {
    return this.api.get<TaskResponse>(`/issues/${issueKey}`);
  }

  updateRank(
    taskId: string,
    request: UpdateBacklogRankRequest,
  ): Observable<TaskResponse> {
    return this.api.patch<TaskResponse>(`/tasks/${taskId}/rank`, request);
  }

  createComponent(
    projectId: string,
    request: CreateComponentRequest,
  ): Observable<ComponentResponse> {
    return this.api.post<ComponentResponse>(
      `/projects/${projectId}/components`,
      request,
    );
  }

  getComponents(projectId: string): Observable<ComponentResponse[]> {
    return this.api.getList<ComponentResponse>(
      `/projects/${projectId}/components`,
    );
  }

  createVersion(
    projectId: string,
    request: CreateReleaseVersionRequest,
  ): Observable<ReleaseVersionResponse> {
    return this.api.post<ReleaseVersionResponse>(
      `/projects/${projectId}/versions`,
      request,
    );
  }

  getVersions(projectId: string): Observable<ReleaseVersionResponse[]> {
    return this.api.getList<ReleaseVersionResponse>(
      `/projects/${projectId}/versions`,
    );
  }

  releaseVersion(versionId: string): Observable<ReleaseVersionResponse> {
    return this.api.patch<ReleaseVersionResponse>(
      `/versions/${versionId}/release`,
      {},
    );
  }

  createLink(
    taskId: string,
    request: CreateTaskLinkRequest,
  ): Observable<TaskLinkResponse> {
    return this.api.post<TaskLinkResponse>(`/tasks/${taskId}/links`, request);
  }

  getLinks(taskId: string): Observable<TaskLinkResponse[]> {
    return this.api.getList<TaskLinkResponse>(`/tasks/${taskId}/links`);
  }

  deleteLink(linkId: string): Observable<void> {
    return this.api.delete(`/links/${linkId}`);
  }

  addWorkLog(
    taskId: string,
    request: CreateWorkLogRequest,
  ): Observable<WorkLogResponse> {
    return this.api.post<WorkLogResponse>(`/tasks/${taskId}/worklogs`, request);
  }

  getWorkLogs(taskId: string): Observable<WorkLogResponse[]> {
    return this.api.getList<WorkLogResponse>(`/tasks/${taskId}/worklogs`);
  }

  bulkUpdate(
    projectId: string,
    request: BulkTaskUpdateRequest,
  ): Observable<BulkTaskUpdateResponse> {
    return this.api.post<BulkTaskUpdateResponse>(
      `/projects/${projectId}/tasks/bulk`,
      request,
    );
  }

  createFilter(
    projectId: string,
    request: CreateSavedFilterRequest,
  ): Observable<SavedFilterResponse> {
    return this.api.post<SavedFilterResponse>(
      `/projects/${projectId}/filters`,
      request,
    );
  }

  getProjectFilters(projectId: string): Observable<SavedFilterResponse[]> {
    return this.api.getList<SavedFilterResponse>(
      `/projects/${projectId}/filters`,
    );
  }

  getMyFilters(): Observable<SavedFilterResponse[]> {
    return this.api.getList<SavedFilterResponse>('/filters');
  }

  executeFilter(filterId: string): Observable<TaskResponse[]> {
    return this.api.getList<TaskResponse>(`/filters/${filterId}/results`);
  }

  updateSprint(
    sprintId: string,
    request: UpdateSprintRequest,
  ): Observable<SprintResponse> {
    return this.api.put<SprintResponse>(`/sprints/${sprintId}`, request);
  }

  deleteSprint(sprintId: string): Observable<void> {
    return this.api.delete(`/sprints/${sprintId}`);
  }
}
