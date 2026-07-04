import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import {
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';

import { ActivityRecord } from '../../../../core/models/activity.models';
import { CommentResponse } from '../../../../core/models/comment.models';
import { FileAttachmentResponse } from '../../../../core/models/file.models';
import {
  CreateTaskLinkRequest,
  CreateWorkLogRequest,
  TaskLinkResponse,
  TaskLinkType,
  WorkLogResponse,
} from '../../../../core/models/task-planning.models';
import {
  EpicResponse,
  SprintResponse,
  SubTaskResponse,
  TaskLabelResponse,
  TaskPriority,
  TaskResponse,
  TaskStatus,
  TaskWatcherResponse,
} from '../../../../core/models/task.models';
import { TeamResponse } from '../../../../core/models/team.models';
import { UserLookup, UserDto } from '../../../../core/models/user.models';
import { UserService } from '../../../../core/services/user.service';
import { ActivityService } from '../../../../core/services/activity.service';
import { CommentService } from '../../../../core/services/comment.service';
import { FileService } from '../../../../core/services/file.service';
import { RealtimeService } from '../../../../core/services/realtime.service';
import { TaskPlanningService } from '../../../../core/services/task-planning.service';
import { getApiErrorMessage } from '../../../../core/utils/api-error.util';
import { RichTextEditorComponent } from '../../../../shared/components/rich-text-editor/rich-text-editor.component';
import { UserPickerComponent } from '../../../../shared/components/user-picker/user-picker.component';
import { TaskService } from '../../services/task.service';
import { TeamService } from '../../../teams/services/team.service';

@Component({
  selector: 'app-task-detail',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    RichTextEditorComponent,
    UserPickerComponent,
  ],
  templateUrl: './task-detail.component.html',
  styles: `
    .task-detail-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 1.5rem 2rem;
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
      animation: fadeIn var(--transition-normal);
    }

    .task-detail-header-bar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      border-bottom: 1px solid var(--border-color);
      padding-bottom: 1rem;
    }

    .back-link {
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      color: var(--text-secondary);
      text-decoration: none;
      font-weight: 500;
      font-size: 0.95rem;
      transition: color var(--transition-fast);
    }

    .back-link:hover {
      color: var(--primary-color);
    }

    .btn-danger-outline {
      background: transparent;
      border: 1px solid rgba(239, 68, 68, 0.4);
      color: var(--danger-color);
    }
    
    .btn-danger-outline:hover {
      background: rgba(239, 68, 68, 0.08);
      border-color: var(--danger-color);
    }

    .task-detail-grid {
      display: grid;
      grid-template-columns: 7fr 4fr;
      gap: 2rem;
    }

    @media (max-width: 992px) {
      .task-detail-grid {
        grid-template-columns: 1fr;
      }
    }

    .task-card {
      background: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      padding: 1.5rem;
      box-shadow: var(--shadow-sm);
      margin-bottom: 1.5rem;
      transition: box-shadow var(--transition-fast), border-color var(--transition-fast);
    }

    .task-card:hover {
      box-shadow: var(--shadow-md);
      border-color: var(--text-muted);
    }

    .task-title-section {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      margin-bottom: 1.5rem;
    }

    .task-key-badge {
      display: inline-flex;
      align-self: flex-start;
      background: var(--bg-hover);
      color: var(--text-secondary);
      font-weight: 700;
      font-size: 0.8rem;
      padding: 0.2rem 0.6rem;
      border-radius: var(--radius-sm);
      border: 1px solid var(--border-color);
    }

    .title-form {
      width: 100%;
    }

    .title-input-el {
      width: 100%;
      border: none;
      background: transparent;
      font-size: 1.8rem;
      font-weight: 800;
      color: var(--text-primary);
      padding: 0.25rem 0;
      border-bottom: 2px solid transparent;
      outline: none;
      transition: border-color var(--transition-fast);
    }

    .title-input-el:focus {
      border-bottom-color: var(--primary-color);
      box-shadow: none !important;
    }

    .section-title-label {
      font-size: 0.95rem;
      font-weight: 600;
      color: var(--text-secondary);
      margin-bottom: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }

    .desc-textarea-el {
      width: 100%;
      border: 1px solid var(--border-color);
      background: var(--bg-body);
      border-radius: var(--radius-md);
      padding: 0.75rem 1rem;
      font-family: var(--font-sans);
      font-size: 0.95rem;
      line-height: 1.6;
      color: var(--text-primary);
      resize: vertical;
    }

    .card-header-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1rem;
    }

    .card-section-title {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 1.1rem;
      font-weight: 700;
      color: var(--text-primary);
    }

    .card-section-title span {
      color: var(--primary-color);
    }

    .subtask-progress-text {
      font-size: 0.8rem;
      color: var(--text-secondary);
      font-weight: 600;
    }

    .add-subtask-form, .add-link-inline-form, .add-worklog-inline-form {
      margin-bottom: 1.25rem;
    }

    .input-group {
      display: flex;
      gap: 0.5rem;
    }

    .form-control-input {
      flex: 1;
      padding: 0.5rem 0.75rem;
      font-size: 0.9rem;
    }

    .subtasks-list-items {
      list-style: none;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .subtask-row-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      background: var(--bg-body);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      padding: 0.6rem 0.85rem;
      transition: all var(--transition-fast);
    }

    .subtask-row-item:hover {
      border-color: var(--text-muted);
      background: var(--bg-hover);
    }

    .subtask-checkbox-label {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      cursor: pointer;
      flex: 1;
    }

    .subtask-checkbox-label input[type="checkbox"] {
      width: 16px;
      height: 16px;
      cursor: pointer;
    }

    .subtask-text {
      font-size: 0.9rem;
      color: var(--text-primary);
      transition: color var(--transition-fast);
    }

    .subtask-row-item.completed .subtask-text {
      text-decoration: line-through;
      color: var(--text-muted);
    }

    .btn-delete-item, .btn-delete-comment, .btn-delete-link {
      background: transparent;
      border: none;
      color: var(--text-muted);
      cursor: pointer;
      padding: 0.2rem;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all var(--transition-fast);
    }

    .btn-delete-item:hover, .btn-delete-comment:hover, .btn-delete-link:hover {
      color: var(--danger-color);
      background: rgba(239, 68, 68, 0.08);
    }

    .btn-delete-item span, .btn-delete-comment span, .btn-delete-link span {
      font-size: 1.1rem;
    }

    .empty-subtasks-msg, .empty-comments-msg, .empty-watchers-msg {
      text-align: center;
      padding: 1.5rem;
      color: var(--text-muted);
      font-size: 0.875rem;
      border: 1px dashed var(--border-color);
      border-radius: var(--radius-md);
    }

    /* Comment Section Styles */
    .comment-input-area {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
      margin-bottom: 1.5rem;
    }

    .comment-submit-row {
      display: flex;
      justify-content: flex-end;
    }

    .comments-list {
      list-style: none;
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .comment-bubble-item {
      display: flex;
      gap: 1rem;
      padding-top: 1rem;
      border-top: 1px solid var(--border-color);
    }

    .comment-author-avatar {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      color: white;
      font-weight: 700;
      font-size: 0.8rem;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
      box-shadow: var(--shadow-sm);
    }

    .comment-content-wrapper {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
    }

    .comment-header-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .comment-author-name {
      font-size: 0.85rem;
      font-weight: 700;
      color: var(--text-primary);
    }

    .comment-body-text {
      font-size: 0.9rem;
      color: var(--text-secondary);
      line-height: 1.5;
    }

    /* Inspector Sidebar Styles */
    .inspector-title {
      font-size: 1rem;
      font-weight: 700;
      color: var(--text-primary);
      border-bottom: 1px solid var(--border-color);
      padding-bottom: 0.75rem;
      margin-bottom: 1rem;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }

    .inspector-grid {
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }

    .property-row {
      display: grid;
      grid-template-columns: 4.5fr 7.5fr;
      align-items: center;
      gap: 0.5rem;
    }

    .property-label {
      font-size: 0.85rem;
      font-weight: 600;
      color: var(--text-secondary);
    }

    .property-value {
      width: 100%;
    }

    .property-select, .property-input-number, .property-input-date {
      width: 100%;
      padding: 0.45rem 0.65rem;
      font-size: 0.875rem;
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      background: var(--bg-body);
      color: var(--text-primary);
    }

    .property-select:focus, .property-input-number:focus, .property-input-date:focus {
      border-color: var(--border-focus);
      background: var(--bg-input);
    }

    .status-select-el {
      font-weight: 600;
      background: var(--primary-glow);
      color: var(--primary-color);
      border-color: transparent;
    }

    .status-select-el:focus {
      background: var(--bg-input);
      border-color: var(--primary-color);
    }

    /* Assignee UI elements */
    .assignee-picker-block {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .active-assignee-row {
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      background: var(--bg-body);
      border: 1px solid var(--border-color);
      padding: 0.35rem 0.6rem;
      border-radius: var(--radius-md);
    }

    .assignee-avatar-icon {
      width: 24px;
      height: 24px;
      border-radius: 50%;
      color: white;
      font-weight: 700;
      font-size: 0.7rem;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .assignee-id-txt {
      font-size: 0.85rem;
      font-weight: 600;
      color: var(--text-primary);
      max-width: 120px;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .btn-clear-assignee {
      background: transparent;
      border: none;
      color: var(--text-muted);
      cursor: pointer;
      padding: 0.1rem;
      display: flex;
      align-items: center;
    }

    .btn-clear-assignee:hover {
      color: var(--danger-color);
    }

    .btn-clear-assignee span {
      font-size: 1rem;
    }

    .unassigned-txt {
      font-size: 0.85rem;
      color: var(--text-muted);
      font-style: italic;
    }

    .user-picker-wrapper {
      margin-top: 0.25rem;
    }

    /* Attachments list styling */
    .file-upload-zone {
      margin-bottom: 1rem;
    }

    .file-upload-label {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 0.5rem;
      border: 1.5px dashed var(--border-color);
      background: var(--bg-body);
      padding: 0.75rem;
      border-radius: var(--radius-md);
      cursor: pointer;
      font-weight: 600;
      font-size: 0.85rem;
      color: var(--text-secondary);
      transition: all var(--transition-fast);
    }

    .file-upload-label:hover {
      border-color: var(--primary-color);
      color: var(--primary-color);
      background: var(--primary-glow);
    }

    .attachments-list-items {
      list-style: none;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .attachment-row-item {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      background: var(--bg-body);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      padding: 0.5rem 0.75rem;
    }

    .file-type-icon {
      color: var(--text-secondary);
      font-size: 1.25rem;
    }

    .file-name-meta {
      flex: 1;
      display: flex;
      flex-direction: column;
      min-width: 0;
    }

    .file-name-txt {
      font-size: 0.85rem;
      font-weight: 600;
      color: var(--text-primary);
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .file-size-txt {
      font-size: 0.7rem;
      color: var(--text-muted);
    }

    .file-row-actions {
      display: flex;
      gap: 0.25rem;
    }

    .btn-icon-action {
      background: transparent;
      border: none;
      color: var(--text-secondary);
      cursor: pointer;
      padding: 0.25rem;
      border-radius: var(--radius-sm);
      display: flex;
      align-items: center;
    }

    .btn-icon-action:hover {
      background: var(--bg-hover);
      color: var(--text-primary);
    }

    .btn-icon-action.delete-act:hover {
      color: var(--danger-color);
      background: rgba(239, 68, 68, 0.08);
    }

    .btn-icon-action span {
      font-size: 1.15rem;
    }

    /* Links card styles */
    .add-link-inline-form, .add-worklog-inline-form {
      display: grid;
      grid-template-columns: 1fr 1fr auto;
      gap: 0.4rem;
    }

    .links-list-items {
      list-style: none;
      display: flex;
      flex-direction: column;
      gap: 0.4rem;
    }

    .link-row-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      background: var(--bg-body);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      padding: 0.4rem 0.65rem;
      font-size: 0.8rem;
    }

    .link-relation-label {
      font-weight: 700;
      color: var(--primary-color);
      text-transform: uppercase;
      font-size: 0.7rem;
    }

    .link-target-key {
      font-weight: 600;
      color: var(--text-primary);
      text-decoration: none;
    }

    .link-target-key:hover {
      text-decoration: underline;
    }

    /* Watchers UI elements */
    .watchers-avatar-grid {
      display: flex;
      flex-wrap: wrap;
      gap: 0.4rem;
    }

    .watcher-avatar-circle {
      width: 28px;
      height: 28px;
      border-radius: 50%;
      color: white;
      font-weight: 700;
      font-size: 0.75rem;
      display: flex;
      align-items: center;
      justify-content: center;
      box-shadow: var(--shadow-sm);
    }

    .watcher-actions {
      display: flex;
      gap: 0.5rem;
    }

    /* Worklog layout */
    .worklogs-list-items {
      display: flex;
      flex-direction: column;
      gap: 0.4rem;
      list-style: none;
    }

    .worklog-row-item {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.825rem;
    }

    .worklog-time-badge {
      background: var(--primary-glow);
      color: var(--primary-color);
      font-weight: 700;
      padding: 0.15rem 0.4rem;
      border-radius: var(--radius-sm);
    }

    .worklog-comment-text {
      color: var(--text-secondary);
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    /* View States */
    .loading-panel-view, .error-panel-view {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 6rem 2rem;
      text-align: center;
      border: 1px solid var(--border-color);
      background: var(--bg-card);
      border-radius: var(--radius-lg);
      box-shadow: var(--shadow-sm);
    }

    .error-icon-panel {
      font-size: 3rem;
      color: var(--danger-color);
      margin-bottom: 1rem;
    }

    .error-msg-txt {
      font-size: 1.1rem;
      color: var(--text-primary);
      margin-bottom: 1.5rem;
    }
  `,
})
export class TaskDetailComponent implements OnInit {
  readonly TaskStatus = TaskStatus;
  readonly TaskPriority = TaskPriority;
  readonly TaskLinkType = TaskLinkType;
  private readonly taskService = inject(TaskService);
  private readonly teamService = inject(TeamService);
  private readonly planning = inject(TaskPlanningService);
  private readonly commentService = inject(CommentService);
  private readonly fileService = inject(FileService);
  private readonly activityService = inject(ActivityService);
  private readonly realtime = inject(RealtimeService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly userService = inject(UserService);

  readonly usersMap = this.userService.usersMap;
  readonly task = signal<TaskResponse | null>(null);
  readonly comments = signal<CommentResponse[]>([]);
  readonly attachments = signal<FileAttachmentResponse[]>([]);
  readonly activities = signal<ActivityRecord[]>([]);
  readonly labels = signal<TaskLabelResponse[]>([]);
  readonly subTasks = signal<SubTaskResponse[]>([]);
  readonly watchers = signal<TaskWatcherResponse[]>([]);
  readonly workLogs = signal<WorkLogResponse[]>([]);
  readonly links = signal<TaskLinkResponse[]>([]);
  readonly sprints = signal<SprintResponse[]>([]);
  readonly epics = signal<EpicResponse[]>([]);
  readonly teams = signal<TeamResponse[]>([]);
  readonly error = signal<string | null>(null);

  readonly editForm = this.fb.group({
    title: ['', Validators.required],
    description: [''],
    priority: [TaskPriority.Medium, Validators.required],
    feDeveloper: [''],
    beDeveloper: [''],
    qaEngineer: [''],
    initialEta: [''],
    latestEta: [''],
    teamId: [''],
  });

  readonly commentForm = this.fb.group({
    body: ['', Validators.required],
  });

  readonly labelForm = this.fb.group({
    name: ['', Validators.required],
  });

  readonly subTaskForm = this.fb.group({
    title: ['', Validators.required],
  });

  readonly workLogForm = this.fb.group({
    timeSpentMinutes: [30, Validators.required],
    comment: [''],
  });

  readonly linkForm = this.fb.group({
    targetTaskId: ['', Validators.required],
    linkType: [TaskLinkType.RelatesTo, Validators.required],
  });

  ngOnInit(): void {
    const taskId = this.route.snapshot.paramMap.get('taskId');
    if (!taskId) {
      this.error.set('Task id missing');
      return;
    }

    this.loadUsers();
    void this.realtime.joinTask(taskId);
    this.loadTask(taskId);
    this.loadTaskExtras(taskId);
  }


  saveTask(): void {
    const taskId = this.task()?.taskId;
    if (!taskId || this.editForm.invalid) {
      return;
    }

    const raw = this.editForm.getRawValue();
    this.taskService
      .updateTask(taskId, {
        title: raw.title,
        description: raw.description || null,
        priority: Number(raw.priority) as TaskPriority,
        dueDate: this.task()?.dueDate ?? null,
        feDeveloper: raw.feDeveloper || null,
        beDeveloper: raw.beDeveloper || null,
        qaEngineer: raw.qaEngineer || null,
        initialEta: raw.initialEta ? new Date(raw.initialEta).toISOString() : null,
        latestEta: raw.latestEta ? new Date(raw.latestEta).toISOString() : null,
        teamId: raw.teamId || null,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (t) => this.task.set(t),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  assignToUser(user: UserLookup): void {
    const taskId = this.task()?.taskId;
    if (!taskId) {
      return;
    }

    this.taskService
      .assignTask(taskId, { assigneeId: user.userId })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (t) => this.task.set(t),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  clearAssignee(): void {
    const taskId = this.task()?.taskId;
    if (!taskId) {
      return;
    }

    this.taskService
      .assignTask(taskId, { assigneeId: null })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (t) => this.task.set(t),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  deleteTask(): void {
    const current = this.task();
    if (!current || !confirm(`Delete task ${current.issueKey}?`)) {
      return;
    }

    this.taskService
      .deleteTask(current.taskId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () =>
          void this.router.navigate(['/projects', current.projectId, 'tasks']),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  onStatusChange(event: Event): void {
    const taskId = this.task()?.taskId;
    if (!taskId) {
      return;
    }

    const status = Number((event.target as HTMLSelectElement).value) as TaskStatus;
    this.taskService
      .changeStatus(taskId, { status })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (t) => this.task.set(t),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  onStoryPointsChange(event: Event): void {
    const taskId = this.task()?.taskId;
    if (!taskId) {
      return;
    }

    const raw = (event.target as HTMLInputElement).value;
    const storyPoints = raw === '' ? null : Number(raw);
    this.taskService
      .updateStoryPoints(taskId, { storyPoints })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (t) => this.task.set(t),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  onSprintChange(event: Event): void {
    const taskId = this.task()?.taskId;
    if (!taskId) {
      return;
    }

    const value = (event.target as HTMLSelectElement).value;
    this.taskService
      .moveToSprint(taskId, { sprintId: value || null })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (t) => this.task.set(t),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  onEpicChange(event: Event): void {
    const taskId = this.task()?.taskId;
    if (!taskId) {
      return;
    }

    const value = (event.target as HTMLSelectElement).value;
    this.taskService
      .assignEpic(taskId, { epicId: value || null })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (t) => this.task.set(t),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  addLabel(): void {
    const taskId = this.task()?.taskId;
    if (!taskId || this.labelForm.invalid) {
      return;
    }

    this.taskService
      .addLabel(taskId, { name: this.labelForm.controls.name.value })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (label) => {
          this.labels.update((list) => [...list, label]);
          this.labelForm.reset();
        },
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  removeLabel(labelId: string): void {
    const taskId = this.task()?.taskId;
    if (!taskId) {
      return;
    }

    this.taskService
      .removeLabel(taskId, labelId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () =>
          this.labels.update((list) =>
            list.filter((l) => l.taskLabelId !== labelId),
          ),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  addSubTask(): void {
    const taskId = this.task()?.taskId;
    if (!taskId || this.subTaskForm.invalid) {
      return;
    }

    this.taskService
      .createSubTask(taskId, { title: this.subTaskForm.controls.title.value })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (st) => {
          this.subTasks.update((list) => [...list, st]);
          this.subTaskForm.reset();
        },
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  toggleSubTask(subTask: SubTaskResponse): void {
    this.taskService
      .changeSubTaskCompletion(subTask.subTaskId, {
        isCompleted: !subTask.isCompleted,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updated) =>
          this.subTasks.update((list) =>
            list.map((st) =>
              st.subTaskId === updated.subTaskId ? updated : st,
            ),
          ),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  deleteSubTask(subTaskId: string): void {
    this.taskService
      .deleteSubTask(subTaskId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () =>
          this.subTasks.update((list) =>
            list.filter((st) => st.subTaskId !== subTaskId),
          ),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  watch(): void {
    const taskId = this.task()?.taskId;
    if (!taskId) {
      return;
    }

    this.taskService
      .watchTask(taskId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (w) => this.watchers.update((list) => [...list, w]),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  unwatch(): void {
    const taskId = this.task()?.taskId;
    if (!taskId) {
      return;
    }

    this.taskService
      .unwatchTask(taskId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.loadTaskExtras(taskId),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  addWorkLog(): void {
    const taskId = this.task()?.taskId;
    if (!taskId || this.workLogForm.invalid) {
      return;
    }

    const raw = this.workLogForm.getRawValue();
    const request: CreateWorkLogRequest = {
      timeSpentMinutes: Number(raw.timeSpentMinutes),
      comment: raw.comment || null,
    };

    this.planning
      .addWorkLog(taskId, request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (log) => {
          this.workLogs.update((list) => [...list, log]);
          this.workLogForm.reset({ timeSpentMinutes: 30, comment: '' });
        },
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  addLink(): void {
    const taskId = this.task()?.taskId;
    if (!taskId || this.linkForm.invalid) {
      return;
    }

    const raw = this.linkForm.getRawValue();
    const request: CreateTaskLinkRequest = {
      targetTaskId: raw.targetTaskId,
      linkType: Number(raw.linkType) as TaskLinkType,
    };

    this.planning
      .createLink(taskId, request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (link) => {
          this.links.update((list) => [...list, link]);
          this.linkForm.reset({ linkType: TaskLinkType.RelatesTo });
        },
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  deleteLink(linkId: string): void {
    this.planning
      .deleteLink(linkId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () =>
          this.links.update((list) =>
            list.filter((l) => l.taskLinkId !== linkId),
          ),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  deleteComment(commentId: string): void {
    this.commentService
      .deleteComment(commentId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () =>
          this.comments.update((list) =>
            list.filter((c) => c.commentId !== commentId),
          ),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  addComment(): void {
    const taskId = this.task()?.taskId;
    if (!taskId || this.commentForm.invalid) {
      return;
    }

    this.commentService
      .createComment(taskId, { body: this.commentForm.controls.body.value })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (c) => {
          this.comments.update((list) => [...list, c]);
          this.commentForm.reset();
        },
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  onFileSelected(event: Event): void {
    const taskId = this.task()?.taskId;
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!taskId || !file) {
      return;
    }

    this.fileService
      .uploadAttachment(taskId, file)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (f) => this.attachments.update((list) => [...list, f]),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  download(file: FileAttachmentResponse): void {
    this.fileService
      .downloadAttachment(file.fileAttachmentId, file.originalFileName)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  removeFile(fileAttachmentId: string): void {
    this.fileService
      .deleteAttachment(fileAttachmentId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () =>
          this.attachments.update((list) =>
            list.filter((f) => f.fileAttachmentId !== fileAttachmentId),
          ),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  private loadTask(taskId: string): void {
    this.taskService
      .getTaskById(taskId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (t) => {
          this.task.set(t);
          this.editForm.patchValue({
            title: t.title,
            description: t.description ?? '',
            priority: t.priority,
            feDeveloper: t.feDeveloper ?? '',
            beDeveloper: t.beDeveloper ?? '',
            qaEngineer: t.qaEngineer ?? '',
            initialEta: t.initialEta ? t.initialEta.split('T')[0] : '',
            latestEta: t.latestEta ? t.latestEta.split('T')[0] : '',
            teamId: t.teamId ?? '',
          });
          this.loadProjectMeta(t.projectId);
        },
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  private loadProjectMeta(projectId: string): void {
    forkJoin({
      sprints: this.taskService.getProjectSprints(projectId),
      epics: this.taskService.getProjectEpics(projectId),
      teams: this.teamService.getTeams(),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ sprints, epics, teams }) => {
          this.sprints.set(sprints);
          this.epics.set(epics);
          this.teams.set(teams);
        },
      });
  }

  private loadTaskExtras(taskId: string): void {
    forkJoin({
      comments: this.commentService.getTaskComments(taskId),
      attachments: this.fileService.getTaskAttachments(taskId),
      activities: this.activityService.getEntityActivities('Task', taskId),
      labels: this.taskService.getLabels(taskId),
      subTasks: this.taskService.getSubTasks(taskId),
      watchers: this.taskService.getWatchers(taskId),
      workLogs: this.planning.getWorkLogs(taskId),
      links: this.planning.getLinks(taskId),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.comments.set(data.comments);
          this.attachments.set(data.attachments);
          this.activities.set(data.activities);
          this.labels.set(data.labels);
          this.subTasks.set(data.subTasks);
          this.watchers.set(data.watchers);
          this.workLogs.set(data.workLogs);
          this.links.set(data.links);
        },
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  getCompletedSubtasksCount(): number {
    return this.subTasks().filter((st) => st.isCompleted).length;
  }

  getLinkRelationLabel(type: TaskLinkType): string {
    switch (type) {
      case TaskLinkType.RelatesTo:
        return 'Relates to';
      case TaskLinkType.Blocks:
        return 'Blocks';
      case TaskLinkType.IsBlockedBy:
        return 'Is blocked by';
      default:
        return 'Links';
    }
  }

  getAssigneeColor(id: string | null): string {
    if (!id) return '#94a3b8';
    let hash = 0;
    for (let i = 0; i < id.length; i++) {
      hash = id.charCodeAt(i) + ((hash << 5) - hash);
    }
    const colors = [
      '#4f46e5',
      '#06b6d4',
      '#10b981',
      '#f59e0b',
      '#ec4899',
      '#8b5cf6',
    ];
    return colors[Math.abs(hash) % colors.length];
  }

  loadUsers(): void {
    this.userService.loadAllUsers();
  }

  getUserDisplayName(userId: string | null): string {
    return this.userService.getUserDisplayName(userId);
  }

  getUserInitials(userId: string | null): string {
    return this.userService.getUserInitials(userId);
  }

  getUserAvatarUrl(userId: string | null): string | null {
    return this.userService.getUserAvatarUrl(userId);
  }
}


