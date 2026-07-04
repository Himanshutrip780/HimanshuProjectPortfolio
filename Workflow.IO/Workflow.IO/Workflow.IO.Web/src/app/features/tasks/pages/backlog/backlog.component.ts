import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';

import {
  BacklogResponse,
  SprintStatus,
  TaskResponse,
  TaskStatus,
} from '../../../../core/models/task.models';
import { getApiErrorMessage } from '../../../../core/utils/api-error.util';
import { TaskPlanningService } from '../../../../core/services/task-planning.service';
import { TaskService } from '../../services/task.service';

@Component({
  selector: 'app-backlog',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './backlog.component.html',
  styles: `
    .backlog-container {
      padding: 2rem;
      max-width: 1200px;
      margin: 0 auto;
      display: flex;
      flex-direction: column;
      gap: 2rem;
    }

    .backlog-header {
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

    .backlog-grid {
      display: flex;
      flex-direction: column;
      gap: 2rem;
    }

    .sprint-form {
      display: flex;
      align-items: center;
      gap: 1rem;
      flex-wrap: wrap;
    }

    .form-group {
      display: flex;
      flex-direction: column;
    }

    .flex-1 {
      flex: 1;
    }

    .min-w-\\[200px\\] {
      min-width: 200px;
    }

    .panel-header {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      margin-bottom: 1.25rem;
      color: var(--text-primary);
    }

    .panel-header h2 {
      font-size: 1.15rem;
      font-weight: 600;
      margin: 0;
    }

    .panel-header span {
      color: var(--primary-color);
    }

    .glass-panel {
      backdrop-filter: blur(10px);
      background: var(--bg-panel);
    }

    .section-header-bar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
      border-bottom: 1px solid var(--border-color);
      padding-bottom: 0.75rem;
    }

    .section-title-group {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }

    .section-title-group h3 {
      font-size: 1.2rem;
      font-weight: 600;
      color: var(--text-primary);
    }

    .task-list {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .task-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      background: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      padding: 0.75rem 1rem;
      transition: border-color var(--transition-fast), box-shadow var(--transition-fast);
    }

    .task-row:hover {
      border-color: var(--primary-color);
      box-shadow: var(--shadow-sm);
    }

    .task-row-left {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      flex: 1;
    }

    .task-key-tag {
      font-size: 0.75rem;
      font-weight: 700;
      color: var(--text-muted);
      background: var(--bg-hover);
      padding: 0.15rem 0.45rem;
      border-radius: var(--radius-sm);
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }

    .task-link-title {
      font-size: 0.9rem;
      font-weight: 500;
      color: var(--text-primary);
      text-decoration: none;
      transition: color var(--transition-fast);
    }

    .task-link-title:hover {
      color: var(--primary-color);
    }

    .task-row-actions {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .btn-secondary-outline {
      background: transparent;
      border: 1.5px solid var(--border-color);
      color: var(--text-secondary);
    }

    .btn-secondary-outline:hover {
      background: var(--bg-hover);
      border-color: var(--text-muted);
      color: var(--text-primary);
    }

    .btn-icon-only {
      width: 32px;
      height: 32px;
      padding: 0;
      border-radius: var(--radius-sm);
    }

    .btn-danger-hover:hover {
      background: var(--danger-color) !important;
      color: white !important;
      border-color: var(--danger-color) !important;
    }

    .empty-list-placeholder {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 0.5rem;
      padding: 3rem 1rem;
      color: var(--text-muted);
      border: 1px dashed var(--border-color);
      border-radius: var(--radius-md);
      text-align: center;
    }

    .empty-list-placeholder span {
      font-size: 2.5rem;
      color: var(--text-muted);
    }

    .empty-list-placeholder p {
      font-size: 0.85rem;
      color: var(--text-muted);
    }

    .sprints-section {
      margin-top: 1.5rem;
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }

    .sprints-section h2 {
      font-size: 1.3rem;
      font-weight: 600;
      color: var(--text-primary);
    }

    .sprints-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(360px, 1fr));
      gap: 1.5rem;
    }

    @media (max-width: 768px) {
      .sprints-grid {
        grid-template-columns: 1fr;
      }
    }

    .sprint-card {
      position: relative;
      display: flex;
      flex-direction: column;
      gap: 1rem;
      border: 1px solid var(--border-color);
      transition: border-color var(--transition-normal);
    }

    .sprint-card.sprint-active {
      border-color: var(--secondary-color);
      box-shadow: 0 0 0 3px rgba(16, 185, 129, 0.1);
    }

    .sprint-card-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      border-bottom: 1px solid var(--border-color);
      padding-bottom: 0.75rem;
    }

    .sprint-name {
      font-size: 1.05rem;
      font-weight: 600;
      color: var(--text-primary);
      margin-bottom: 0.25rem;
    }

    .sprint-header-actions {
      display: flex;
      align-items: center;
    }

    .btn-sm {
      padding: 0.35rem 0.75rem;
      font-size: 0.8rem;
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
export class BacklogComponent implements OnInit {
  readonly SprintStatus = SprintStatus;
  private readonly taskService = inject(TaskService);
  private readonly planning = inject(TaskPlanningService);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly backlog = signal<BacklogResponse | null>(null);
  readonly loading = signal(true);
  readonly busy = signal(false);
  readonly error = signal<string | null>(null);

  readonly sprintForm = this.fb.group({
    name: ['', Validators.required],
    startDate: ['', Validators.required],
    endDate: ['', Validators.required],
  });

  ngOnInit(): void {
    this.loadBacklog();
  }

  sprintStatusLabel(status: SprintStatus): string {
    return SprintStatus[status] ?? String(status);
  }

  createSprint(): void {
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    if (!projectId || this.sprintForm.invalid) {
      return;
    }

    this.busy.set(true);
    const raw = this.sprintForm.getRawValue();
    this.taskService
      .createSprint(projectId, {
        name: raw.name,
        startDate: raw.startDate,
        endDate: raw.endDate,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.sprintForm.reset();
          this.busy.set(false);
          this.loadBacklog();
        },
        error: (err) => {
          this.error.set(getApiErrorMessage(err));
          this.busy.set(false);
        },
      });
  }

  startSprint(sprintId: string): void {
    this.busy.set(true);
    this.taskService
      .startSprint(sprintId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.busy.set(false);
          this.loadBacklog();
        },
        error: (err) => {
          this.error.set(getApiErrorMessage(err));
          this.busy.set(false);
        },
      });
  }

  completeSprint(sprintId: string): void {
    this.busy.set(true);
    this.taskService
      .completeSprint(sprintId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.busy.set(false);
          this.loadBacklog();
        },
        error: (err) => {
          this.error.set(getApiErrorMessage(err));
          this.busy.set(false);
        },
      });
  }

  moveToBacklog(task: TaskResponse): void {
    this.taskService
      .moveToSprint(task.taskId, { sprintId: null })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.loadBacklog(),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  markBacklogDone(tasks: TaskResponse[]): void {
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    if (!projectId || tasks.length === 0) {
      return;
    }

    this.busy.set(true);
    this.planning
      .bulkUpdate(projectId, {
        taskIds: tasks.map((t) => t.taskId),
        status: TaskStatus.Done,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.busy.set(false);
          this.loadBacklog();
        },
        error: (err) => {
          this.error.set(getApiErrorMessage(err));
          this.busy.set(false);
        },
      });
  }

  rankUp(task: TaskResponse): void {
    this.planning
      .updateRank(task.taskId, { backlogRank: task.backlogRank - 1 })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.loadBacklog(),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  private loadBacklog(): void {
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    if (!projectId) {
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.taskService
      .getBacklog(projectId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (b) => {
          this.backlog.set(b);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(getApiErrorMessage(err));
          this.loading.set(false);
        },
      });
  }
}
