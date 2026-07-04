import { Component, inject, OnInit, signal, computed, ViewChild, ElementRef } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TaskResponse } from '../../../../core/models/task.models';
import { ProjectResponse } from '../../../../core/models/project.models';
import { TaskService } from '../../services/task.service';
import { ProjectService } from '../../../projects/services/project.service';

interface TimelineProject {
  project: ProjectResponse;
  tasks: TaskResponse[];
}

@Component({
  selector: 'app-timeline',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './timeline.component.html',
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
      flex-wrap: wrap;
      gap: 1rem;
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

    .timeline-legend {
      display: flex;
      gap: 1rem;
      font-size: 0.75rem;
      font-weight: 600;
      color: var(--text-secondary);
    }

    .legend-item {
      display: flex;
      align-items: center;
      gap: 0.35rem;
    }

    .legend-color {
      width: 10px;
      height: 10px;
      border-radius: 50%;
      display: inline-block;
    }

    .bg-todo { background-color: #a1a1aa; }
    .bg-progress { background-color: var(--info-color); }
    .bg-done { background-color: var(--success-color); }

    .timeline-container {
      display: flex;
      padding: 0;
      border: 1px solid var(--border-color);
      border-radius: var(--radius-xl);
      overflow: hidden;
      box-shadow: var(--shadow-sm);
      height: 600px;
    }

    /* Left Sidebar */
    .timeline-sidebar {
      width: 250px;
      border-right: 1px solid var(--border-color);
      display: flex;
      flex-direction: column;
      flex-shrink: 0;
      background-color: var(--bg-panel);
    }

    .sidebar-header {
      height: 48px;
      display: flex;
      align-items: center;
      padding: 0 1rem;
      font-weight: 700;
      font-size: 0.8rem;
      color: var(--text-secondary);
      background-color: var(--bg-hover);
      border-bottom: 1px solid var(--border-color);
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }

    .sidebar-rows {
      flex: 1;
      overflow-y: auto;
    }

    .sidebar-rows::-webkit-scrollbar {
      width: 0px; /* Hide scrollbar and sync with grid */
    }

    .sidebar-project-row {
      height: 40px;
      display: flex;
      align-items: center;
      padding: 0 1rem;
      font-weight: 700;
      font-size: 0.85rem;
      color: var(--text-primary);
      background-color: var(--bg-hover);
      border-bottom: 1px solid var(--border-color);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .project-indicator {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      margin-right: 0.5rem;
      display: inline-block;
    }

    .sidebar-task-row {
      height: 36px;
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0 1.25rem;
      border-bottom: 1px solid var(--border-color);
      font-size: 0.75rem;
    }

    .task-key {
      font-weight: 700;
      color: var(--text-secondary);
      background-color: var(--bg-hover);
      padding: 0.1rem 0.3rem;
      border-radius: var(--radius-sm);
      border: 1px solid var(--border-color);
    }

    .task-title {
      font-weight: 500;
      color: var(--text-primary);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .sidebar-empty-row {
      height: 36px;
      display: flex;
      align-items: center;
      padding: 0 1.25rem;
      color: var(--text-muted);
      font-style: italic;
      font-size: 0.75rem;
      border-bottom: 1px solid var(--border-color);
    }

    /* Right Timeline Grid */
    .timeline-grid-wrapper {
      flex: 1;
      display: flex;
      flex-direction: column;
      overflow: auto;
      background-color: var(--bg-card);
    }

    .timeline-grid-header {
      height: 48px;
      display: flex;
      background-color: var(--bg-hover);
      border-bottom: 1px solid var(--border-color);
      position: sticky;
      top: 0;
      z-index: 10;
    }

    .day-header {
      width: 50px;
      min-width: 50px;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      border-right: 1px solid var(--border-color);
      font-size: 0.65rem;
      color: var(--text-secondary);
    }

    .day-header.today {
      background-color: var(--primary-glow);
      color: var(--primary-color);
      font-weight: 700;
    }

    .day-num {
      font-size: 0.8rem;
      font-weight: 700;
    }

    .timeline-grid-body {
      flex: 1;
      position: relative;
    }

    .grid-project-row {
      height: 40px;
      background-color: var(--bg-hover);
      border-bottom: 1px solid var(--border-color);
    }

    .grid-task-row {
      height: 36px;
      border-bottom: 1px solid var(--border-color);
      position: relative;
    }

    .grid-empty-row {
      height: 36px;
      border-bottom: 1px solid var(--border-color);
    }

    .task-bar {
      position: absolute;
      top: 6px;
      height: 24px;
      border-radius: var(--radius-md);
      display: flex;
      align-items: center;
      padding: 0 0.5rem;
      font-size: 0.7rem;
      font-weight: 700;
      color: #fff;
      text-decoration: none;
      box-shadow: var(--shadow-sm);
      overflow: hidden;
      white-space: nowrap;
      text-overflow: ellipsis;
      transition: transform var(--transition-fast), box-shadow var(--transition-fast);
    }

    .task-bar:hover {
      transform: translateY(-1px);
      box-shadow: var(--shadow-md);
    }

    .bar-todo {
      background-color: #a1a1aa;
      border: 1px solid #71717a;
      color: #18181b;
    }

    .bar-progress {
      background: linear-gradient(95deg, var(--info-color) 0%, #60a5fa 100%);
      border: 1px solid #2563eb;
    }

    .bar-review {
      background: linear-gradient(95deg, var(--warning-color) 0%, #fbbf24 100%);
      border: 1px solid #d97706;
    }

    .bar-done {
      background: linear-gradient(95deg, var(--success-color) 0%, #34d399 100%);
      border: 1px solid #059669;
    }

    .bar-blocked {
      background: linear-gradient(95deg, var(--danger-color) 0%, #f87171 100%);
      border: 1px solid #dc2626;
    }

    .bar-text {
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
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
export class TimelineComponent implements OnInit {
  @ViewChild('sidebarRows') sidebarRows!: ElementRef<HTMLDivElement>;
  @ViewChild('gridWrapper') gridWrapper!: ElementRef<HTMLDivElement>;

  private readonly taskService = inject(TaskService);
  private readonly projectService = inject(ProjectService);

  private isScrollingSidebar = false;
  private isScrollingGrid = false;

  onSidebarScroll(): void {
    if (this.isScrollingGrid) return;
    this.isScrollingSidebar = true;
    const sidebar = this.sidebarRows.nativeElement;
    const grid = this.gridWrapper.nativeElement;
    grid.scrollTop = sidebar.scrollTop;
    setTimeout(() => (this.isScrollingSidebar = false), 50);
  }

  onGridScroll(): void {
    if (this.isScrollingSidebar) return;
    this.isScrollingGrid = true;
    const sidebar = this.sidebarRows.nativeElement;
    const grid = this.gridWrapper.nativeElement;
    sidebar.scrollTop = grid.scrollTop;
    setTimeout(() => (this.isScrollingGrid = false), 50);
  }

  readonly projects = signal<ProjectResponse[]>([]);
  readonly tasks = signal<TaskResponse[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  // Timeline start is 7 days ago, end is 23 days from now (30 days total)
  readonly timelineDays = computed(() => {
    const list: Date[] = [];
    const start = new Date();
    start.setDate(start.getDate() - 7);
    for (let i = 0; i < 30; i++) {
      const d = new Date(start);
      d.setDate(start.getDate() + i);
      list.push(d);
    }
    return list;
  });

  readonly timelineProjects = computed<TimelineProject[]>(() => {
    const list: TimelineProject[] = [];
    for (const p of this.projects()) {
      const projTasks = this.tasks().filter(t => t.projectId === p.projectId && t.dueDate);
      
      const sortedTasks: TaskResponse[] = [];
      const rootTasks = projTasks.filter(t => !t.parentTaskId);
      const subTasks = projTasks.filter(t => t.parentTaskId);
      
      for (const root of rootTasks) {
        sortedTasks.push(root);
        const children = subTasks.filter(t => t.parentTaskId === root.taskId);
        for (const child of children) {
          sortedTasks.push(child);
        }
      }
      
      const orphans = subTasks.filter(t => !rootTasks.some(r => r.taskId === t.parentTaskId));
      for (const orphan of orphans) {
        sortedTasks.push(orphan);
      }
      
      list.push({
        project: p,
        tasks: sortedTasks
      });
    }
    return list;
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
        this.error.set('Failed to load tasks. Make sure your gateway routes are correctly configured.');
        this.loading.set(false);
      }
    });
  }

  getProjectColor(name: string): string {
    const colors = ['#6366f1', '#3b82f6', '#ec4899', '#10b981', '#f59e0b'];
    let hash = 0;
    for (let i = 0; i < name.length; i++) {
      hash = name.charCodeAt(i) + ((hash << 5) - hash);
    }
    const index = Math.abs(hash) % colors.length;
    return colors[index];
  }

  getDayLabel(d: Date): string {
    const days = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
    return days[d.getDay()];
  }

  isToday(d: Date): boolean {
    const today = new Date();
    return d.getFullYear() === today.getFullYear() &&
           d.getMonth() === today.getMonth() &&
           d.getDate() === today.getDate();
  }

  formatDueDate(dateStr: string | null): string {
    if (!dateStr) return '';
    const date = new Date(dateStr);
    return `${date.getMonth() + 1}/${date.getDate()}`;
  }

  getTaskBarClass(status: number): string {
    switch (status) {
      case 1: return 'bar-todo';
      case 2: return 'bar-progress';
      case 3: return 'bar-review';
      case 4: return 'bar-done';
      case 5: return 'bar-blocked';
      default: return 'bar-todo';
    }
  }

  getTaskSpan(task: TaskResponse): { left: number; width: number } | null {
    if (!task.dueDate) return null;
    const due = new Date(task.dueDate);
    const start = new Date(task.createdAt);

    const timelineStart = this.timelineDays()[0];
    const timelineEnd = this.timelineDays()[this.timelineDays().length - 1];

    // Clamp values within timeline limits
    const barStart = start < timelineStart ? timelineStart : start;
    const barEnd = due > timelineEnd ? timelineEnd : due;

    if (barEnd < barStart) {
      return { left: this.getDayOffset(due), width: 50 }; // default 1 day width
    }

    const offsetLeft = this.getDayOffset(barStart);
    const diffMs = barEnd.getTime() - barStart.getTime();
    const diffDays = Math.max(1, Math.ceil(diffMs / (1000 * 60 * 60 * 24)));
    
    return {
      left: offsetLeft,
      width: diffDays * 50 // 50px per day
    };
  }

  private getDayOffset(date: Date): number {
    const timelineStart = this.timelineDays()[0];
    const cleanStart = new Date(timelineStart.getFullYear(), timelineStart.getMonth(), timelineStart.getDate());
    const cleanDate = new Date(date.getFullYear(), date.getMonth(), date.getDate());

    const diffMs = cleanDate.getTime() - cleanStart.getTime();
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));
    return diffDays * 50;
  }
}
