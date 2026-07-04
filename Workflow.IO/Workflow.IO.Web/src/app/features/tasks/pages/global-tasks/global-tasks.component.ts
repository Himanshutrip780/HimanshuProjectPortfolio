import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TaskResponse, TaskStatus, TaskPriority } from '../../../../core/models/task.models';
import { TaskService } from '../../services/task.service';
import { ProjectService } from '../../../projects/services/project.service';
import { UserService } from '../../../../core/services/user.service';
import { ProjectResponse } from '../../../../core/models/project.models';
import { UserDto } from '../../../../core/models/user.models';

@Component({
  selector: 'app-global-tasks',
  standalone: true,
  imports: [RouterLink, ReactiveFormsModule],
  templateUrl: './global-tasks.component.html',
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

    .page-header h1 {
      font-size: 1.5rem;
      font-weight: 700;
      letter-spacing: -0.025em;
      margin: 0;
    }

    .subtitle {
      font-size: 0.85rem;
      color: var(--text-secondary);
      margin-top: 0.15rem;
    }

    .badge-total {
      background-color: var(--primary-glow);
      color: var(--primary-color);
      font-size: 0.8rem;
      font-weight: 700;
      padding: 0.35rem 0.75rem;
      border-radius: var(--radius-md);
      border: 1px solid var(--border-color);
    }

    .filter-bar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      flex-wrap: wrap;
      gap: 1rem;
      padding: 1rem;
    }

    .filter-group {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      flex-wrap: wrap;
      flex: 1;
    }

    .search-box {
      display: flex;
      align-items: center;
      gap: 0.4rem;
      background-color: var(--bg-body);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      padding: 0.4rem 0.65rem;
      width: 260px;
    }

    .search-box span {
      color: var(--text-muted);
      font-size: 1.1rem;
    }

    .search-box input {
      background: transparent;
      border: none;
      outline: none;
      font-size: 0.8rem;
      color: var(--text-primary);
      width: 100%;
    }

    .select-wrapper select {
      padding: 0.4rem 2rem 0.4rem 0.65rem;
      font-size: 0.8rem;
      background-color: var(--bg-body);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      cursor: pointer;
    }

    .sort-group {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .sort-label {
      font-size: 0.8rem;
      color: var(--text-secondary);
      font-weight: 600;
    }

    .sort-group select {
      padding: 0.4rem 1.75rem 0.4rem 0.5rem;
      font-size: 0.8rem;
      background-color: var(--bg-body);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
    }

    /* Table styles */
    .table-container {
      background-color: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      overflow-x: auto;
      box-shadow: var(--shadow-sm);
    }

    .tasks-table {
      width: 100%;
      border-collapse: collapse;
      text-align: left;
      font-size: 0.85rem;
    }

    .tasks-table th {
      background-color: var(--bg-hover);
      color: var(--text-secondary);
      font-weight: 600;
      padding: 0.75rem 1rem;
      border-bottom: 1px solid var(--border-color);
    }

    .tasks-table td {
      padding: 0.75rem 1rem;
      border-bottom: 1px solid var(--border-color);
      vertical-align: middle;
      color: var(--text-primary);
    }

    .task-row {
      cursor: pointer;
      transition: background-color var(--transition-fast);
    }

    .task-row:hover {
      background-color: var(--bg-hover);
    }

    .task-key {
      font-weight: 700;
    }

    .key-tag {
      background-color: var(--bg-hover);
      border: 1px solid var(--border-color);
      padding: 0.15rem 0.4rem;
      border-radius: var(--radius-sm);
      color: var(--text-secondary);
    }

    .project-tag {
      border-left: 3px solid var(--primary-color);
      padding-left: 0.5rem;
      font-weight: 600;
      color: var(--text-secondary);
    }

    .task-title {
      font-weight: 500;
      color: var(--text-primary);
    }

    /* Badges */
    .status-badge {
      display: inline-flex;
      font-size: 0.7rem;
      font-weight: 700;
      padding: 0.15rem 0.5rem;
      border-radius: 9999px;
      text-transform: capitalize;
    }

    .status-todo { background-color: rgba(161, 161, 170, 0.15); color: var(--text-secondary); }
    .status-progress { background-color: rgba(59, 130, 246, 0.15); color: var(--info-color); }
    .status-review { background-color: rgba(245, 158, 11, 0.15); color: var(--warning-color); }
    .status-done { background-color: rgba(16, 185, 129, 0.15); color: var(--success-color); }
    .status-blocked { background-color: rgba(239, 68, 68, 0.15); color: var(--danger-color); }

    .priority-badge {
      display: inline-flex;
      align-items: center;
      gap: 0.25rem;
      font-size: 0.7rem;
      font-weight: 600;
      padding: 0.15rem 0.45rem;
      border-radius: var(--radius-sm);
    }

    .priority-low { background-color: rgba(161, 161, 170, 0.1); color: var(--text-secondary); }
    .priority-medium { background-color: rgba(59, 130, 246, 0.1); color: var(--info-color); }
    .priority-high { background-color: rgba(245, 158, 11, 0.1); color: var(--warning-color); }
    .priority-critical { background-color: rgba(239, 68, 68, 0.1); color: var(--danger-color); }

    .priority-icon {
      font-size: 0.9rem;
    }

    .assignee-cell {
      display: flex;
      align-items: center;
    }

    .avatar {
      width: 22px;
      height: 22px;
      border-radius: 50%;
      object-fit: cover;
    }

    .avatar-placeholder {
      width: 22px;
      height: 22px;
      border-radius: 50%;
      color: #fff;
      font-size: 0.6rem;
      font-weight: 700;
      display: flex;
      align-items: center;
      justify-content: center;
      text-transform: uppercase;
    }

    .unassigned {
      color: var(--text-muted);
      font-style: italic;
    }

    .due-date {
      color: var(--text-secondary);
    }

    .due-date.overdue {
      color: var(--danger-color);
      font-weight: 600;
    }

    /* States */
    .loading-state, .error-state, .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 4rem 2rem;
      background: var(--bg-card);
      border: 1px dashed var(--border-color);
      border-radius: var(--radius-lg);
      color: var(--text-secondary);
      text-align: center;
      gap: 1rem;
    }

    .loading-state span, .error-state span, .empty-state span {
      font-size: 2.5rem;
    }

    .spinner {
      width: 32px;
      height: 32px;
      border: 3px solid var(--border-color);
      border-top-color: var(--primary-color);
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }
  `
})
export class GlobalTasksComponent implements OnInit {
  readonly TaskStatus = TaskStatus;
  readonly TaskPriority = TaskPriority;

  private readonly taskService = inject(TaskService);
  private readonly projectService = inject(ProjectService);
  private readonly userService = inject(UserService);

  readonly tasks = signal<TaskResponse[]>([]);
  readonly projects = signal<ProjectResponse[]>([]);
  readonly usersMap = signal<Record<string, UserDto | undefined>>({});
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  readonly searchControl = new FormControl('', { nonNullable: true });
  readonly statusFilter = new FormControl('', { nonNullable: true });
  readonly priorityFilter = new FormControl('', { nonNullable: true });
  readonly projectFilter = new FormControl('', { nonNullable: true });
  readonly sortByControl = new FormControl('recent', { nonNullable: true });

  readonly filteredTasks = computed(() => {
    let list = [...this.tasks()];

    const query = this.searchControl.value.trim().toLowerCase();
    if (query) {
      list = list.filter(t =>
        t.title.toLowerCase().includes(query) ||
        t.issueKey.toLowerCase().includes(query)
      );
    }

    const statusVal = this.statusFilter.value;
    if (statusVal) {
      list = list.filter(t => t.status === Number(statusVal));
    }

    const priorityVal = this.priorityFilter.value;
    if (priorityVal) {
      list = list.filter(t => t.priority === Number(priorityVal));
    }

    const projectVal = this.projectFilter.value;
    if (projectVal) {
      list = list.filter(t => t.projectId === projectVal);
    }

    const sortBy = this.sortByControl.value;
    if (sortBy === 'recent') {
      list.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
    } else if (sortBy === 'priority') {
      list.sort((a, b) => b.priority - a.priority);
    } else if (sortBy === 'status') {
      list.sort((a, b) => a.status - b.status);
    } else if (sortBy === 'dueDate') {
      list.sort((a, b) => {
        if (!a.dueDate) return 1;
        if (!b.dueDate) return -1;
        return new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime();
      });
    }

    return list;
  });

  ngOnInit(): void {
    this.loadInitialData();
  }

  private loadInitialData(): void {
    this.loading.set(true);
    this.error.set(null);

    // Fetch users, projects and tasks in parallel
    this.userService.getAllUsers().subscribe({
      next: (users) => {
        const map: Record<string, UserDto> = {};
        for (const u of users) {
          map[u.userId.toLowerCase()] = u;
        }
        this.usersMap.set(map);
      }
    });

    this.projectService.getMyProjects().subscribe({
      next: (projects) => {
        this.projects.set(projects);
      }
    });

    this.taskService.getAllTasks().subscribe({
      next: (tasks) => {
        this.tasks.set(tasks);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load tasks. Make sure your gateway routes are correctly configured.');
        this.loading.set(false);
      }
    });
  }

  getProjectName(projectId: string): string {
    const proj = this.projects().find(p => p.projectId === projectId);
    return proj ? proj.name : 'Unknown';
  }

  getProjectColor(name: string): string {
    if (name === 'Unknown') return '#a1a1aa';
    const colors = ['#6366f1', '#3b82f6', '#ec4899', '#10b981', '#f59e0b'];
    let hash = 0;
    for (let i = 0; i < name.length; i++) {
      hash = name.charCodeAt(i) + ((hash << 5) - hash);
    }
    const index = Math.abs(hash) % colors.length;
    return colors[index];
  }

  getStatusLabel(status: TaskStatus): string {
    return TaskStatus[status] || String(status);
  }

  getStatusClass(status: TaskStatus): string {
    switch (status) {
      case TaskStatus.Todo: return 'status-todo';
      case TaskStatus.InProgress: return 'status-progress';
      case TaskStatus.Review: return 'status-review';
      case TaskStatus.Done: return 'status-done';
      case TaskStatus.Blocked: return 'status-blocked';
      default: return 'status-todo';
    }
  }

  getPriorityLabel(priority: TaskPriority): string {
    return TaskPriority[priority] || String(priority);
  }

  getPriorityClass(priority: TaskPriority): string {
    switch (priority) {
      case TaskPriority.Low: return 'priority-low';
      case TaskPriority.Medium: return 'priority-medium';
      case TaskPriority.High: return 'priority-high';
      case TaskPriority.Critical: return 'priority-critical';
      default: return 'priority-medium';
    }
  }

  getPriorityIcon(priority: TaskPriority): string {
    switch (priority) {
      case TaskPriority.Low: return 'arrow_downward';
      case TaskPriority.Medium: return 'drag_handle';
      case TaskPriority.High: return 'arrow_upward';
      case TaskPriority.Critical: return 'warning';
      default: return 'drag_handle';
    }
  }

  getUserInitials(userId: string): string {
    const user = this.usersMap()[userId.toLowerCase()];
    if (!user) return '??';
    const first = user.firstName ? user.firstName.charAt(0).toUpperCase() : '';
    const last = user.lastName ? user.lastName.charAt(0).toUpperCase() : '';
    return first + last || '?';
  }

  getUserBgColor(userId: string): string {
    const colors = ['#8b5cf6', '#3b82f6', '#10b981', '#f59e0b', '#ec4899', '#f43f5e', '#06b6d4'];
    let hash = 0;
    for (let i = 0; i < userId.length; i++) {
      hash = userId.charCodeAt(i) + ((hash << 5) - hash);
    }
    const index = Math.abs(hash) % colors.length;
    return colors[index];
  }

  formatDueDate(dateStr: string | null): string {
    if (!dateStr) return 'No due date';
    const date = new Date(dateStr);
    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    return `${months[date.getMonth()]} ${date.getDate()}, ${date.getFullYear()}`;
  }
}
