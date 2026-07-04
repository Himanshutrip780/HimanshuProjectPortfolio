import { Component, DestroyRef, inject, OnInit, signal, computed } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';
import {
  FormControl,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
  FormsModule,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { forkJoin } from 'rxjs';

import {
  TaskPriority,
  TaskResponse,
  TaskStatus,
  IssueType,
} from '../../../../core/models/task.models';
import { TeamResponse } from '../../../../core/models/team.models';
import { TaskPlanningService } from '../../../../core/services/task-planning.service';
import { getApiErrorMessage } from '../../../../core/utils/api-error.util';
import { TaskService } from '../../services/task.service';
import { TeamService } from '../../../teams/services/team.service';
import { ProjectService } from '../../../projects/services/project.service';
import { UserService } from '../../../../core/services/user.service';
import { UserDto } from '../../../../core/models/user.models';

interface PredefinedTask {
  id: string;
  title: string;
  description: string;
  issueType: IssueType;
}

interface PredefinedTaskGroup {
  name: string;
  role: string;
  tasks: PredefinedTask[];
}

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink, PaginationComponent],
  templateUrl: './task-list.component.html',
  styles: `
    .page-container {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    /* Toolbar Styling */
    .toolbar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.5rem 0;
      flex-wrap: wrap;
      gap: 0.75rem;
      width: 100%;
    }

    .toolbar-left {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      flex-shrink: 0;
    }

    .toolbar-title {
      font-size: 0.95rem;
      font-weight: 700;
      color: var(--text-primary);
    }

    .toolbar-badge {
      background-color: var(--bg-hover);
      color: var(--text-secondary);
      font-size: 0.7rem;
      font-weight: 700;
      padding: 0.05rem 0.45rem;
      border-radius: 9999px;
      border: 1px solid var(--border-color);
    }

    .toolbar-right {
      display: flex;
      align-items: center;
      gap: 0.65rem;
      flex-wrap: wrap;
      margin-left: auto;
    }

    .search-box {
      display: inline-flex;
      align-items: center;
      gap: 0.4rem;
      background-color: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      padding: 0.35rem 0.65rem;
      width: 200px;
      transition: border-color var(--transition-fast), box-shadow var(--transition-fast);
      box-sizing: border-box;
    }

    .search-box:focus-within {
      border-color: var(--primary-color);
      box-shadow: 0 0 0 2px var(--primary-glow);
    }

    .search-box span {
      font-size: 1rem;
      color: var(--text-muted);
    }

    .search-box input {
      background: transparent;
      border: none;
      outline: none;
      font-size: 0.8rem;
      color: var(--text-primary);
      width: 100%;
    }

    .search-box input::placeholder {
      color: var(--text-muted);
    }

    .select-toolbar {
      background-color: var(--bg-card);
      border: 1px solid var(--border-color);
      color: var(--text-secondary);
      font-size: 0.8rem;
      font-weight: 600;
      padding: 0.35rem 1.5rem 0.35rem 0.65rem;
      border-radius: var(--radius-md);
      cursor: pointer;
      outline: none;
      transition: all var(--transition-fast);
      appearance: none;
      width: auto !important;
      min-width: 120px;
      max-width: 160px;
      background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='%23a1a1aa' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpath d='m6 9 6 6 6-6'/%3E%3C/svg%3E");
      background-repeat: no-repeat;
      background-position: right 0.4rem center;
      background-size: 1rem;
    }

    .select-toolbar:hover {
      background-color: var(--bg-hover);
      color: var(--text-primary);
      border-color: var(--text-muted);
    }

    .view-toggles {
      display: flex;
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      overflow: hidden;
      background-color: var(--bg-card);
      flex-shrink: 0;
    }

    .btn-toggle {
      background: transparent;
      border: none;
      color: var(--text-muted);
      padding: 0.35rem 0.5rem;
      display: flex;
      align-items: center;
      cursor: pointer;
      transition: all var(--transition-fast);
    }

    .btn-toggle.active {
      background-color: var(--bg-hover);
      color: var(--primary-color);
    }

    .btn-toggle span {
      font-size: 1.1rem;
    }

    .page-header h1 {
      font-size: 1.5rem;
      font-weight: 700;
      letter-spacing: -0.025em;
      color: var(--text-primary);
      margin: 0;
    }

    .page-header .subtitle {
      font-size: 0.85rem;
      color: var(--text-secondary);
      margin-top: 0.15rem;
    }

    .header-actions {
      display: flex;
      gap: 0.75rem;
    }

    .btn-icon {
      font-size: 1.15rem;
    }

    /* Stats Grid */
    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      gap: 1rem;
    }

    .stat-card {
      background-color: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      padding: 1rem;
      display: flex;
      align-items: flex-start;
      gap: 0.75rem;
      box-shadow: var(--shadow-sm);
      transition: border-color var(--transition-fast), box-shadow var(--transition-fast);
    }

    .stat-card:hover {
      border-color: var(--primary-color);
      box-shadow: var(--shadow-hover);
    }

    .stat-icon-wrapper {
      width: 36px;
      height: 36px;
      border-radius: var(--radius-md);
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }

    .stat-icon-wrapper span {
      font-size: 1.25rem;
    }

    .tasks-icon { background-color: rgba(99, 102, 241, 0.1); color: #6366f1; }
    .todo-icon { background-color: rgba(59, 130, 246, 0.1); color: #3b82f6; }
    .progress-icon { background-color: rgba(245, 158, 11, 0.1); color: #f59e0b; }
    .completed-icon { background-color: rgba(16, 185, 129, 0.1); color: #10b981; }

    .stat-details {
      display: flex;
      flex-direction: column;
      gap: 0.15rem;
      flex: 1;
    }

    .stat-label {
      font-size: 0.75rem;
      font-weight: 600;
      color: var(--text-secondary);
    }

    .stat-value {
      font-size: 1.5rem;
      font-weight: 700;
      color: var(--text-primary);
      line-height: 1.1;
    }

    /* Tasks Grid Layout */
    .tasks-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
      gap: 1.25rem;
      width: 100%;
    }

    .task-card {
      position: relative;
      background-color: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-xl);
      padding: 1.25rem;
      text-decoration: none;
      color: inherit;
      box-shadow: var(--shadow-sm);
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
      transition: border-color var(--transition-fast), box-shadow var(--transition-fast), transform var(--transition-fast);
      overflow: hidden;
    }

    .task-card:hover {
      border-color: var(--primary-color);
      box-shadow: var(--shadow-hover);
      transform: translateY(-2px);
    }

    .task-card-glow {
      position: absolute;
      top: 0;
      left: 0;
      bottom: 0;
      width: 4px;
    }

    .task-card-glow.priority-low { background-color: var(--text-muted); }
    .task-card-glow.priority-medium { background-color: var(--warning-color); }
    .task-card-glow.priority-high { background-color: var(--danger-color); }
    .task-card-glow.priority-critical { background-color: var(--danger-color); }

    .task-card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .task-key {
      font-size: 0.75rem;
      font-weight: 700;
      color: var(--text-muted);
      letter-spacing: 0.05em;
    }

    .priority-label-badge {
      display: inline-flex;
      align-items: center;
      gap: 0.25rem;
      font-size: 0.7rem;
      font-weight: 600;
      padding: 0.15rem 0.45rem;
      border-radius: var(--radius-sm);
    }

    .priority-label-badge.priority-low { background-color: var(--bg-hover); color: var(--text-secondary); }
    .priority-label-badge.priority-medium { background-color: rgba(245, 158, 11, 0.12); color: var(--warning-color); }
    .priority-label-badge.priority-high { background-color: rgba(239, 68, 68, 0.1); color: var(--danger-color); }
    .priority-label-badge.priority-critical { background-color: rgba(239, 68, 68, 0.15); color: var(--danger-color); font-weight: bold; }

    .priority-icon {
      font-size: 0.9rem;
    }

    .task-title {
      font-size: 1rem;
      font-weight: 600;
      color: var(--text-primary);
      margin: 0;
      line-height: 1.4;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }

    .task-card-footer {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-top: 0.5rem;
      padding-top: 0.75rem;
      border-top: 1px solid var(--border-color);
    }

    .team-badge {
      display: inline-flex;
      align-items: center;
      gap: 0.25rem;
      font-size: 0.75rem;
      color: var(--text-secondary);
      font-weight: 500;
    }

    .team-badge span {
      font-size: 0.95rem;
      color: var(--text-muted);
    }

    .status-badge {
      display: inline-flex;
      align-items: center;
      font-size: 0.7rem;
      font-weight: 600;
      padding: 0.15rem 0.55rem;
      border-radius: 9999px;
      text-transform: uppercase;
      letter-spacing: 0.02em;
    }

    .status-todo { background-color: rgba(99, 102, 241, 0.12); color: var(--primary-color); }
    .status-progress { background-color: rgba(245, 158, 11, 0.12); color: var(--warning-color); }
    .status-review { background-color: rgba(59, 130, 246, 0.12); color: var(--info-color); }
    .status-done { background-color: rgba(16, 185, 129, 0.12); color: var(--success-color); }
    .status-blocked { background-color: rgba(239, 68, 68, 0.12); color: var(--danger-color); }

    .assignee-avatar {
      width: 24px;
      height: 24px;
      border-radius: 50%;
      background-color: var(--primary-color);
      color: #fff;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 0.65rem;
      font-weight: 700;
    }

    .assignee-avatar.unassigned {
      background-color: var(--bg-hover);
      color: var(--text-muted);
      border: 1px dashed var(--border-color);
    }

    /* Badges */
    .badge-outline {
      background: transparent;
      border: 1.5px solid currentColor;
      padding: 0.05rem 0.35rem;
      font-size: 0.65rem;
    }
    
    .type-story { color: var(--success-color); }
    .type-task { color: var(--primary-color); }
    .type-bug { color: var(--danger-color); }
    .type-subtask { color: var(--info-color); }

    /* Developer Badges */
    .dev-badge {
      display: inline-flex;
      align-items: center;
      font-size: 0.65rem;
      font-weight: 600;
      padding: 0.1rem 0.45rem;
      border-radius: 999px;
      letter-spacing: 0.02em;
    }
    .dev-badge.fe { background: rgba(79, 70, 229, 0.12); color: #4f46e5; }
    .dev-badge.be { background: rgba(6, 182, 212, 0.12); color: #06b6d4; }
    .dev-badge.qa { background: rgba(16, 185, 129, 0.12); color: #10b981; }

    /* Table styles for List view */
    .tasks-list-table {
      width: 100%;
    }
    .task-list-row {
      position: relative;
    }
    .task-list-row:hover {
      background-color: var(--bg-hover) !important;
    }
    .row-glow {
      position: absolute;
      left: 0;
      top: 0;
      bottom: 0;
      width: 4px;
    }
    .row-glow.priority-low { background-color: var(--text-muted); }
    .row-glow.priority-medium { background-color: var(--warning-color); }
    .row-glow.priority-high { background-color: var(--danger-color); }
    .row-glow.priority-critical { background-color: var(--danger-color); }

    .btn-toggle-mode.active {
      background-color: var(--bg-card) !important;
      color: var(--primary-color) !important;
      box-shadow: var(--shadow-sm);
    }

    /* Modal Backdrop & Contents */
    .modal-backdrop {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.4);
      backdrop-filter: blur(4px);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1100;
      animation: fadeIn var(--transition-fast) forwards;
    }

    .modal-content {
      background: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-xl);
      width: 100%;
      max-width: 500px;
      box-shadow: var(--shadow-lg);
      overflow: hidden;
      animation: slideUp var(--transition-normal) var(--motion-easing) forwards;
    }

    .predefined-modal {
      max-width: 760px;
      max-height: 85vh;
      display: flex;
      flex-direction: column;
    }

    .modal-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1.25rem 1.5rem;
      border-bottom: 1px solid var(--border-color);
    }

    .modal-header h2 {
      margin: 0;
      font-size: 1.25rem;
    }

    .btn-close-modal {
      background: transparent;
      border: none;
      color: var(--text-muted);
      padding: 0.25rem;
      border-radius: 50%;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .btn-close-modal:hover {
      background: var(--bg-hover);
      color: var(--text-primary);
    }

    .modal-form {
      padding: 1.5rem;
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: 0.4rem;
    }

    .form-group label {
      font-size: 0.875rem;
      font-weight: 500;
      color: var(--text-secondary);
    }

    .required {
      color: var(--danger-color);
    }

    .modal-actions {
      display: flex;
      justify-content: flex-end;
      gap: 0.75rem;
      margin-top: 0.5rem;
    }

    .spinner {
      width: 40px;
      height: 40px;
      border: 3px solid var(--border-color);
      border-top-color: var(--primary-color);
      border-radius: 50%;
      animation: spin 1s linear infinite;
      margin: 2rem auto;
    }

    .spinner-sm {
      width: 16px;
      height: 16px;
      border: 2px solid rgba(255, 255, 255, 0.3);
      border-top-color: white;
      border-radius: 50%;
      animation: spin 1s linear infinite;
      display: inline-block;
    }

    /* Predefined Modal specific styling */
    .modal-body-scrollable {
      padding: 1.5rem;
      overflow-y: auto;
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }

    .modal-instruction {
      font-size: 0.9rem;
      color: var(--text-secondary);
    }

    .predefined-global-actions {
      display: flex;
      gap: 0.5rem;
    }

    .btn-sm {
      padding: 0.35rem 0.75rem;
      font-size: 0.8rem;
    }

    .predefined-groups-list {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    .predefined-group-card {
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      background-color: var(--bg-hover);
      padding: 1rem;
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }

    .group-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      border-bottom: 1px solid var(--border-color);
      padding-bottom: 0.5rem;
    }

    .group-header-left {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .group-icon {
      color: var(--primary-color);
      font-size: 1.25rem;
    }

    .group-header h3 {
      font-size: 1rem;
      font-weight: 700;
      color: var(--text-primary);
    }

    .btn-text-action {
      background: transparent;
      border: none;
      color: var(--primary-color);
      font-size: 0.8rem;
      font-weight: 600;
      cursor: pointer;
      padding: 0.2rem 0.4rem;
      border-radius: var(--radius-sm);
    }

    .btn-text-action:hover {
      background-color: var(--primary-glow);
    }

    .group-tasks-list {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 0.75rem;
    }

    @media (max-width: 600px) {
      .group-tasks-list {
        grid-template-columns: 1fr;
      }
    }

    .predefined-task-item {
      display: flex;
      align-items: flex-start;
      gap: 0.6rem;
      background-color: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      padding: 0.75rem;
      cursor: pointer;
      transition: all var(--transition-fast);
    }

    .predefined-task-item:hover {
      border-color: var(--text-secondary);
      background-color: var(--bg-hover);
    }

    .predefined-task-item.selected {
      border-color: var(--primary-color);
      background-color: var(--primary-glow);
    }

    .task-checkbox-wrapper {
      padding-top: 0.15rem;
    }

    .task-checkbox-wrapper input[type="checkbox"] {
      cursor: pointer;
      width: 15px;
      height: 15px;
    }

    .task-item-details {
      display: flex;
      flex-direction: column;
      gap: 0.2rem;
      flex: 1;
    }

    .task-item-title-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 0.5rem;
    }

    .task-item-title {
      font-size: 0.85rem;
      font-weight: 600;
      color: var(--text-primary);
    }

    .task-item-desc {
      font-size: 0.75rem;
      color: var(--text-secondary);
      line-height: 1.3;
    }

    .predefined-config-section {
      padding: 1rem 1.5rem;
      background-color: var(--bg-hover);
      border-top: 1px solid var(--border-color);
    }

    .form-row-2 {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
    }

    .modal-footer {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1rem 1.5rem;
      border-top: 1px solid var(--border-color);
      background-color: var(--bg-card);
    }

    .selection-count {
      font-size: 0.85rem;
      font-weight: 600;
      color: var(--text-secondary);
    }

    .loading-state, .error-state, .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      text-align: center;
      padding: 5rem 2rem;
      background: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      box-shadow: var(--shadow-sm);
    }

    .loading-state p, .error-state p, .empty-state p {
      margin-top: 1rem;
      color: var(--text-secondary);
    }

    @keyframes fadeIn {
      from { opacity: 0; }
      to { opacity: 1; }
    }

    @keyframes slideUp {
      from { transform: translateY(20px); opacity: 0; }
      to { transform: translateY(0); opacity: 1; }
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }
  `,

})
export class TaskListComponent implements OnInit {
  readonly TaskPriority = TaskPriority;
  readonly TaskStatus = TaskStatus;
  readonly IssueType = IssueType;

