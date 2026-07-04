import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { DatePipe } from '@angular/common';

import { ActivityRecord } from '../../../../core/models/activity.models';
import { ActivityService } from '../../../../core/services/activity.service';
import { getApiErrorMessage } from '../../../../core/utils/api-error.util';

@Component({
  selector: 'app-project-activity',
  standalone: true,
  imports: [DatePipe],
  template: `
    <section class="activity-container">
      <header class="page-header">
        <div class="header-main">
          <span class="material-symbols-outlined header-icon">history</span>
          <div>
            <h1>Project Activity Feed</h1>
            <p class="subtitle">Chronological timeline of all updates, events, and task transitions in this project.</p>
          </div>
        </div>
      </header>

      @if (loading()) {
        <div class="state-container">
          <div class="spinner"></div>
          <p>Loading project activity feed...</p>
        </div>
      } @else if (error()) {
        <div class="error-banner">
          <span class="material-symbols-outlined">error</span>
          <span>{{ error() }}</span>
        </div>
      } @else if (activities().length === 0) {
        <div class="state-container">
          <span class="material-symbols-outlined" style="font-size: 2.5rem; color: var(--text-muted);">history_toggle_off</span>
          <p>No activity recorded yet in this project.</p>
        </div>
      } @else {
        <div class="timeline">
          @for (a of activities(); track a.activityRecordId; let last = $last) {
            <div class="timeline-item">
              <div class="timeline-marker-wrapper">
                <div class="timeline-marker" [class]="markerClass(a.eventType)">
                  <span class="material-symbols-outlined timeline-icon">{{ iconName(a.eventType) }}</span>
                </div>
                @if (!last) {
                  <div class="timeline-line"></div>
                }
              </div>
              <div class="timeline-content panel">
                <div class="timeline-content-header">
                  <span class="event-badge" [class]="markerClass(a.eventType)">{{ a.eventType }}</span>
                  <span class="timestamp">{{ a.createdAt | date:'medium' }}</span>
                </div>
                <p class="description">{{ a.description ?? 'No details provided.' }}</p>
                <div class="meta-footer">
                  <span class="entity-badge">
                     <span class="material-symbols-outlined">database</span>
                    {{ a.entityType }}
                  </span>
                </div>
              </div>
            </div>
          }
        </div>
      }
    </section>
  `,
  styles: `
    .activity-container {
      padding: 2rem;
      max-width: 800px;
      margin: 0 auto;
      display: flex;
      flex-direction: column;
      gap: 2rem;
    }

    .page-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
    }

    .header-main {
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .header-icon {
      font-size: 2.5rem;
      color: var(--primary-color);
      background: var(--primary-glow);
      padding: 0.5rem;
      border-radius: var(--radius-lg);
    }

    .subtitle {
      color: var(--text-secondary);
      font-size: 0.95rem;
      margin-top: 0.25rem;
    }

    .timeline {
      display: flex;
      flex-direction: column;
      position: relative;
    }

    .timeline-item {
      display: flex;
      gap: 1.5rem;
      margin-bottom: 0.5rem;
    }

    .timeline-marker-wrapper {
      display: flex;
      flex-direction: column;
      align-items: center;
    }

    .timeline-marker {
      width: 2.5rem;
      height: 2.5rem;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      border: 2px solid var(--border-color);
      background-color: var(--bg-panel);
      z-index: 10;
      flex-shrink: 0;
    }

    .timeline-icon {
      font-size: 1.25rem;
    }

    .timeline-line {
      width: 2px;
      flex: 1;
      background-color: var(--border-color);
      min-height: 2.5rem;
    }

    .timeline-content {
      flex: 1;
      margin-bottom: 1.5rem;
    }

    .panel {
      background: var(--bg-panel);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      padding: 1.25rem 1.5rem;
      box-shadow: var(--shadow-sm);
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }

    .timeline-content-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      flex-wrap: wrap;
      gap: 0.5rem;
    }

    .event-badge {
      font-size: 0.7rem;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      padding: 0.15rem 0.5rem;
      border-radius: 9999px;
    }

    .timestamp {
      font-size: 0.8rem;
      color: var(--text-secondary);
    }

    .description {
      font-size: 0.9rem;
      color: var(--text-primary);
      margin: 0;
      line-height: 1.5;
    }

    .meta-footer {
      display: flex;
      gap: 0.5rem;
    }

    .entity-badge {
      display: inline-flex;
      align-items: center;
      gap: 0.25rem;
      font-size: 0.75rem;
      color: var(--text-secondary);
      background-color: var(--bg-hover);
      padding: 0.2rem 0.5rem;
      border-radius: var(--radius-sm);
      border: 1px solid var(--border-color);
    }

    .entity-badge span {
      font-size: 0.95rem;
    }

    .event-create {
      background-color: rgba(16, 185, 129, 0.1) !important;
      color: #10b981 !important;
      border-color: rgba(16, 185, 129, 0.25) !important;
    }

    .event-update {
      background-color: rgba(59, 130, 246, 0.1) !important;
      color: #3b82f6 !important;
      border-color: rgba(59, 130, 246, 0.25) !important;
    }

    .event-delete {
      background-color: rgba(239, 68, 68, 0.1) !important;
      color: var(--danger-color) !important;
      border-color: rgba(239, 68, 68, 0.25) !important;
    }

    .event-comment {
      background-color: rgba(245, 158, 11, 0.1) !important;
      color: var(--accent-color) !important;
      border-color: rgba(245, 158, 11, 0.25) !important;
    }

    .event-transition {
      background-color: rgba(139, 92, 246, 0.1) !important;
      color: #8b5cf6 !important;
      border-color: rgba(139, 92, 246, 0.25) !important;
    }

    .event-default {
      background-color: rgba(148, 163, 184, 0.1) !important;
      color: #94a3b8 !important;
      border-color: rgba(148, 163, 184, 0.25) !important;
    }

    .state-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 5rem 2rem;
      background: var(--bg-panel);
      border: 1px dashed var(--border-color);
      border-radius: var(--radius-lg);
      text-align: center;
      color: var(--text-secondary);
      gap: 1rem;
    }

    .spinner {
      width: 36px;
      height: 36px;
      border: 3px solid var(--border-color);
      border-top-color: var(--primary-color);
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    .error-banner {
      background-color: rgba(239, 68, 68, 0.1);
      color: var(--danger-color);
      border: 1px solid rgba(239, 68, 68, 0.2);
      border-radius: var(--radius-md);
      padding: 0.75rem 1rem;
      font-size: 0.875rem;
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }
  `,
})
export class ProjectActivityComponent implements OnInit {
  private readonly activityService = inject(ActivityService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly activities = signal<ActivityRecord[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  markerClass(eventType: string): string {
    switch (eventType?.toLowerCase()) {
      case 'create':
      case 'created':
        return 'event-create';
      case 'update':
      case 'updated':
        return 'event-update';
      case 'delete':
      case 'deleted':
        return 'event-delete';
      case 'comment':
      case 'commented':
        return 'event-comment';
      case 'transition':
      case 'transitioned':
        return 'event-transition';
      default:
        return 'event-default';
    }
  }

  iconName(eventType: string): string {
    switch (eventType?.toLowerCase()) {
      case 'create':
      case 'created':
        return 'add_circle';
      case 'update':
      case 'updated':
        return 'edit_note';
      case 'delete':
      case 'deleted':
        return 'delete';
      case 'comment':
      case 'commented':
        return 'comment';
      case 'transition':
      case 'transitioned':
        return 'swap_horiz';
      default:
        return 'info';
    }
  }

  ngOnInit(): void {
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    if (!projectId) {
      return;
    }

    this.activityService
      .getProjectActivities(projectId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (activities) => {
          this.activities.set(activities);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(getApiErrorMessage(err));
          this.loading.set(false);
        },
      });
  }
}
