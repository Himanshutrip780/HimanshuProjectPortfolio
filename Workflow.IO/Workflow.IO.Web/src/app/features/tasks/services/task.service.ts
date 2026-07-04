import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import {
  AddTaskLabelRequest,
  AssignEpicRequest,
  AssignTaskRequest,
  BacklogResponse,
  BoardResponse,
  BoardViewResponse,
  ChangeSubTaskCompletionRequest,
  ChangeTaskStatusRequest,
  CreateBoardRequest,
  CreateEpicRequest,
  CreateSprintRequest,
  CreateSubTaskRequest,
  CreateTaskRequest,
  EpicResponse,
  MoveTaskToSprintRequest,
  SprintResponse,
  SubTaskResponse,
  TaskLabelResponse,
  TaskResponse,
  TaskSearchRequest,
  TaskWatcherResponse,
  UpdateStoryPointsRequest,
  UpdateTaskRequest,
} from '../../../core/models/task.models';
import { ApiHttpService } from '../../../core/services/api-http.service';
import { AuthService } from '../../../core/services/auth.service';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly api = inject(ApiHttpService);
  private readonly auth = inject(AuthService);

  getAllTasks(): Observable<TaskResponse[]> {
    return this.api.getList<TaskResponse>('/tasks');
  }

  createTask(
    projectId: string,
    request: CreateTaskRequest,
  ): Observable<TaskResponse> {
    return this.api.post<TaskResponse>(`/projects/${projectId}/tasks`, request);
  }

  getProjectTasks(projectId: string): Observable<TaskResponse[]> {
    return this.api.getList<TaskResponse>(`/projects/${projectId}/tasks`);
  }

  searchProjectTasks(
    projectId: string,
    criteria: TaskSearchRequest,
  ): Observable<TaskResponse[]> {
    return this.api.getList<TaskResponse>(
      `/projects/${projectId}/tasks/search`,
      criteria as Record<string, string | number | boolean | null | undefined>,
    );
  }

  createBoard(
    projectId: string,
    request: CreateBoardRequest,
  ): Observable<BoardResponse> {
    return this.api.post<BoardResponse>(
      `/projects/${projectId}/boards`,
      request,
    );
  }

  getBoardView(projectId: string): Observable<BoardViewResponse> {
    return this.api.get<BoardViewResponse>(`/projects/${projectId}/board`);
  }

  getBacklog(projectId: string): Observable<BacklogResponse> {
    return this.api.get<BacklogResponse>(`/projects/${projectId}/backlog`);
  }

  createSprint(
    projectId: string,
    request: CreateSprintRequest,
  ): Observable<SprintResponse> {
    return this.api.post<SprintResponse>(
      `/projects/${projectId}/sprints`,
      request,
    );
  }

  getProjectSprints(projectId: string): Observable<SprintResponse[]> {
    return this.api.getList<SprintResponse>(`/projects/${projectId}/sprints`);
  }

  createEpic(
    projectId: string,
    request: CreateEpicRequest,
  ): Observable<EpicResponse> {
    return this.api.post<EpicResponse>(`/projects/${projectId}/epics`, request);
  }

  getProjectEpics(projectId: string): Observable<EpicResponse[]> {
    return this.api.getList<EpicResponse>(`/projects/${projectId}/epics`);
  }

  getTaskById(taskId: string): Observable<TaskResponse> {
    return this.api.get<TaskResponse>(`/tasks/${taskId}`);
  }

  updateTask(
    taskId: string,
    request: UpdateTaskRequest,
  ): Observable<TaskResponse> {
    return this.api.put<TaskResponse>(`/tasks/${taskId}`, request);
  }

  changeStatus(
    taskId: string,
    request: ChangeTaskStatusRequest,
  ): Observable<TaskResponse> {
    return this.api.patch<TaskResponse>(`/tasks/${taskId}/status`, request);
  }

  assignTask(
    taskId: string,
    request: AssignTaskRequest,
  ): Observable<TaskResponse> {
    return this.api.patch<TaskResponse>(`/tasks/${taskId}/assign`, request);
  }

  moveToSprint(
    taskId: string,
    request: MoveTaskToSprintRequest,
  ): Observable<TaskResponse> {
    return this.api.patch<TaskResponse>(`/tasks/${taskId}/sprint`, request);
  }

  assignEpic(
    taskId: string,
    request: AssignEpicRequest,
  ): Observable<TaskResponse> {
    return this.api.patch<TaskResponse>(`/tasks/${taskId}/epic`, request);
  }

  updateStoryPoints(
    taskId: string,
    request: UpdateStoryPointsRequest,
  ): Observable<TaskResponse> {
    return this.api.patch<TaskResponse>(
      `/tasks/${taskId}/story-points`,
      request,
    );
  }

  deleteTask(taskId: string): Observable<void> {
    return this.api.delete(`/tasks/${taskId}`);
  }

  watchTask(taskId: string): Observable<TaskWatcherResponse> {
    return this.api.post<TaskWatcherResponse>(
      `/tasks/${taskId}/watchers/me`,
      null,
    );
  }

  getWatchers(taskId: string): Observable<TaskWatcherResponse[]> {
    return this.api.getList<TaskWatcherResponse>(`/tasks/${taskId}/watchers`);
  }

  unwatchTask(taskId: string): Observable<void> {
    return this.api.delete(`/tasks/${taskId}/watchers/me`);
  }

  addLabel(
    taskId: string,
    request: AddTaskLabelRequest,
  ): Observable<TaskLabelResponse> {
    return this.api.post<TaskLabelResponse>(
      `/tasks/${taskId}/labels`,
      request,
    );
  }

  getLabels(taskId: string): Observable<TaskLabelResponse[]> {
    return this.api.getList<TaskLabelResponse>(`/tasks/${taskId}/labels`);
  }

  removeLabel(taskId: string, labelId: string): Observable<void> {
    return this.api.delete(`/tasks/${taskId}/labels/${labelId}`);
  }

  createSubTask(
    taskId: string,
    request: CreateSubTaskRequest,
  ): Observable<SubTaskResponse> {
    return this.api.post<SubTaskResponse>(
      `/tasks/${taskId}/subtasks`,
      request,
    );
  }

  getSubTasks(taskId: string): Observable<SubTaskResponse[]> {
    return this.api.getList<SubTaskResponse>(`/tasks/${taskId}/subtasks`);
  }

  changeSubTaskCompletion(
    subTaskId: string,
    request: ChangeSubTaskCompletionRequest,
  ): Observable<SubTaskResponse> {
    return this.api.patch<SubTaskResponse>(
      `/subtasks/${subTaskId}/completion`,
      request,
    );
  }

  deleteSubTask(subTaskId: string): Observable<void> {
    return this.api.delete(`/subtasks/${subTaskId}`);
  }

  startSprint(sprintId: string): Observable<SprintResponse> {
    return this.api.patch<SprintResponse>(`/sprints/${sprintId}/start`, {});
  }

  completeSprint(sprintId: string): Observable<SprintResponse> {
    return this.api.patch<SprintResponse>(`/sprints/${sprintId}/complete`, {});
  }

  getDailyUpdateState(projectId: string): Observable<any> {
    return this.api.get<any>(`/tasks/projects/${projectId}/daily-update`);
  }

  sendDailyUpdate(projectId: string): Observable<any> {
    return this.api.post<any>(`/tasks/projects/${projectId}/daily-update/send`, {});
  }

  saveDailyUpdateRecipients(projectId: string, extraRecipients: string[]): Observable<any> {
    return this.api.post<any>(`/tasks/projects/${projectId}/daily-update/recipients`, { extraRecipients });
  }

  getChildTasks(taskId: string): Observable<TaskResponse[]> {
    return this.api.getList<TaskResponse>(`/tasks/${taskId}/child-tasks`);
  }

  createAutomationRule(projectId: string, request: any): Observable<any> {
    return this.api.post<any>(`/projects/${projectId}/automation-rules`, request);
  }

  getAutomationRules(projectId: string): Observable<any[]> {
    return this.api.getList<any>(`/projects/${projectId}/automation-rules`);
  }

  deleteAutomationRule(ruleId: string): Observable<void> {
    return this.api.delete(`/automation-rules/${ruleId}`);
  }

  toggleAutomationRule(ruleId: string, isEnabled: boolean): Observable<any> {
    return this.api.post<any>(`/automation-rules/${ruleId}/toggle?isEnabled=${isEnabled}`, {});
  }
}
