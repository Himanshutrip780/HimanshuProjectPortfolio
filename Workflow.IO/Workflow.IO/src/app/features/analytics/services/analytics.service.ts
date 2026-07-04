import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, map, throwError } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/models/api-response.model';
import {
  ActivityTrend,
  MetricBucket,
  ProjectAnalyticsSummary,
  SprintVelocity,
} from '../../../core/models/analytics.models';
import { ApiErrorService } from '../../../core/services/api-error.service';
import { unwrapApiResponse } from '../../../core/utils/api.util';

@Injectable({ providedIn: 'root' })
export class AnalyticsService {
  private readonly http = inject(HttpClient);
  private readonly apiErrors = inject(ApiErrorService);
  private readonly baseUrl = `${environment.apiGatewayUrl}/analytics`;

  getProjectSummary(projectId: string): Observable<ProjectAnalyticsSummary> {
    return this.get<ProjectAnalyticsSummary>(`/projects/${projectId}/summary`);
  }

  getTasksByStatus(projectId: string): Observable<MetricBucket[]> {
    return this.get<MetricBucket[]>(`/projects/${projectId}/tasks-by-status`);
  }

  getSprintVelocity(
    projectId: string,
    sprintId: string,
  ): Observable<SprintVelocity> {
    return this.get<SprintVelocity>(
      `/projects/${projectId}/sprints/${sprintId}/velocity`,
    );
  }

  getActivityTrends(
    projectId: string,
    days = 14,
  ): Observable<ActivityTrend[]> {
    return this.get<ActivityTrend[]>(
      `/projects/${projectId}/activity-trends?days=${days}`,
    );
  }

  private get<T>(path: string): Observable<T> {
    return this.http.get<ApiResponse<T>>(`${this.baseUrl}${path}`).pipe(
      map(unwrapApiResponse),
      catchError((error) => {
        this.apiErrors.capture(error);
        return throwError(() => error);
      }),
    );
  }
}
