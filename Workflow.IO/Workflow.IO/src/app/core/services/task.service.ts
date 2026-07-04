import { Injectable, inject } from '@angular/core';

import { BoardViewDto, TaskDto } from '../models/api.models';
import { ApiClientService } from './api-client.service';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly api = inject(ApiClientService);

  listByProject(projectId: string) {
    return this.api.get<TaskDto[]>(`/projects/${projectId}/tasks`);
  }

  getBoard(projectId: string) {
    return this.api.get<BoardViewDto>(`/projects/${projectId}/board`);
  }

  getBacklog(projectId: string) {
    return this.api.get<unknown>(`/projects/${projectId}/backlog`);
  }

  create(
    projectId: string,
    payload: {
      title: string;
      description?: string;
      priority: string;
      assigneeId?: string;
      dueDate?: string;
    },
  ) {
    return this.api.post<TaskDto>(`/projects/${projectId}/tasks`, payload);
  }

  changeStatus(taskId: string, status: string) {
    return this.api.patch<TaskDto>(`/tasks/${taskId}/status`, { status });
  }

  moveToSprint(taskId: string, sprintId: string | null) {
    return this.api.patch<TaskDto>(`/tasks/${taskId}/sprint`, { sprintId });
  }
}
