/** Mirrors TaskApi.Model.Domain.Enums.TaskStatus */
export enum TaskStatus {
  Todo = 1,
  InProgress = 2,
  Review = 3,
  Done = 4,
  Blocked = 5,
}

/** Mirrors TaskApi.Model.Domain.Enums.TaskPriority */
export enum TaskPriority {
  Low = 1,
  Medium = 2,
  High = 3,
  Critical = 4,
}

/** Mirrors TaskApi.Model.Domain.Enums.SprintStatus */
export enum SprintStatus {
  Planned = 1,
  Active = 2,
  Completed = 3,
}

/** Mirrors TaskApi.Model.Domain.Enums.IssueType */
export enum IssueType {
  Story = 1,
  Task = 2,
  Bug = 3,
  SubTask = 4,
}

/** Mirrors TaskApi.Model.Domain.Enums.TaskResolution */
export enum TaskResolution {
  Fixed = 1,
  WontDo = 2,
  Duplicate = 3,
  CannotReproduce = 4,
  Done = 5,
}

/** Mirrors TaskApi.Model.Dto.TaskResponseDto */
export interface TaskResponse {
  taskId: string;
  projectId: string;
  issueNumber: number;
  issueKey: string;
  issueType: IssueType;
  title: string;
  description: string | null;
  status: TaskStatus;
  resolution: TaskResolution | null;
  priority: TaskPriority;
  parentTaskId: string | null;
  componentId: string | null;
  fixVersionId: string | null;
  originalEstimateMinutes: number | null;
  remainingEstimateMinutes: number | null;
  backlogRank: number;
  totalLoggedMinutes: number;
  assigneeId: string | null;
  reporterId: string;
  sprintId: string | null;
  epicId: string | null;
  storyPoints: number | null;
  dueDate: string | null;
  createdAt: string;
  updatedAt: string;
  isOverdue: boolean;
  feDeveloper?: string | null;
  beDeveloper?: string | null;
  qaEngineer?: string | null;
  initialEta?: string | null;
  latestEta?: string | null;
  teamId?: string | null;
}

/** Mirrors TaskApi.Model.Dto.CreateTaskRequestDto */
export interface CreateTaskRequest {
  title: string;
  description?: string | null;
  priority: TaskPriority;
  issueType?: IssueType;
  assigneeId?: string | null;
  dueDate?: string | null;
  parentTaskId?: string | null;
  feDeveloper?: string | null;
  beDeveloper?: string | null;
  qaEngineer?: string | null;
  initialEta?: string | null;
  latestEta?: string | null;
  teamId?: string | null;
}

/** Mirrors TaskApi.Model.Dto.UpdateTaskRequestDto */
export interface UpdateTaskRequest {
  title: string;
  description?: string | null;
  priority: TaskPriority;
  dueDate?: string | null;
  parentTaskId?: string | null;
  feDeveloper?: string | null;
  beDeveloper?: string | null;
  qaEngineer?: string | null;
  initialEta?: string | null;
  latestEta?: string | null;
  teamId?: string | null;
}

/** Mirrors TaskApi.Model.Dto.ChangeTaskStatusRequestDto */
export interface ChangeTaskStatusRequest {
  status: TaskStatus;
  resolution?: TaskResolution | null;
  transitionComment?: string | null;
}

/** Mirrors TaskApi.Model.Dto.AssignTaskRequestDto */
export interface AssignTaskRequest {
  assigneeId: string | null;
}

/** Mirrors TaskApi.Model.Dto.TaskSearchRequestDto (query params) */
export interface TaskSearchRequest {
  query?: string | null;
  status?: TaskStatus | null;
  priority?: TaskPriority | null;
  assigneeId?: string | null;
  reporterId?: string | null;
  sprintId?: string | null;
  epicId?: string | null;
  isOverdue?: boolean | null;
  teamId?: string | null;
}

