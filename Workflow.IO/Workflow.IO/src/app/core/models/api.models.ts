export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  correlationId?: string;
}

export interface AuthenticationResponse {
  email: string;
  jwtToken: string;
  refreshToken: string;
  expiresIn: number;
  role: string;
}

export interface RegisterUserRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface ProjectDto {
  projectId: string;
  name: string;
  description?: string;
  ownerId: string;
  status: string;
}

export interface TaskDto {
  taskId: string;
  projectId: string;
  title: string;
  description?: string;
  status: string;
  priority: string;
  assigneeId?: string;
  reporterId: string;
  sprintId?: string;
  epicId?: string;
  storyPoints?: number;
  dueDate?: string;
}

export interface BoardViewDto {
  board: { boardId: string; projectId: string; name: string };
  columns: Array<{
    column: { boardColumnId: string; name: string; status: string; sortOrder: number };
    tasks: TaskDto[];
  }>;
}
