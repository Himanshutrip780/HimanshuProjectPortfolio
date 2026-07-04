export interface MetricBucket {
  name: string;
  count: number;
}

export interface ProjectAnalyticsSummary {
  projectId: string;
  totalTasks: number;
  completedTasks: number;
  overdueTasks: number;
  unassignedTasks: number;
  totalStoryPoints: number;
  completedStoryPoints: number;
  completionRate: number;
  tasksByStatus: MetricBucket[];
  tasksByPriority: MetricBucket[];
}

export interface SprintVelocity {
  sprintId: string;
  totalTasks: number;
  completedTasks: number;
  totalStoryPoints: number;
  completedStoryPoints: number;
  completionRate: number;
}

export interface ActivityTrend {
  date: string;
  count: number;
}

export interface SprintBurndownPoint {
  date: string;
  remainingStoryPoints: number;
  idealRemainingStoryPoints: number;
}

export interface SprintBurndown {
  sprintId: string;
  totalStoryPoints: number;
  points: SprintBurndownPoint[];
}
