import { Injectable, inject } from '@angular/core';

import { ProjectDto } from '../models/api.models';
import { ApiClientService } from './api-client.service';

@Injectable({ providedIn: 'root' })
export class ProjectService {
  private readonly api = inject(ApiClientService);

  listMine() {
    return this.api.get<ProjectDto[]>('/projects');
  }

  getById(projectId: string) {
    return this.api.get<ProjectDto>(`/projects/${projectId}`);
  }

  create(name: string, description?: string) {
    return this.api.post<ProjectDto>('/projects', { name, description });
  }
}
