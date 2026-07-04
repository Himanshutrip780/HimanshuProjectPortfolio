import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ActivityRecord } from '../models/activity.models';
import { ApiHttpService } from './api-http.service';

@Injectable({ providedIn: 'root' })
export class ActivityService {
  private readonly api = inject(ApiHttpService);

  getEntityActivities(
    entityType: string,
    entityId: string,
  ): Observable<ActivityRecord[]> {
    return this.api.getList<ActivityRecord>(
      `/activities/${entityType}/${entityId}`,
    );
  }

  getProjectActivities(
    projectId: string,
    limit = 50,
  ): Observable<ActivityRecord[]> {
    return this.api.getList<ActivityRecord>(
      `/activities/projects/${projectId}`,
      { limit },
    );
  }
}
