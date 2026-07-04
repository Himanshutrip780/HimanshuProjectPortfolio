import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import {
  ActivityTrend,
  MetricBucket,
  ProjectAnalyticsSummary,
  SprintBurndown,
  SprintVelocity,
} from '../models/analytics.models';
import { ApiHttpService } from './api-http.service';

@Injectable({ providedIn: 'root' })
export class AnalyticsService {
  private readonly api = inject(ApiHttpService);

  getProjectSummary(projectId: string): Observable<ProjectAnalyticsSummary> {
    return this.api.get<ProjectAnalyticsSummary>(
      `/analytics/projects/${projectId}/summary`,
    );
  }

  getTasksByStatus(projectId: string): Observable<MetricBucket[]> {
    return this.api.getList<MetricBucket>(
      `/analytics/projects/${projectId}/tasks-by-status`,
    );
  }

  getSprintVelocity(
    projectId: string,
    sprintId: string,
  ): Observable<SprintVelocity> {
    return this.api.get<SprintVelocity>(
      `/analytics/projects/${projectId}/sprints/${sprintId}/velocity`,
    );
  }

  getSprintBurndown(
    projectId: string,
    sprintId: string,
  ): Observable<SprintBurndown> {
    return this.api.get<SprintBurndown>(
      `/analytics/projects/${projectId}/sprints/${sprintId}/burndown`,
    );
  }

  getActivityTrends(
    projectId: string,
    days = 14,
  ): Observable<ActivityTrend[]> {
    return this.api.getList<ActivityTrend>(
      `/analytics/projects/${projectId}/activity-trends`,
      { days },
    );
  }
}