/** Mirrors TaskApi.Model.Dto.CreateBoardRequestDto */
export interface CreateBoardRequest {
  name: string;
}

/** Mirrors TaskApi.Model.Dto.BoardResponseDto */
export interface BoardResponse {
  boardId: string;
  projectId: string;
  name: string;
  createdAt: string;
  updatedAt: string;
}

/** Mirrors TaskApi.Model.Dto.BoardColumnResponseDto */
export interface BoardColumnResponse {
  boardColumnId: string;
  boardId: string;
  name: string;
  status: TaskStatus;
  sortOrder: number;
}

/** Mirrors TaskApi.Model.Dto.BoardColumnViewDto */
export interface BoardColumnView {
  column: BoardColumnResponse;
  tasks: TaskResponse[];
}

/** Mirrors TaskApi.Model.Dto.BoardViewResponseDto */
export interface BoardViewResponse {
  board: BoardResponse;
  columns: BoardColumnView[];
}

/** Mirrors TaskApi.Model.Dto.CreateSprintRequestDto */
export interface CreateSprintRequest {
  name: string;
  startDate: string;
  endDate: string;
}

/** Mirrors TaskApi.Model.Dto.SprintResponseDto */
export interface SprintResponse {
  sprintId: string;
  projectId: string;
  name: string;
  startDate: string;
  endDate: string;
  status: SprintStatus;
  createdAt: string;
  updatedAt: string;
}

/** Mirrors TaskApi.Model.Dto.SprintBacklogResponseDto */
export interface SprintBacklogResponse {
  sprint: SprintResponse;
  tasks: TaskResponse[];
}

/** Mirrors TaskApi.Model.Dto.BacklogResponseDto */
export interface BacklogResponse {
  projectId: string;
  backlogTasks: TaskResponse[];
  sprints: SprintBacklogResponse[];
}

/** Mirrors TaskApi.Model.Dto.CreateEpicRequestDto */
export interface CreateEpicRequest {
  name: string;
  description?: string | null;
}

/** Mirrors TaskApi.Model.Dto.EpicResponseDto */
export interface EpicResponse {
  epicId: string;
  projectId: string;
  name: string;
  description: string | null;
  createdAt: string;
  updatedAt: string;
}

/** Mirrors TaskApi.Model.Dto.MoveTaskToSprintRequestDto */
export interface MoveTaskToSprintRequest {
  sprintId: string | null;
}

/** Mirrors TaskApi.Model.Dto.AssignEpicRequestDto */
export interface AssignEpicRequest {
  epicId: string | null;
}

/** Mirrors TaskApi.Model.Dto.UpdateStoryPointsRequestDto */
export interface UpdateStoryPointsRequest {
  storyPoints: number | null;
}

/** Mirrors TaskApi.Model.Dto.AddTaskLabelRequestDto */
export interface AddTaskLabelRequest {
  name: string;
  color?: string | null;
}

/** Mirrors TaskApi.Model.Dto.TaskLabelResponseDto */
export interface TaskLabelResponse {
  taskLabelId: string;
  taskId: string;
  name: string;
  color: string | null;
  createdAt: string;
}

/** Mirrors TaskApi.Model.Dto.CreateSubTaskRequestDto */
export interface CreateSubTaskRequest {
  title: string;
}

/** Mirrors TaskApi.Model.Dto.SubTaskResponseDto */
export interface SubTaskResponse {
  subTaskId: string;
  taskId: string;
  title: string;
  isCompleted: boolean;
  createdAt: string;
  updatedAt: string;
}

/** Mirrors TaskApi.Model.Dto.ChangeSubTaskCompletionRequestDto */
export interface ChangeSubTaskCompletionRequest {
  isCompleted: boolean;
}

/** Mirrors TaskApi.Model.Dto.TaskWatcherResponseDto */
export interface TaskWatcherResponse {
  taskWatcherId: string;
  taskId: string;
  userId: string;
  createdAt: string;
}
