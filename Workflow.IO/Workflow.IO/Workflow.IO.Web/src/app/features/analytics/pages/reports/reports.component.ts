import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { TaskResponse, TaskStatus, TaskPriority } from '../../../../core/models/task.models';
import { ProjectResponse } from '../../../../core/models/project.models';
import { TaskService } from '../../../tasks/services/task.service';
import { ProjectService } from '../../../projects/services/project.service';

@Component({
  selector: 'app-reports',
  standalone: true,
  templateUrl: './reports.component.html',
  styles: `
    .page-container {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
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

    .reports-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
      gap: 1.25rem;
    }

    .report-card {
      background-color: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-xl);
      padding: 1.25rem;
      box-shadow: var(--shadow-sm);
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 1.25rem;
    }

    .report-card-label {
      font-size: 0.8rem;
      font-weight: 700;
      color: var(--text-secondary);
      text-transform: uppercase;
      letter-spacing: 0.05em;
      align-self: flex-start;
    }

    .health-gauge {
      position: relative;
      width: 120px;
      height: 120px;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .donut-svg {
      transform: rotate(-90deg);
    }

    .gauge-text {
      position: absolute;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
    }

    .gauge-value {
      font-size: 1.45rem;
      font-weight: 700;
      color: var(--text-primary);
      line-height: 1;
    }

    .gauge-sub {
      font-size: 0.6rem;
      color: var(--text-secondary);
      font-weight: 600;
    }

    .card-summary {
      font-size: 0.8rem;
      color: var(--text-secondary);
      font-weight: 500;
    }

    /* Breakdown Bar Charts */
    .progress-report, .priority-report {
      align-items: stretch;
    }

    .bar-chart-container {
      display: flex;
      flex-direction: column;
      gap: 0.85rem;
    }

    .chart-bar-item {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
    }

    .item-label-group {
      display: flex;
      justify-content: space-between;
      font-size: 0.75rem;
      font-weight: 600;
    }

    .item-label {
      color: var(--text-primary);
    }

    .item-count {
      color: var(--text-secondary);
    }

    .progress-bar-track {
      height: 6px;
      background-color: var(--bg-hover);
      border-radius: 99px;
      overflow: hidden;
    }

    .progress-bar-fill {
      height: 100%;
      border-radius: 99px;
    }

    /* Colors mapping */
    .bar-todo { background-color: var(--text-muted); }
    .bar-progress { background-color: var(--info-color); }
    .bar-review { background-color: var(--warning-color); }
    .bar-done { background-color: var(--success-color); }
    .bar-blocked { background-color: var(--danger-color); }

    .bar-low { background-color: #a1a1aa; }
    .bar-medium { background-color: var(--info-color); }
    .bar-high { background-color: var(--warning-color); }
    .bar-critical { background-color: var(--danger-color); }

    /* Index Table */
    .performance-table-container {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .performance-table-container h3 {
      font-size: 0.95rem;
      font-weight: 700;
      color: var(--text-primary);
      margin: 0;
    }

    .table-scroll {
      overflow-x: auto;
    }

    .report-table {
      width: 100%;
      border-collapse: collapse;
      font-size: 0.85rem;
      text-align: left;
    }

    .report-table th {
      background-color: var(--bg-hover);
      color: var(--text-secondary);
      font-weight: 600;
      padding: 0.6rem 0.85rem;
      border-bottom: 1px solid var(--border-color);
    }

    .report-table td {
      padding: 0.65rem 0.85rem;
      border-bottom: 1px solid var(--border-color);
      color: var(--text-primary);
    }

    .project-name-cell {
      font-weight: 700;
      color: var(--text-primary);
    }

    .overdue-count {
      color: var(--danger-color);
      font-weight: 700;
    }

    .rate-cell {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .mini-progress-track {
      width: 60px;
      height: 4px;
      background-color: var(--bg-hover);
      border-radius: 99px;
      overflow: hidden;
    }

    .mini-progress-fill {
      height: 100%;
      background-color: var(--primary-color);
      border-radius: 99px;
    }

    .rate-val {
      font-weight: 700;
      font-size: 0.75rem;
    }

    /* States */
    .loading-state, .error-state {
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
export class ReportsComponent implements OnInit {
  private readonly taskService = inject(TaskService);
  private readonly projectService = inject(ProjectService);

  readonly projects = signal<ProjectResponse[]>([]);
  readonly tasks = signal<TaskResponse[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  // Computations
  readonly totalTasksCount = computed(() => this.tasks().length);
  
  readonly doneTasksCount = computed(() => 
    this.tasks().filter(t => t.status === TaskStatus.Done).length
  );

  readonly completionRate = computed(() => {
    const total = this.totalTasksCount();
    if (total === 0) return 0;
    return Math.round((this.doneTasksCount() * 100) / total);
  });

  readonly statusStats = computed(() => {
    const total = this.totalTasksCount() || 1;
    const todo = this.tasks().filter(t => t.status === TaskStatus.Todo).length;
    const progress = this.tasks().filter(t => t.status === TaskStatus.InProgress).length;
    const review = this.tasks().filter(t => t.status === TaskStatus.Review).length;
    const done = this.tasks().filter(t => t.status === TaskStatus.Done).length;
    const blocked = this.tasks().filter(t => t.status === TaskStatus.Blocked).length;

    return [
      { label: 'Todo', count: todo, percentage: Math.round((todo * 100) / total), class: 'bar-todo' },
      { label: 'In Progress', count: progress, percentage: Math.round((progress * 100) / total), class: 'bar-progress' },
      { label: 'Review', count: review, percentage: Math.round((review * 100) / total), class: 'bar-review' },
      { label: 'Done', count: done, percentage: Math.round((done * 100) / total), class: 'bar-done' },
      { label: 'Blocked', count: blocked, percentage: Math.round((blocked * 100) / total), class: 'bar-blocked' },
    ];
  });

  readonly priorityStats = computed(() => {
    const total = this.totalTasksCount() || 1;
    const low = this.tasks().filter(t => t.priority === TaskPriority.Low).length;
    const medium = this.tasks().filter(t => t.priority === TaskPriority.Medium).length;
    const high = this.tasks().filter(t => t.priority === TaskPriority.High).length;
    const critical = this.tasks().filter(t => t.priority === TaskPriority.Critical).length;

    return [
      { label: 'Low', count: low, percentage: Math.round((low * 100) / total), class: 'bar-low' },
      { label: 'Medium', count: medium, percentage: Math.round((medium * 100) / total), class: 'bar-medium' },
      { label: 'High', count: high, percentage: Math.round((high * 100) / total), class: 'bar-high' },
      { label: 'Critical', count: critical, percentage: Math.round((critical * 100) / total), class: 'bar-critical' },
    ];
  });

  readonly projectMetrics = computed(() => {
    return this.projects().map(p => {
      const projTasks = this.tasks().filter(t => t.projectId === p.projectId);
      const total = projTasks.length;
      const done = projTasks.filter(t => t.status === TaskStatus.Done).length;
      const overdue = projTasks.filter(t => t.isOverdue && t.status !== TaskStatus.Done).length;
      const rate = total === 0 ? 0 : Math.round((done * 100) / total);

      return {
        name: p.name,
        total,
        done,
        overdue,
        rate
      };
    });
  });

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading.set(true);
    this.error.set(null);

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
        this.error.set('Failed to load reports data.');
        this.loading.set(false);
      }
    });
  }
}
