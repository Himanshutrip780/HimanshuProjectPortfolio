/** Mirrors ProjectApi.Model.Domain.Enums.ProjectStatus (serialized as int). */
export enum ProjectStatus {
  Active = 0,
  Archived = 1,
  Completed = 2,
}

/** Mirrors ProjectApi.Model.Domain.Enums.ProjectType */
export enum ProjectType {
  Scrum = 1,
  Kanban = 2,
}

/** Mirrors ProjectApi.Model.Domain.Enums.ProjectRole (serialized as int). */
export enum ProjectRole {
  Owner = 0,
  Admin = 1,
  Member = 2,
  Viewer = 3,
}

/** Mirrors ProjectApi.Model.Dto.ProjectResponseDto */
export interface ProjectResponse {
  projectId: string;
  name: string;
  description: string | null;
  ownerId: string;
  key: string;
  projectType: ProjectType;
  status: ProjectStatus;
  createdAt: string;
}

/** Mirrors ProjectApi.Model.Dto.CreateProjectRequestDto */
export interface CreateProjectRequest {
  name: string;
  description?: string | null;
  key?: string | null;
  projectType?: ProjectType;
}

/** Mirrors ProjectApi.Model.Dto.UpdateProjectRequestDto */
export interface UpdateProjectRequest {
  name: string;
  description?: string | null;
}

/** Mirrors ProjectApi.Model.Dto.AddProjectMemberRequestDto */
export interface AddProjectMemberRequest {
  userId: string;
  role: ProjectRole | number;
}

/** Mirrors ProjectApi.Model.Dto.ChangeProjectMemberRoleRequestDto */
export interface ChangeProjectMemberRoleRequest {
  role: ProjectRole | number;
}

/** Mirrors ProjectApi.Model.Domain.Entities.ProjectMember */
export interface ProjectMember {
  projectMemberId: string;
  projectId: string;
  userId: string;
  role: ProjectRole;
  joinedAt: string;
}
