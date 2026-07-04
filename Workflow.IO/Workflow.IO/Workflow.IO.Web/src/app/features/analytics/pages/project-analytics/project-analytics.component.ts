import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { forkJoin } from 'rxjs';

import {
  ActivityTrend,
  ProjectAnalyticsSummary,
  SprintBurndown,
  SprintVelocity,
} from '../../../../core/models/analytics.models';
import { SprintResponse } from '../../../../core/models/task.models';
import { AnalyticsService } from '../../../../core/services/analytics.service';
import { TaskService } from '../../../tasks/services/task.service';
import { getApiErrorMessage } from '../../../../core/utils/api-error.util';

@Component({
  selector: 'app-project-analytics',
  standalone: true,
  imports: [DecimalPipe, DatePipe],
  template: `
    <section class="analytics-container">
      <header class="analytics-header">
        <div class="header-main">
          <span class="material-symbols-outlined header-icon">insights</span>
          <div>
            <h1>Project Analytics</h1>
            <p class="subtitle">Real-time performance metrics, team velocity, and project health</p>
          </div>
        </div>
        <button type="button" class="btn btn-secondary btn-icon" (click)="reload()" [disabled]="loading()">
          <span class="material-symbols-outlined" [class.spin]="loading()">refresh</span> Refresh
        </button>
      </header>

      @if (loading()) {
        <div class="state-container">
          <div class="spinner"></div>
          <p>Crunching analytics data...</p>
        </div>
      } @else if (error()) {
        <div class="state-container error-state">
          <span class="material-symbols-outlined">error_outline</span>
          <p>{{ error() }}</p>
          <button type="button" class="btn btn-primary" (click)="reload()">Try Again</button>
        </div>
      } @else {
        @let s = summary();
        @if (s) {
          <!-- Overview Grid -->
          <div class="analytics-grid">
            <div class="stat-card">
              <div class="card-glow primary-glow"></div>
              <div class="stat-card-header">
                <span>Total Tasks</span>
                <span class="material-symbols-outlined icon-blue">assignment</span>
              </div>
              <strong class="stat-number">{{ s.totalTasks }}</strong>
            </div>

            <div class="stat-card">
              <div class="card-glow success-glow"></div>
              <div class="stat-card-header">
                <span>Completed Tasks</span>
                <span class="material-symbols-outlined icon-green">task_alt</span>
              </div>
              <strong class="stat-number">{{ s.completedTasks }}</strong>
            </div>

            <div class="stat-card">
              <div class="card-glow danger-glow"></div>
              <div class="stat-card-header">
                <span>Overdue Tasks</span>
                <span class="material-symbols-outlined icon-red">event_busy</span>
              </div>
              <strong class="stat-number">{{ s.overdueTasks }}</strong>
            </div>

            <div class="stat-card">
              <div class="card-glow warning-glow"></div>
              <div class="stat-card-header">
                <span>Story Points Completed</span>
                <span class="material-symbols-outlined icon-yellow">toll</span>
              </div>
              <strong class="stat-number">{{ s.completedStoryPoints }}/{{ s.totalStoryPoints }}</strong>
            </div>

            <div class="stat-card highlight-card">
              <div class="card-glow accent-glow"></div>
              <div class="stat-card-header">
                <span>Completion Rate</span>
                <span class="material-symbols-outlined icon-purple">donut_large</span>
              </div>
              <div class="rate-group">
                <strong class="stat-number">{{ s.completionRate | number:'1.0-1' }}%</strong>
                <div class="circular-progress-wrap">
                  <div class="progress-bar-linear">
                    <div class="progress-fill" [style.width.%]="s.completionRate"></div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Charts Row -->
          <div class="charts-row">
            <!-- Tasks by Status -->
            <section class="panel glass-panel chart-box">
              <div class="chart-header">
                <span class="material-symbols-outlined">analytics</span>
                <h2>Tasks by Status</h2>
              </div>
              <div class="bars-container">
                @for (b of s.tasksByStatus; track b.name) {
                  <div class="bar-row">
                    <span class="bar-label">{{ b.name }}</span>
                    <div class="bar-track">
                      <div class="bar-fill" [style.width.%]="barWidth(b.count, s.totalTasks)" [class]="getStatusClass(b.name)"></div>
                    </div>
                    <strong class="bar-value">{{ b.count }}</strong>
                  </div>
                }
              </div>
            </section>

            <!-- Activity Trends -->
            @if (trends().length > 0) {
              <section class="panel glass-panel chart-box">
                <div class="chart-header">
                  <span class="material-symbols-outlined">trending_up</span>
                  <h2>Activity Trend (Last 14 Days)</h2>
                </div>
                <div class="bars-container scrollable-bars">
                  @for (t of trends(); track t.date) {
                    <div class="bar-row">
                      <span class="bar-label">{{ t.date | date:'MMM d' }}</span>
                      <div class="bar-track">
                        <div class="bar-fill trend-fill" [style.width.%]="barWidth(t.count, maxTrend())"></div>
                      </div>
                      <strong class="bar-value">{{ t.count }}</strong>
                    </div>
                  }
                </div>
              </section>
            }
          </div>

          <!-- Sprint Metrics Section -->
          <section class="panel glass-panel sprint-metrics-panel">
            <div class="sprint-section-header">
              <div class="sprint-title-group">
                <span class="material-symbols-outlined">speed</span>
                <h2>Sprint Metrics</h2>
              </div>
              <div class="sprint-selector-group">
                <label for="sprint-select">Target Sprint</label>
                <select id="sprint-select" [value]="selectedSprintId() ?? ''" (change)="onSprintSelect($event)">
                  <option value="">Choose a sprint</option>
                  @for (sp of sprints(); track sp.sprintId) {
                    <option [value]="sp.sprintId">{{ sp.name }}</option>
                  }
                </select>
              </div>
            </div>

            @if (velocity(); as v) {
              <div class="sprint-metrics-grid">
                <div class="sprint-stat-card">
                  <span>Velocity Tasks (Done / Total)</span>
                  <strong>{{ v.completedTasks }} / {{ v.totalTasks }}</strong>
                </div>
                <div class="sprint-stat-card">
                  <span>Velocity Points (Done / Total)</span>
                  <strong>{{ v.completedStoryPoints }} / {{ v.totalStoryPoints }}</strong>
                </div>
              </div>
            }

            @if (burndown(); as b) {
              <div class="burndown-section">
                <div class="burndown-header">
                  <span class="material-symbols-outlined">insert_chart</span>
                  <h3>Burndown Chart ({{ b.totalStoryPoints }} pts)</h3>
                </div>
                <div class="bars-container">
                  @for (p of b.points; track p.date) {
                    <div class="bar-row">
                      <span class="bar-label">{{ p.date | date:'MMM d' }}</span>
                      <div class="bar-track">
                        <div class="bar-fill burndown-fill" [style.width.%]="barWidth(p.remainingStoryPoints, b.totalStoryPoints)"></div>
                      </div>
                      <strong class="bar-value">{{ p.remainingStoryPoints }}</strong>
                    </div>
                  }
                </div>
              </div>
            } @else {
              <div class="sprint-empty-state">
                <span class="material-symbols-outlined">view_kanban</span>
                <p>Select a sprint above to inspect velocity and burndown metrics.</p>
              </div>
            }
          </section>
        }
      }
    </section>
  `,
  styles: `
    .analytics-container {
      padding: 2rem;
      max-width: 1200px;
      margin: 0 auto;
      display: flex;
      flex-direction: column;
      gap: 2rem;
    }

    .analytics-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 0.5rem;
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

    .btn-icon {
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
    }

    .spin {
      animation: rotate 1.5s linear infinite;
    }

    @keyframes rotate {
      to { transform: rotate(360deg); }
    }

    .analytics-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(210px, 1fr));
      gap: 1.25rem;
    }

    .stat-card {
      position: relative;
      background: var(--bg-panel);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      padding: 1.5rem;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      box-shadow: var(--shadow-sm);
      overflow: hidden;
      transition: border-color var(--transition-normal), box-shadow var(--transition-normal);
    }

    .stat-card:hover {
      border-color: var(--primary-color);
      box-shadow: var(--shadow-md);
    }

    .card-glow {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      height: 3px;
    }

    .primary-glow { background: #3b82f6; }
    .success-glow { background: var(--secondary-color); }
    .danger-glow { background: var(--danger-color); }
    .warning-glow { background: var(--accent-color); }
    .accent-glow { background: linear-gradient(135deg, #8b5cf6, #ec4899); }

    .stat-card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-size: 0.85rem;
      color: var(--text-secondary);
      font-weight: 500;
    }

    .stat-card-header span:last-child {
      font-size: 1.25rem;
    }

    .icon-blue { color: #3b82f6; }
    .icon-green { color: var(--secondary-color); }
    .icon-red { color: var(--danger-color); }
    .icon-yellow { color: var(--accent-color); }
    .icon-purple { color: #8b5cf6; }

    .stat-number {
      font-size: 1.85rem;
      font-weight: 700;
      color: var(--text-primary);
      line-height: 1.2;
    }

    .highlight-card {
      background: linear-gradient(135deg, var(--bg-panel) 0%, rgba(139, 92, 246, 0.05) 100%);
      border-color: rgba(139, 92, 246, 0.2);
    }

    .rate-group {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .progress-bar-linear {
      height: 6px;
      background: var(--border-color);
      border-radius: 99px;
      overflow: hidden;
    }

    .progress-fill {
      height: 100%;
      background: linear-gradient(90deg, #8b5cf6 0%, #ec4899 100%);
      border-radius: 99px;
    }

    .charts-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1.5rem;
    }

    @media (max-width: 820px) {
      .charts-row {
        grid-template-columns: 1fr;
      }
    }

    .chart-box {
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }

    .chart-header {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      color: var(--text-primary);
    }

    .chart-header h2 {
      font-size: 1.15rem;
      font-weight: 600;
      margin: 0;
    }

    .chart-header span {
      color: var(--primary-color);
    }

    .bars-container {
      display: flex;
      flex-direction: column;
      gap: 0.85rem;
    }

    .scrollable-bars {
      max-height: 320px;
      overflow-y: auto;
      padding-right: 0.25rem;
    }

    .bar-row {
      display: grid;
      grid-template-columns: 110px 1fr 45px;
      gap: 0.75rem;
      align-items: center;
      font-size: 0.85rem;
    }

    .bar-label {
      color: var(--text-secondary);
      font-weight: 500;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .bar-track {
      height: 10px;
      background: var(--bg-hover);
      border-radius: var(--radius-sm);
      overflow: hidden;
      border: 1px solid var(--border-color);
    }

    .bar-fill {
      height: 100%;
      border-radius: var(--radius-sm);
      transition: width var(--transition-slow) var(--motion-easing);
    }

    .status-todo { background: #64748b; }
    .status-progress { background: #3b82f6; }
    .status-review { background: var(--accent-color); }
    .status-done { background: var(--secondary-color); }
    .status-blocked { background: var(--danger-color); }
    .status-default { background: var(--primary-color); }

    .trend-fill {
      background: linear-gradient(90deg, #7c3aed 0%, #4f46e5 100%);
      transition: width var(--transition-slow) var(--motion-easing);
    }

    .burndown-fill {
      background: linear-gradient(90deg, #10b981 0%, #059669 100%);
      transition: width var(--transition-slow) var(--motion-easing);
    }

    .bar-value {
      text-align: right;
      color: var(--text-primary);
      font-weight: 600;
    }

    .sprint-metrics-panel {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    .sprint-section-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      flex-wrap: wrap;
      gap: 1rem;
      border-bottom: 1px solid var(--border-color);
      padding-bottom: 1rem;
    }

    .sprint-title-group {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .sprint-title-group h2 {
      font-size: 1.25rem;
      font-weight: 600;
      margin: 0;
      color: var(--text-primary);
    }

    .sprint-title-group span {
      color: var(--primary-color);
    }

    .sprint-selector-group {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }

    .sprint-selector-group label {
      font-size: 0.85rem;
      font-weight: 500;
      color: var(--text-secondary);
    }

    .sprint-selector-group select {
      min-width: 180px;
      padding: 0.45rem 0.75rem;
      border-radius: var(--radius-md);
      background: var(--bg-input);
      color: var(--text-primary);
      border: 1.5px solid var(--border-color);
      outline: none;
    }

    .sprint-metrics-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1.25rem;
    }

    @media (max-width: 580px) {
      .sprint-metrics-grid {
        grid-template-columns: 1fr;
      }
    }

    .sprint-stat-card {
      background: var(--bg-hover);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      padding: 1.25rem;
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
    }

    .sprint-stat-card span {
      font-size: 0.85rem;
      color: var(--text-secondary);
      font-weight: 500;
    }

    .sprint-stat-card strong {
      font-size: 1.5rem;
      font-weight: 700;
      color: var(--text-primary);
    }

    .burndown-section {
      display: flex;
      flex-direction: column;
      gap: 1rem;
      border-top: 1px solid var(--border-color);
      padding-top: 1.5rem;
    }

    .burndown-header {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      color: var(--text-primary);
    }

    .burndown-header h3 {
      font-size: 1.05rem;
      font-weight: 600;
      margin: 0;
    }

    .sprint-empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 0.5rem;
      padding: 4rem 2rem;
      color: var(--text-muted);
      text-align: center;
    }

    .sprint-empty-state span {
      font-size: 2.5rem;
      color: var(--text-muted);
    }

    .state-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 6rem 2rem;
      background: var(--bg-panel);
      border: 1px dashed var(--border-color);
      border-radius: var(--radius-lg);
      text-align: center;
      color: var(--text-secondary);
      gap: 1rem;
    }

    .spinner {
      width: 40px;
      height: 40px;
      border: 3px solid var(--border-color);
      border-top-color: var(--primary-color);
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }
  `,
})
export class ProjectAnalyticsComponent implements OnInit {
  private readonly analytics = inject(AnalyticsService);
  private readonly taskService = inject(TaskService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly summary = signal<ProjectAnalyticsSummary | null>(null);
  readonly trends = signal<ActivityTrend[]>([]);
  readonly sprints = signal<SprintResponse[]>([]);
  readonly velocity = signal<SprintVelocity | null>(null);
  readonly burndown = signal<SprintBurndown | null>(null);
  readonly selectedSprintId = signal<string | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    if (!projectId) {
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    forkJoin({
      summary: this.analytics.getProjectSummary(projectId),
      trends: this.analytics.getActivityTrends(projectId, 14),
      sprints: this.taskService.getProjectSprints(projectId),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ summary, trends, sprints }) => {
          this.summary.set(summary);
          this.trends.set(trends);
          this.sprints.set(sprints);
          this.loading.set(false);

          const active = sprints.find((s) => s.status === 2) ?? sprints[0];
          if (active) {
            this.selectedSprintId.set(active.sprintId);
            this.loadSprintMetrics(projectId, active.sprintId);
          }
        },
        error: (err) => {
          this.error.set(getApiErrorMessage(err));
          this.loading.set(false);
        },
      });
  }

  onSprintSelect(event: Event): void {
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    const sprintId = (event.target as HTMLSelectElement).value;
    if (!projectId || !sprintId) {
      this.velocity.set(null);
      this.burndown.set(null);
      this.selectedSprintId.set(null);
      return;
    }

    this.selectedSprintId.set(sprintId);
    this.loadSprintMetrics(projectId, sprintId);
  }

  barWidth(value: number, max: number): number {
    if (max <= 0) {
      return 0;
    }

    return Math.min(100, Math.round((value / max) * 100));
  }

  maxTrend(): number {
    const counts = this.trends().map((t) => t.count);
    return Math.max(...counts, 1);
  }

  getStatusClass(status: string): string {
    const s = status.toLowerCase();
    if (s.includes('todo') || s.includes('to do')) return 'status-todo';
    if (s.includes('progress')) return 'status-progress';
    if (s.includes('review')) return 'status-review';
    if (s.includes('done')) return 'status-done';
    if (s.includes('block')) return 'status-blocked';
    return 'status-default';
  }

  private loadSprintMetrics(projectId: string, sprintId: string): void {
    forkJoin({
      velocity: this.analytics.getSprintVelocity(projectId, sprintId),
      burndown: this.analytics.getSprintBurndown(projectId, sprintId),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ velocity, burndown }) => {
          this.velocity.set(velocity);
          this.burndown.set(burndown);
        },
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }
}
