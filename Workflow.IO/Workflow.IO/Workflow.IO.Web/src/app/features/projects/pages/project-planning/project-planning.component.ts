import { Component, DestroyRef, inject, OnInit, signal, effect } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute } from '@angular/router';


import {
  ComponentResponse,
  ReleaseVersionResponse,
  SavedFilterResponse,
} from '../../../../core/models/task-planning.models';
import { EpicResponse } from '../../../../core/models/task.models';
import { TaskPlanningService } from '../../../../core/services/task-planning.service';
import { getApiErrorMessage } from '../../../../core/utils/api-error.util';
import { TaskService } from '../../../tasks/services/task.service';
import { BackButtonService } from '../../../../core/services/back-button.service';

@Component({
  selector: 'app-project-planning',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './project-planning.component.html',
  styles: `
    .panel {
      background-color: var(--bg-panel);
      border: 1px solid var(--border-color);
      border-radius: 0.5rem;
      padding: 1rem;
    }

    section {
      margin-bottom: 1.5rem;
    }

    .inline {
      display: flex;
      gap: 0.5rem;
      margin-bottom: 0.5rem;
    }

    .stack {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      margin-bottom: 0.75rem;
    }

    input,
    textarea,
    button {
      padding: 0.45rem 0.6rem;
      border: 1px solid var(--border-color);
      border-radius: 0.375rem;
      background-color: var(--bg-input);
      color: var(--text-primary);
    }

    ul {
      list-style: none;
      padding: 0;
    }

    li {
      padding: 0.35rem 0;
    }

    code {
      display: block;
      font-size: 0.8rem;
      color: #64748b;
    }

    .released {
      color: #15803d;
      font-size: 0.875rem;
    }

    .error {
      color: #b91c1c;
    }
  `,
})
export class ProjectPlanningComponent implements OnInit {
  private readonly planning = inject(TaskPlanningService);
  private readonly taskService = inject(TaskService);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly backButtonService = inject(BackButtonService);



  readonly components = signal<ComponentResponse[]>([]);
  readonly versions = signal<ReleaseVersionResponse[]>([]);
  readonly filters = signal<SavedFilterResponse[]>([]);
  readonly myFilters = signal<SavedFilterResponse[]>([]);
  readonly epics = signal<EpicResponse[]>([]);
  readonly filterResults = signal<{ taskId: string; issueKey: string; title: string }[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);



  readonly componentForm = this.fb.group({
    name: ['', Validators.required],
  });

  readonly versionForm = this.fb.group({
    name: ['', Validators.required],
  });

  readonly filterForm = this.fb.group({
    name: ['', Validators.required],
    jqlQuery: ['', Validators.required],
  });

  readonly epicForm = this.fb.group({
    name: ['', Validators.required],
  });

  ngOnInit(): void {
    this.load();
  }

  createEpic(): void {
    const projectId = this.projectId();
    if (!projectId || this.epicForm.invalid) {
      return;
    }

    this.taskService
      .createEpic(projectId, { name: this.epicForm.controls.name.value })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.epicForm.reset();
          this.load();
        },
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  createComponent(): void {
    const projectId = this.projectId();
    if (!projectId || this.componentForm.invalid) {
      return;
    }

    this.planning
      .createComponent(projectId, { name: this.componentForm.controls.name.value })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.componentForm.reset();
          this.load();
        },
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  createVersion(): void {
    const projectId = this.projectId();
    if (!projectId || this.versionForm.invalid) {
      return;
    }

    this.planning
      .createVersion(projectId, { name: this.versionForm.controls.name.value })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.versionForm.reset();
          this.load();
        },
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  releaseVersion(versionId: string): void {
    this.planning
      .releaseVersion(versionId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.load(),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  createFilter(): void {
    const projectId = this.projectId();
    if (!projectId || this.filterForm.invalid) {
      return;
    }

    const raw = this.filterForm.getRawValue();
    this.planning
      .createFilter(projectId, { name: raw.name, jqlQuery: raw.jqlQuery })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.filterForm.reset();
          this.load();
        },
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  executeFilter(filterId: string): void {
    this.planning
      .executeFilter(filterId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (tasks) => this.filterResults.set(tasks),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  private projectId(): string | null {
    return this.route.parent?.snapshot.paramMap.get('projectId') ?? null;
  }

  private load(): void {
    const projectId = this.projectId();
    if (!projectId) {
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.planning
      .getComponents(projectId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (components) => {
          this.components.set(components);
          this.planning
            .getVersions(projectId)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
              next: (versions) => {
                this.versions.set(versions);
                this.planning
                  .getProjectFilters(projectId)
                  .pipe(takeUntilDestroyed(this.destroyRef))
                  .subscribe({
                    next: (filters) => {
                      this.filters.set(filters);
                      this.taskService
                        .getProjectEpics(projectId)
                        .pipe(takeUntilDestroyed(this.destroyRef))
                        .subscribe({
                          next: (epics) => this.epics.set(epics),
                        });
                      this.planning
                        .getMyFilters()
                        .pipe(takeUntilDestroyed(this.destroyRef))
                        .subscribe({
                          next: (myFilters) => this.myFilters.set(myFilters),
                        });
                      this.loading.set(false);
                    },
                    error: (err) => {
                      this.error.set(getApiErrorMessage(err));
                      this.loading.set(false);
                    },
                  });
              },
              error: (err) => {
                this.error.set(getApiErrorMessage(err));
                this.loading.set(false);
              },
            });
        },
        error: (err) => {
          this.error.set(getApiErrorMessage(err));
          this.loading.set(false);
        },
      });
  }


}