  private readonly taskService = inject(TaskService);
  private readonly teamService = inject(TeamService);
  private readonly projectService = inject(ProjectService);
  private readonly userService = inject(UserService);
  private readonly planning = inject(TaskPlanningService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  // States
  readonly currentProjectName = signal<string>('SaaS Workspace');
  readonly currentProjectDesc = signal<string>('');
  readonly tasks = signal<TaskResponse[]>([]);
  readonly teams = signal<TeamResponse[]>([]);
  readonly usersMap = this.userService.usersMap;
  readonly loading = signal(true);
  readonly creating = signal(false);
  readonly error = signal<string | null>(null);
  readonly viewMode = signal<'grid' | 'list'>('grid');

  readonly expandedTasks = signal<Record<string, boolean>>({});

  toggleSubtasks(taskId: string): void {
    this.expandedTasks.update(map => ({
      ...map,
      [taskId]: !map[taskId]
    }));
  }

  hasSubtasks(taskId: string): boolean {
    return this.tasks().some(t => t.parentTaskId === taskId);
  }

  getSubtasks(taskId: string): TaskResponse[] {
    return this.tasks().filter(t => t.parentTaskId === taskId);
  }

  readonly potentialParents = computed(() => {
    return this.tasks().filter(t => !t.parentTaskId);
  });

  readonly currentPage = signal(1);
  readonly pageSize = signal(10);

  readonly pagedTasks = computed(() => {
    const list = this.filteredTasks();
    const startIndex = (this.currentPage() - 1) * this.pageSize();
    return list.slice(startIndex, startIndex + this.pageSize());
  });

  // Live filter and sort signals
  readonly querySig = signal('');
  readonly statusSig = signal('');
  readonly prioritySig = signal('');
  readonly teamIdSig = signal('');
  readonly sortByControl = new FormControl('recent', { nonNullable: true });
  readonly sortBySig = signal('recent');

  readonly filteredTasks = computed(() => {
    let list = [...this.tasks()];
    const query = this.querySig().toLowerCase().trim();
    const statusVal = this.statusSig();
    const priorityVal = this.prioritySig();
    const teamIdVal = this.teamIdSig();

    if (!query && !statusVal && !priorityVal && !teamIdVal) {
      list = list.filter(t => !t.parentTaskId);
    }

    if (query) {
      list = list.filter((t) =>
        t.title.toLowerCase().includes(query) ||
        t.issueKey.toLowerCase().includes(query) ||
        (t.description && t.description.toLowerCase().includes(query))
      );
    }

    if (statusVal) {
      list = list.filter((t) => String(t.status) === statusVal);
    }

    if (priorityVal) {
      list = list.filter((t) => String(t.priority) === priorityVal);
    }

    if (teamIdVal) {
      list = list.filter((t) => t.teamId === teamIdVal);
    }

    const sortBy = this.sortBySig();
    if (sortBy === 'recent') {
      list.sort((a, b) => new Date(b.createdAt || 0).getTime() - new Date(a.createdAt || 0).getTime());
    } else if (sortBy === 'priority') {
      list.sort((a, b) => (b.priority || 0) - (a.priority || 0));
    } else if (sortBy === 'dueDate') {
      list.sort((a, b) => {
        if (!a.dueDate) return 1;
        if (!b.dueDate) return -1;
        return new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime();
      });
    } else if (sortBy === 'storyPoints') {
      list.sort((a, b) => (b.storyPoints || 0) - (a.storyPoints || 0));
    } else if (sortBy === 'title') {
      list.sort((a, b) => a.title.localeCompare(b.title));
    }

    return list;
  });

  // Modals visibility
  readonly showCreateModal = signal(false);
  readonly showPredefinedModal = signal(false);

  // Stats computed
  readonly totalTasksCount = computed(() => this.tasks().length);
  readonly todoTasksCount = computed(() => this.tasks().filter(t => t.status === TaskStatus.Todo).length);
  readonly inProgressTasksCount = computed(() => this.tasks().filter(t => t.status === TaskStatus.InProgress || t.status === TaskStatus.Review).length);
  readonly completedTasksCount = computed(() => this.tasks().filter(t => t.status === TaskStatus.Done).length);

  // Forms
  readonly createForm = this.fb.group({
    title: ['', Validators.required],
    priority: [TaskPriority.Medium, Validators.required],
    teamId: [''],
    parentTaskId: [''],
  });

  readonly searchForm = this.fb.group({
    query: [''],
    status: [''],
    priority: [''],
    teamId: [''],
  });

  readonly issueKeyLookup = new FormControl('', {
    nonNullable: true,
    validators: Validators.required,
  });

  // Predefined tasks state
  readonly selectedTaskIds = signal<Record<string, boolean>>({});
  predefinedPriority: TaskPriority = TaskPriority.Medium;
  predefinedTeamId = '';

  readonly predefinedGroups = computed<PredefinedTaskGroup[]>(() => {
    return this.generateCustomPredefinedGroups(this.currentProjectName(), this.currentProjectDesc());
  });

  generateCustomPredefinedGroups(projectName: string, description: string): PredefinedTaskGroup[] {
    const nameLower = projectName.toLowerCase();
    const descLower = (description || '').toLowerCase();
    const groups: PredefinedTaskGroup[] = [];

    const isMarketingOrSales = nameLower.includes('market') || nameLower.includes('sale') || nameLower.includes('campaign') || nameLower.includes('promo') || descLower.includes('market') || descLower.includes('sale');
    const isDesignOrCreative = nameLower.includes('design') || nameLower.includes('creative') || nameLower.includes('ui') || nameLower.includes('ux') || nameLower.includes('art') || descLower.includes('design') || descLower.includes('creative');
    const isDataOrAnalytics = nameLower.includes('data') || nameLower.includes('analytics') || nameLower.includes('db') || nameLower.includes('sql') || descLower.includes('data') || descLower.includes('analytics') || descLower.includes('database');
    const isMobile = nameLower.includes('mobile') || nameLower.includes('app') || nameLower.includes('ios') || nameLower.includes('android') || descLower.includes('mobile') || descLower.includes('ios');

    // Group 1: Core Setup
    groups.push({
      name: `Core Setup & Strategy for ${projectName}`,
      role: 'Project Coordinator',
      tasks: [
        {
          id: 'core-scope',
          title: `Define Scope & Milestones for ${projectName}`,
          description: `Map out the key requirements, deliverables, and timelines based on: "${description || 'No description provided'}"`,
          issueType: IssueType.Task
        },
        {
          id: 'core-onboard',
          title: `Onboard Stakeholders to ${projectName}`,
          description: 'Set up communication channels, permissions, and roles for the team.',
          issueType: IssueType.Task
        },
        {
          id: 'core-resources',
          title: 'Resource Allocation & Workspace Setup',
          description: `Configure shared folders, environments, and tracking boards specific to ${projectName}.`,
          issueType: IssueType.Task
        }
      ]
    });

    // Group 2: Niche customized tasks
    if (isMarketingOrSales) {
      groups.push({
        name: 'Launch & Promotion Campaign',
        role: 'Growth Marketer',
        tasks: [
          {
            id: 'mkt-content',
            title: 'Draft Campaign Copy & Assets',
            description: `Write and review copy tailored for the launch of ${projectName}.`,
            issueType: IssueType.Story
          },
          {
            id: 'mkt-channels',
            title: 'Configure Marketing Channels',
            description: 'Set up newsletters, social posts, and ads tracking for the campaign.',
            issueType: IssueType.Task
          },
          {
            id: 'mkt-analytics',
            title: 'Establish Lead Attribution & Goal Tracking',
            description: 'Verify conversion tracking pixels and dashboard integrations.',
            issueType: IssueType.Task
          }
        ]
      });
    } else if (isDesignOrCreative) {
      groups.push({
        name: 'Creative Direction & Prototyping',
        role: 'Lead Designer',
        tasks: [
          {
            id: 'dsn-wireframes',
            title: 'Low-Fidelity Wireframing',
            description: `Sketch layouts and initial mockups for ${projectName}.`,
            issueType: IssueType.Task
          },
          {
            id: 'dsn-palette',
            title: 'Design System & Color Palette',
            description: 'Establish typography, primary/secondary colors, and spacing tokens.',
            issueType: IssueType.Task
          },
          {
            id: 'dsn-prototype',
            title: 'Interactive Prototype Feedback Session',
            description: 'Build clickable flow for client verification and user testing.',
            issueType: IssueType.Story
          }
        ]
      });
    } else if (isDataOrAnalytics) {
      groups.push({
        name: 'Data Engineering & Pipeline Setup',
        role: 'Data Architect',
        tasks: [
          {
            id: 'data-schema',
            title: 'Design Data Models & Storage Schema',
            description: 'Draft table schemas, collections, relationships, and index configurations.',
            issueType: IssueType.Task
          },
          {
            id: 'data-pipeline',
            title: 'Configure ETL Pipelines & Sync',
            description: 'Setup automatic data flows, validations, and error handling loops.',
            issueType: IssueType.Task
          },
          {
            id: 'data-security',
            title: 'Implement Row-Level Security & Compliance',
            description: 'Restrict access to sensitive columns and encrypt resting data.',
            issueType: IssueType.Task
          }
        ]
      });
    } else if (isMobile) {
      groups.push({
        name: 'Mobile Client Experience',
        role: 'Mobile Lead',
        tasks: [
          {
            id: 'mob-setup',
            title: 'Scaffold App Boilerplate & Environment Configuration',
            description: 'Configure separate build pipelines for iOS and Android environments.',
            issueType: IssueType.Task
          },
          {
            id: 'mob-layout',
            title: 'Responsive Screen Layout Implementation',
            description: 'Code the navigation tabs, shell layout, and responsive layouts.',
            issueType: IssueType.Task
          },
          {
            id: 'mob-store',
            title: 'Prepare App Store Metadata & Screenshots',
            description: 'Collect descriptions, keywords, privacy policies, and promotional images for review.',
            issueType: IssueType.Task
          }
        ]
      });
    } else {
      // Default general SaaS features group based on Project Name
      groups.push({
        name: `Core Features for ${projectName}`,
        role: 'Product Specialist',
        tasks: [
          {
            id: 'saas-landing',
            title: `Structure Landing Page for ${projectName}`,
            description: `Create an elegant showcase outlining the value proposition of ${projectName}.`,
            issueType: IssueType.Story
          },
          {
            id: 'saas-onboard',
            title: 'User Onboarding & Signup Flow',
            description: 'Design a seamless registration process with standard verification steps.',
            issueType: IssueType.Story
          },
          {
            id: 'saas-settings',
            title: 'Customer Dashboard & Configuration Panel',
            description: 'Provide an interface for customers to view and configure their workspace details.',
            issueType: IssueType.Task
          }
        ]
      });
    }

    // Group 3: Operations & Verification
    groups.push({
      name: 'Operational Excellence & Verification',
      role: 'Operations Lead',
      tasks: [
        {
          id: 'ops-verify',
          title: 'Execute Quality Checklist & Audit',
          description: 'Verify usability, verify layout alignment, and ensure error-free performance.',
          issueType: IssueType.Task
        },
        {
          id: 'ops-deploy',
          title: `Deploy ${projectName} to Staging/Production`,
          description: 'Build production bundles, configure SSL/DNS settings, and complete initial smoke tests.',
          issueType: IssueType.Task
        },
        {
          id: 'ops-feedback',
          title: 'Post-Launch Feedback & Iteration Log',
          description: 'Monitor active usage, collect initial support queries, and document updates.',
          issueType: IssueType.Story
        }
      ]
    });

    return groups;
  }

  ngOnInit(): void {
    this.loadUsers();
    this.loadTasks();

    this.searchForm.valueChanges.subscribe(() => {
      const raw = this.searchForm.getRawValue();
      this.querySig.set(raw.query || '');
      this.statusSig.set(raw.status || '');
      this.prioritySig.set(raw.priority || '');
      this.teamIdSig.set(raw.teamId || '');
      this.currentPage.set(1);
    });

    this.sortByControl.valueChanges.subscribe(val => {
      this.sortBySig.set(val || 'recent');
      this.currentPage.set(1);
    });
  }

  openByIssueKey(): void {
    const key = this.issueKeyLookup.value.trim().toUpperCase();
    if (!key) {
      return;
    }

    this.planning
      .getByIssueKey(key)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (task) => void this.router.navigate(['/tasks', task.taskId]),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  search(): void {
    // Live reactive filtering is performed client-side on the task list.
  }

  clearSearch(): void {
    this.searchForm.reset();
    this.sortByControl.setValue('recent');
    this.currentPage.set(1);
    this.loadTasks();
  }

  onPageChange(page: number): void {
    this.currentPage.set(page);
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.currentPage.set(1);
  }

  createTask(): void {
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    if (!projectId || this.createForm.invalid) {
      return;
    }

    this.creating.set(true);
    const raw = this.createForm.getRawValue();
    this.taskService
      .createTask(projectId, {
        title: raw.title,
        priority: Number(raw.priority) as TaskPriority,
        teamId: raw.teamId || null,
        parentTaskId: raw.parentTaskId || null,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.createForm.reset({ priority: TaskPriority.Medium, teamId: '' });
          this.creating.set(false);
          this.closeCreateModal();
          this.loadTasks();
        },
        error: (err) => {
          this.error.set(getApiErrorMessage(err));
          this.creating.set(false);
        },
      });
  }

  private loadTasks(): void {
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    if (!projectId) {
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.teamService.getTeams().subscribe({
      next: (list: TeamResponse[]) => this.teams.set(list),
    });

    this.projectService.getProjectById(projectId).subscribe({
      next: (proj) => {
        this.currentProjectName.set(proj.name);
        this.currentProjectDesc.set(proj.description || '');
      }
    });

    this.taskService
      .getProjectTasks(projectId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (tasks) => {
          this.tasks.set(tasks);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(getApiErrorMessage(err));
          this.loading.set(false);
        },
      });
  }

  // Predefined modal helpers
  openCreateModal(): void {
    this.createForm.reset({ priority: TaskPriority.Medium, teamId: '' });
    this.showCreateModal.set(true);
  }

  closeCreateModal(): void {
    this.showCreateModal.set(false);
  }

  openPredefinedModal(): void {
    this.selectedTaskIds.set({});
    this.predefinedPriority = TaskPriority.Medium;
    this.predefinedTeamId = '';
    this.showPredefinedModal.set(true);
  }

  closePredefinedModal(): void {
    this.showPredefinedModal.set(false);
  }

  toggleTaskSelection(taskId: string): void {
    this.selectedTaskIds.update(ids => ({
      ...ids,
      [taskId]: !ids[taskId]
    }));
  }

  isGroupSelected(group: PredefinedTaskGroup): boolean {
    return group.tasks.every(t => !!this.selectedTaskIds()[t.id]);
  }

  toggleGroupSelection(group: PredefinedTaskGroup): void {
    const isSelected = this.isGroupSelected(group);
    this.selectedTaskIds.update(ids => {
      const next = { ...ids };
      group.tasks.forEach(t => {
        next[t.id] = !isSelected;
      });
      return next;
    });
  }

  selectAllPredefined(): void {
    this.selectedTaskIds.update(() => {
      const next: Record<string, boolean> = {};
      this.predefinedGroups().forEach(g => {
        g.tasks.forEach(t => {
          next[t.id] = true;
        });
      });
      return next;
    });
  }

  deselectAllPredefined(): void {
    this.selectedTaskIds.update(() => ({}));
  }

  getSelectedCount(): number {
    return Object.values(this.selectedTaskIds()).filter(Boolean).length;
  }

  createPredefinedTasks(): void {
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    if (!projectId) {
      return;
    }

    this.creating.set(true);
    this.error.set(null);

    const selectedTasksList: PredefinedTask[] = [];
    this.predefinedGroups().forEach(g => {
      g.tasks.forEach(t => {
        if (this.selectedTaskIds()[t.id]) {
          selectedTasksList.push(t);
        }
      });
    });

    if (selectedTasksList.length === 0) {
      this.creating.set(false);
      return;
    }

    const priorityVal = Number(this.predefinedPriority) as TaskPriority;
    const teamIdVal = this.predefinedTeamId || null;

    const requests = selectedTasksList.map(t =>
      this.taskService.createTask(projectId, {
        title: t.title,
        description: t.description,
        priority: priorityVal,
        issueType: t.issueType,
        teamId: teamIdVal,
      })
    );

    forkJoin(requests)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.creating.set(false);
          this.closePredefinedModal();
          this.loadTasks();
        },
        error: (err) => {
          this.error.set(getApiErrorMessage(err));
          this.creating.set(false);
        }
      });
  }

  // Display label helpers
  getPriorityClass(priority: TaskPriority): string {
    switch (priority) {
      case TaskPriority.Low: return 'priority-low';
      case TaskPriority.Medium: return 'priority-medium';
      case TaskPriority.High: return 'priority-high';
      case TaskPriority.Critical: return 'priority-critical';
      default: return '';
    }
  }

  getPriorityLabel(priority: TaskPriority): string {
    return TaskPriority[priority] ?? String(priority);
  }

  getPriorityIcon(priority: TaskPriority): string {
    switch (priority) {
      case TaskPriority.Low: return 'keyboard_arrow_down';
      case TaskPriority.Medium: return 'drag_handle';
      case TaskPriority.High: return 'keyboard_arrow_up';
      case TaskPriority.Critical: return 'double_arrow';
      default: return 'help';
    }
  }

  getIssueTypeClass(type: IssueType): string {
    switch (type) {
      case IssueType.Story: return 'type-story';
      case IssueType.Task: return 'type-task';
      case IssueType.Bug: return 'type-bug';
      case IssueType.SubTask: return 'type-subtask';
      default: return '';
    }
  }

  getIssueTypeLabel(type: IssueType): string {
    return IssueType[type] ?? String(type);
  }

  getStatusLabel(status: TaskStatus): string {
    switch (status) {
      case TaskStatus.Todo: return 'To Do';
      case TaskStatus.InProgress: return 'In Progress';
      case TaskStatus.Review: return 'Review';
      case TaskStatus.Done: return 'Done';
      case TaskStatus.Blocked: return 'Blocked';
      default: return 'Unknown';
    }
  }

  getStatusClass(status: TaskStatus): string {
    switch (status) {
      case TaskStatus.Todo: return 'status-todo';
      case TaskStatus.InProgress: return 'status-progress';
      case TaskStatus.Review: return 'status-review';
      case TaskStatus.Done: return 'status-done';
      case TaskStatus.Blocked: return 'status-blocked';
      default: return '';
    }
  }

  getTeamName(teamId: string): string {
    const team = this.teams().find(t => t.teamId === teamId);
    return team ? team.name : 'Unknown Team';
  }

  getAssigneeColor(id: string | null): string {
    if (!id) return '#94a3b8';
    let hash = 0;
    for (let i = 0; i < id.length; i++) {
      hash = id.charCodeAt(i) + ((hash << 5) - hash);
    }
    const colors = ['#4f46e5', '#06b6d4', '#10b981', '#f59e0b', '#ec4899', '#8b5cf6'];
    return colors[Math.abs(hash) % colors.length];
  }

  formatDate(dateStr: string | null | undefined): string {
    if (!dateStr) return '—';
    try {
      return new Date(dateStr).toLocaleDateString('en-GB', { day: '2-digit', month: 'short' });
    } catch {
      return dateStr;
    }
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
