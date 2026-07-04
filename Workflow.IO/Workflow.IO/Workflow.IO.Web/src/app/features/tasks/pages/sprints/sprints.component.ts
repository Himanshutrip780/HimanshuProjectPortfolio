import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute } from '@angular/router';

import { SprintResponse, SprintStatus } from '../../../../core/models/task.models';
import { UpdateSprintRequest } from '../../../../core/models/task-planning.models';
import { TaskPlanningService } from '../../../../core/services/task-planning.service';
import { getApiErrorMessage } from '../../../../core/utils/api-error.util';
import { TaskService } from '../../services/task.service';

@Component({
  selector: 'app-sprints',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './sprints.component.html',
  styles: `
    .panel {
      background-color: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg, 0.75rem);
      padding: 1.25rem;
      margin-bottom: 1.5rem;
    }

    .inline {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
      margin-bottom: 1rem;
      align-items: center;
    }

    input[type="text"],
    input[type="date"] {
      padding: 0.5rem 0.65rem;
      height: 2.25rem;
      border: 1px solid var(--border-color);
      border-radius: 0.375rem;
      background-color: var(--bg-input);
      color: var(--text-primary);
      font-size: 0.875rem;
      outline: none;
      transition: border-color 0.15s;
      color-scheme: dark light;
    }

    input[type="text"] {
      min-width: 200px;
      flex: 1;
    }

    input[type="date"] {
      min-width: 160px;
    }

    input[type="text"]:focus,
    input[type="date"]:focus {
      border-color: var(--border-focus);
    }

    button {
      padding: 0.5rem 1rem;
      height: 2.25rem;
      border: 1px solid var(--border-color);
      border-radius: 0.375rem;
      background-color: var(--bg-hover);
      color: var(--text-primary);
      font-size: 0.875rem;
      cursor: pointer;
      transition: background-color 0.15s;
      white-space: nowrap;
    }

    button:hover {
      background-color: var(--primary-color);
      color: #fff;
      border-color: var(--primary-color);
    }

    button.primary-action {
      background: var(--primary-gradient);
      color: #fff;
      border-color: transparent;
    }

    ul {
      list-style: none;
      padding: 0;
    }

    li {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      gap: 1rem;
      padding: 0.75rem 0;
      border-bottom: 1px solid var(--border-color);
    }

    .status {
      margin-left: 0.5rem;
      color: var(--text-muted);
      font-size: 0.875rem;
    }

    .actions {
      display: flex;
      gap: 0.5rem;
      flex-shrink: 0;
    }

    .danger {
      color: var(--danger-color, #ef4444);
      border-color: rgba(239, 68, 68, 0.3);
    }

    .danger:hover {
      background-color: rgba(239, 68, 68, 0.1);
      color: var(--danger-color, #ef4444);
      border-color: var(--danger-color, #ef4444);
    }

    .error {
      color: var(--danger-color, #ef4444);
    }
  `,
})
export class SprintsComponent implements OnInit {
  readonly SprintStatus = SprintStatus;
  private readonly taskService = inject(TaskService);
  private readonly planning = inject(TaskPlanningService);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly sprints = signal<SprintResponse[]>([]);
  readonly loading = signal(true);
  readonly busy = signal(false);
  readonly error = signal<string | null>(null);
  readonly editingId = signal<string | null>(null);

  readonly createForm = this.fb.group({
    name: ['', Validators.required],
    startDate: ['', Validators.required],
    endDate: ['', Validators.required],
  });

  readonly editForm = this.fb.group({
    name: ['', Validators.required],
    startDate: ['', Validators.required],
    endDate: ['', Validators.required],
  });

  ngOnInit(): void {
    this.loadSprints();
  }

  statusLabel(status: SprintStatus): string {
    return SprintStatus[status] ?? String(status);
  }

  createSprint(): void {
    const projectId = this.projectId();
    if (!projectId || this.createForm.invalid) {
      return;
    }

    this.busy.set(true);
    const raw = this.createForm.getRawValue();
    this.taskService
      .createSprint(projectId, raw)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.createForm.reset();
          this.busy.set(false);
          this.loadSprints();
        },
        error: (err) => {
          this.error.set(getApiErrorMessage(err));
          this.busy.set(false);
        },
      });
  }

  startEdit(sprint: SprintResponse): void {
    this.editingId.set(sprint.sprintId);
    this.editForm.patchValue({
      name: sprint.name,
      startDate: sprint.startDate.slice(0, 10),
      endDate: sprint.endDate.slice(0, 10),
    });
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(sprintId: string): void {
    if (this.editForm.invalid) {
      return;
    }

    const raw = this.editForm.getRawValue();
    const request: UpdateSprintRequest = {
      name: raw.name,
      startDate: raw.startDate,
      endDate: raw.endDate,
    };

    this.planning
      .updateSprint(sprintId, request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.editingId.set(null);
          this.loadSprints();
        },
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  startSprint(sprintId: string): void {
    this.taskService
      .startSprint(sprintId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.loadSprints(),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  completeSprint(sprintId: string): void {
    this.taskService
      .completeSprint(sprintId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.loadSprints(),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  deleteSprint(sprintId: string): void {
    if (!confirm('Delete this sprint?')) {
      return;
    }

    this.planning
      .deleteSprint(sprintId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.loadSprints(),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  private projectId(): string | null {
    return this.route.parent?.snapshot.paramMap.get('projectId') ?? null;
  }

  private loadSprints(): void {
    const projectId = this.projectId();
    if (!projectId) {
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.taskService
      .getProjectSprints(projectId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (sprints) => {
          this.sprints.set(sprints);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(getApiErrorMessage(err));
          this.loading.set(false);
        },
      });
  }
}
