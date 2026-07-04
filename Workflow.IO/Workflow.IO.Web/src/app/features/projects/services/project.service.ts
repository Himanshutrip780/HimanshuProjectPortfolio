import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import {
  AddProjectMemberRequest,
  ChangeProjectMemberRoleRequest,
  CreateProjectRequest,
  ProjectMember,
  ProjectResponse,
  UpdateProjectRequest,
} from '../../../core/models/project.models';
import { ApiHttpService } from '../../../core/services/api-http.service';

@Injectable({ providedIn: 'root' })
export class ProjectService {
  private readonly api = inject(ApiHttpService);

  createProject(request: CreateProjectRequest): Observable<ProjectResponse> {
    return this.api.post<ProjectResponse>('/projects', request);
  }

  importProject(request: any): Observable<ProjectResponse> {
    return this.api.post<ProjectResponse>('/projects/import', request);
  }

  getMyProjects(): Observable<ProjectResponse[]> {
    return this.api.getList<ProjectResponse>('/projects');
  }

  getProjectById(projectId: string): Observable<ProjectResponse> {
    return this.api.get<ProjectResponse>(`/projects/${projectId}`);
  }

  getProjectByKey(projectKey: string): Observable<ProjectResponse> {
    return this.api.get<ProjectResponse>(`/projects/key/${projectKey}`);
  }

  getMyMembership(projectId: string): Observable<ProjectMember> {
    return this.api.get<ProjectMember>(`/projects/${projectId}/members/me`);
  }

  updateProject(
    projectId: string,
    request: UpdateProjectRequest,
  ): Observable<ProjectResponse> {
    return this.api.put<ProjectResponse>(`/projects/${projectId}`, request);
  }

  deleteProject(projectId: string): Observable<void> {
    return this.api.delete(`/projects/${projectId}`);
  }

  archiveProject(projectId: string): Observable<void> {
    return this.api.patchCommand(`/projects/${projectId}/archive`, {});
  }

  addMember(
    projectId: string,
    request: AddProjectMemberRequest,
  ): Observable<void> {
    return this.api.postCommand(`/projects/${projectId}/members`, request);
  }

  getMembers(projectId: string): Observable<ProjectMember[]> {
    return this.api.getList<ProjectMember>(`/projects/${projectId}/members`);
  }

  removeMember(projectId: string, userId: string): Observable<void> {
    return this.api.delete(`/projects/${projectId}/members/${userId}`);
  }

  changeMemberRole(
    projectId: string,
    userId: string,
    request: ChangeProjectMemberRoleRequest,
  ): Observable<void> {
    return this.api.patchCommand(
      `/projects/${projectId}/members/${userId}/role`,
      request,
    );
  }
}
