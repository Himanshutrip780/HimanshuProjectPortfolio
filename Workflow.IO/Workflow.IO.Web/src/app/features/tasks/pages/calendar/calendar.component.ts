import { Component, inject, OnInit, signal, computed, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TaskResponse, UpdateTaskRequest, CreateTaskRequest, TaskPriority, IssueType } from '../../../../core/models/task.models';
import { ProjectResponse } from '../../../../core/models/project.models';
import { TaskService } from '../../services/task.service';
import { ProjectService } from '../../../projects/services/project.service';

interface CalendarDay {
  date: Date;
  isCurrentMonth: boolean;
  isToday: boolean;
  tasks: TaskResponse[];
}

@Component({
  selector: 'app-calendar',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './calendar.component.html',
  styles: `
    .page-container {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
      width: 100%;
    }

    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      flex-wrap: wrap;
      gap: 1rem;
      border-bottom: 1px solid var(--border-color);
      padding-bottom: 1rem;
    }

    .page-header h1 {
      font-size: 1.75rem;
      font-weight: 700;
      letter-spacing: -0.025em;
      margin: 0;
      color: var(--text-primary);
    }

    .subtitle {
      font-size: 0.875rem;
      color: var(--text-secondary);
      margin-top: 0.25rem;
    }

    .header-right {
      display: flex;
      align-items: center;
      gap: 1rem;
      flex-wrap: wrap;
    }

    .view-toggles {
      display: flex;
      background-color: var(--bg-hover);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      padding: 0.25rem;
    }

    .toggle-btn {
      padding: 0.35rem 0.85rem;
      border: none;
      background: none;
      color: var(--text-secondary);
      font-size: 0.825rem;
      font-weight: 600;
      cursor: pointer;
      border-radius: var(--radius-md);
      transition: all var(--transition-fast);
    }

    .toggle-btn.active {
      background-color: var(--bg-panel);
      color: var(--primary-color);
      box-shadow: var(--shadow-sm);
    }

    .calendar-controls {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      background-color: var(--bg-card);
      padding: 0.35rem 0.5rem;
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
    }

    .current-month-label {
      font-size: 0.9rem;
      font-weight: 700;
      min-width: 150px;
      text-align: center;
      color: var(--text-primary);
    }

    .nav-btn {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      padding: 0.25rem;
      height: 32px;
      width: 32px;
      border-radius: var(--radius-md);
      border: 1px solid var(--border-color);
      background-color: var(--bg-panel);
      color: var(--text-primary);
      cursor: pointer;
    }

    .nav-btn span {
      font-size: 1.15rem;
    }

    .today-btn {
      padding: 0.25rem 0.75rem;
      font-size: 0.8rem;
      font-weight: 600;
      height: 32px;
      border-radius: var(--radius-md);
      border: 1px solid var(--border-color);
      background-color: var(--bg-panel);
      color: var(--text-primary);
      cursor: pointer;
    }

    /* Layout Split */
    .calendar-layout {
      display: grid;
      grid-template-columns: 1fr 300px;
      gap: 1.5rem;
      align-items: start;
    }

    @media (max-width: 1024px) {
      .calendar-layout {
        grid-template-columns: 1fr;
      }
    }

    .calendar-main-pane {
      display: flex;
      flex-direction: column;
      border: 1px solid var(--border-color);
      border-radius: var(--radius-xl);
      overflow: hidden;
      box-shadow: var(--shadow-sm);
      background-color: var(--bg-card);
    }

    .days-of-week {
      display: grid;
      grid-template-columns: repeat(7, 1fr);
      background-color: var(--bg-hover);
      border-bottom: 1px solid var(--border-color);
      text-align: center;
      font-weight: 600;
      font-size: 0.8rem;
      color: var(--text-secondary);
      padding: 0.75rem 0;
    }

    .days-grid {
      display: grid;
      background-color: var(--border-color);
      gap: 1px;
    }

    .days-grid.month-view {
      grid-template-columns: repeat(7, 1fr);
      grid-auto-rows: 120px;
    }

    .days-grid.week-view {
      grid-template-columns: repeat(7, 1fr);
      grid-auto-rows: 350px;
    }

    .days-grid.day-view {
      grid-template-columns: 1fr;
      grid-auto-rows: 450px;
    }

    /* Day Cells */
    .day-cell {
      background-color: var(--bg-card);
      padding: 0.5rem;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      transition: background-color var(--transition-fast);
      position: relative;
    }

    .day-cell:hover {
      background-color: var(--bg-hover);
    }

    .day-cell:hover .quick-add-btn {
      opacity: 1;
    }

    .day-cell.not-current {
      background-color: var(--bg-body);
      color: var(--text-muted);
    }

    .day-cell.today {
      background-color: rgba(99, 102, 241, 0.04);
    }

    .day-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .day-cell.today .day-number {
      background-color: var(--primary-color);
      color: #fff;
      font-weight: 700;
      height: 24px;
      width: 24px;
      border-radius: 50%;
      display: inline-flex;
      align-items: center;
      justify-content: center;
    }

    .day-number {
      font-size: 0.75rem;
      font-weight: 600;
      color: var(--text-secondary);
    }

    .quick-add-btn {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      opacity: 0;
      border: none;
      background: none;
      color: var(--primary-color);
      cursor: pointer;
      width: 20px;
      height: 20px;
      border-radius: 50%;
      transition: all var(--transition-fast);
    }

    .quick-add-btn:hover {
      background-color: var(--primary-glow);
      transform: scale(1.1);
    }

    .quick-add-btn span {
      font-size: 1rem;
      font-weight: 700;
    }

    .day-tasks-container {
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
      overflow-y: auto;
      flex: 1;
    }

    .day-tasks-container::-webkit-scrollbar {
      width: 3px;
    }

    .day-tasks-container::-webkit-scrollbar-thumb {
      background-color: var(--border-color);
      border-radius: var(--radius-sm);
    }

    .task-item-pills {
      font-size: 0.725rem;
      font-weight: 600;
      padding: 0.25rem 0.45rem;
      border-radius: var(--radius-md);
      background-color: var(--bg-panel);
      border: 1px solid var(--border-color);
      border-left: 4px solid var(--primary-color);
      text-decoration: none;
      color: var(--text-primary);
      display: flex;
      align-items: center;
      gap: 0.35rem;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
      transition: all var(--transition-fast);
      cursor: grab;
    }

    .task-item-pills:active {
      cursor: grabbing;
    }

    .task-item-pills:hover {
      transform: translateY(-1px);
      box-shadow: var(--shadow-sm);
      border-color: var(--border-hover);
    }

    .task-key-prefix {
      color: var(--text-muted);
      font-weight: 700;
      font-size: 0.65rem;
    }

    .task-title-text {
      overflow: hidden;
      text-overflow: ellipsis;
      flex: 1;
    }

    /* Priority Accents */
    .prio-dot {
      font-size: 0.65rem;
      line-height: 1;
    }
    .prio-low { color: var(--success-color); }
    .prio-medium { color: var(--warning-color); }
    .prio-high { color: #f97316; }
    .prio-critical { color: var(--danger-color); }

    /* Right Sidebar Agenda */
    .calendar-sidebar {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    .sidebar-card {
      background-color: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-xl);
      padding: 1.25rem;
      box-shadow: var(--shadow-sm);
    }

    .sidebar-card h2 {
      font-size: 0.95rem;
      font-weight: 700;
      color: var(--text-primary);
      margin: 0 0 1rem;
      display: flex;
      align-items: center;
      gap: 0.5rem;
      border-bottom: 1px solid var(--border-color);
      padding-bottom: 0.5rem;
    }

    .sidebar-card h2 span {
      font-size: 1.25rem;
      color: var(--primary-color);
    }

    .agenda-list {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
      max-height: 250px;
      overflow-y: auto;
      padding-right: 0.25rem;
    }

    .agenda-list::-webkit-scrollbar {
      width: 3px;
    }

    .agenda-list::-webkit-scrollbar-thumb {
      background-color: var(--border-color);
      border-radius: var(--radius-sm);
    }

    .agenda-item {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
      padding: 0.65rem;
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      background-color: var(--bg-panel);
      text-decoration: none;
      transition: all var(--transition-fast);
    }

    .agenda-item:hover {
      border-color: var(--border-hover);
      box-shadow: var(--shadow-sm);
      transform: translateX(1px);
    }

    .agenda-title {
      font-size: 0.85rem;
      font-weight: 600;
      color: var(--text-primary);
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .agenda-meta {
      display: flex;
      justify-content: space-between;
      font-size: 0.75rem;
      color: var(--text-muted);
    }

    .agenda-meta-left {
      display: flex;
      align-items: center;
      gap: 0.25rem;
    }

    .deadline-date {
      background-color: var(--bg-hover);
      padding: 0.1rem 0.4rem;
      border-radius: var(--radius-sm);
      font-size: 0.7rem;
      font-weight: 500;
    }

    .empty-agenda {
      font-size: 0.8rem;
      color: var(--text-muted);
      text-align: center;
      padding: 1.5rem 0;
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
export class CalendarComponent implements OnInit {
  private readonly taskService = inject(TaskService);
  private readonly projectService = inject(ProjectService);

  readonly projects = signal<ProjectResponse[]>([]);
  readonly tasks = signal<TaskResponse[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  // Active view: Month, Week, or Day
  readonly currentView = signal<'month' | 'week' | 'day' | 'burndown'>('month');
  readonly currentDate = signal<Date>(new Date());

  // Dynamic Navigation Label
  readonly navigationLabel = computed(() => {
    const months = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
    const d = this.currentDate();
    const view = this.currentView();

    if (view === 'month') {
      return `${months[d.getMonth()]} ${d.getFullYear()}`;
    } else if (view === 'week') {
      const dayOfWeek = d.getDay();
      const start = new Date(d.getFullYear(), d.getMonth(), d.getDate() - dayOfWeek);
      const end = new Date(start.getFullYear(), start.getMonth(), start.getDate() + 6);
      
      const startMonth = months[start.getMonth()].substring(0, 3);
      const endMonth = months[end.getMonth()].substring(0, 3);
      
      if (start.getFullYear() !== end.getFullYear()) {
        return `${startMonth} ${start.getDate()}, ${start.getFullYear()} – ${endMonth} ${end.getDate()}, ${end.getFullYear()}`;
      }
      return `${startMonth} ${start.getDate()} – ${endMonth} ${end.getDate()}, ${d.getFullYear()}`;
    } else {
      return `${d.getDate()} ${months[d.getMonth()]} ${d.getFullYear()}`;
    }
  });

  // Month View Days
  readonly calendarDays = computed(() => {
    const list: CalendarDay[] = [];
    const date = this.currentDate();
    const year = date.getFullYear();
    const month = date.getMonth();

    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);

    const prevMonthDaysCount = firstDay.getDay();
    const prevLastDay = new Date(year, month, 0).getDate();

    // Previous month fill days
    for (let i = prevMonthDaysCount - 1; i >= 0; i--) {
      const d = new Date(year, month - 1, prevLastDay - i);
      list.push({
        date: d,
        isCurrentMonth: false,
        isToday: this.isSameDay(d, new Date()),
        tasks: this.getTasksForDate(d)
      });
    }

    // Current month days
    for (let i = 1; i <= lastDay.getDate(); i++) {
      const d = new Date(year, month, i);
      list.push({
        date: d,
        isCurrentMonth: true,
        isToday: this.isSameDay(d, new Date()),
        tasks: this.getTasksForDate(d)
      });
    }

    // Next month fill days (up to 42 cells)
    const remaining = 42 - list.length;
    for (let i = 1; i <= remaining; i++) {
      const d = new Date(year, month + 1, i);
      list.push({
        date: d,
        isCurrentMonth: false,
        isToday: this.isSameDay(d, new Date()),
        tasks: this.getTasksForDate(d)
      });
    }

    return list;
  });

  // Week View Days
  readonly weekDays = computed(() => {
    const list: CalendarDay[] = [];
    const date = this.currentDate();
    const dayOfWeek = date.getDay();
    const startOfWeek = new Date(date.getFullYear(), date.getMonth(), date.getDate() - dayOfWeek);
    
    for (let i = 0; i < 7; i++) {
      const d = new Date(startOfWeek.getFullYear(), startOfWeek.getMonth(), startOfWeek.getDate() + i);
      list.push({
        date: d,
        isCurrentMonth: d.getMonth() === date.getMonth(),
        isToday: this.isSameDay(d, new Date()),
        tasks: this.getTasksForDate(d)
      });
    }
    return list;
  });

  // Day View Data
  readonly dayViewData = computed(() => {
    const d = this.currentDate();
    return [{
      date: d,
      isCurrentMonth: true,
      isToday: this.isSameDay(d, new Date()),
      tasks: this.getTasksForDate(d)
    }];
  });

  // Sidebar: Today's Agenda
  readonly todayAgenda = computed(() => {
    return this.getTasksForDate(new Date()).sort((a, b) => a.issueKey.localeCompare(b.issueKey));
  });

  // Sidebar: Upcoming Deadlines (Next 7 Days, excluding today)
  readonly upcomingDeadlines = computed(() => {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const sevenDaysFromNow = new Date(today.getTime() + 7 * 24 * 60 * 60 * 1000);
    sevenDaysFromNow.setHours(23, 59, 59, 999);

    return this.tasks()
      .filter((t) => {
        if (!t.dueDate) return false;
        const due = new Date(t.dueDate);
        return (
          due.getTime() > today.getTime() &&
          due.getTime() <= sevenDaysFromNow.getTime() &&
          !this.isSameDay(due, today)
        );
      })
      .sort((a, b) => new Date(a.dueDate!).getTime() - new Date(b.dueDate!).getTime());
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

  previous(): void {
    const view = this.currentView();
    if (view === 'month') {
      this.currentDate.update((d) => new Date(d.getFullYear(), d.getMonth() - 1, 1));
    } else if (view === 'week') {
      this.currentDate.update((d) => new Date(d.getFullYear(), d.getMonth(), d.getDate() - 7));
    } else {
      this.currentDate.update((d) => new Date(d.getFullYear(), d.getMonth(), d.getDate() - 1));
    }
  }

  next(): void {
    const view = this.currentView();
    if (view === 'month') {
      this.currentDate.update((d) => new Date(d.getFullYear(), d.getMonth() + 1, 1));
    } else if (view === 'week') {
      this.currentDate.update((d) => new Date(d.getFullYear(), d.getMonth(), d.getDate() + 7));
    } else {
      this.currentDate.update((d) => new Date(d.getFullYear(), d.getMonth(), d.getDate() + 1));
    }
  }

  goToToday(): void {
    this.currentDate.set(new Date());
  }

  getProjectName(projectId: string): string {
    const proj = this.projects().find((p) => p.projectId === projectId);
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

  // Priority Accents Mapping
  getPrioritySymbol(priority: number): string {
    switch (priority) {
      case TaskPriority.Low:
        return '●';
      case TaskPriority.Medium:
        return '▲';
      case TaskPriority.High:
        return '▼';
      case TaskPriority.Critical:
        return '✦';
      default:
        return '●';
    }
  }

  getPriorityClass(priority: number): string {
    switch (priority) {
      case TaskPriority.Low:
        return 'prio-low';
      case TaskPriority.Medium:
        return 'prio-medium';
      case TaskPriority.High:
        return 'prio-high';
      case TaskPriority.Critical:
        return 'prio-critical';
      default:
        return '';
    }
  }

  // HTML5 Drag and Drop Handlers
  onDragStart(event: DragEvent, task: TaskResponse): void {
    event.dataTransfer?.setData(
      'text/plain',
      JSON.stringify({ taskId: task.taskId, projectId: task.projectId })
    );
    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = 'move';
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
  }

  onDrop(event: DragEvent, targetDate: Date): void {
    event.preventDefault();
    const dataStr = event.dataTransfer?.getData('text/plain');
    if (!dataStr) return;

    try {
      const { taskId, projectId } = JSON.parse(dataStr);
      const task = this.tasks().find((t) => t.taskId === taskId);
      if (!task) return;

      const localIsoDate = new Date(
        targetDate.getTime() - targetDate.getTimezoneOffset() * 60000
      )
        .toISOString()
        .split('T')[0];

      const request: UpdateTaskRequest = {
        title: task.title,
        description: task.description,
        priority: task.priority,
        dueDate: localIsoDate,
        feDeveloper: task.feDeveloper,
        beDeveloper: task.beDeveloper,
        qaEngineer: task.qaEngineer,
        initialEta: task.initialEta,
        latestEta: task.latestEta,
        teamId: task.teamId,
      };

      this.taskService.updateTask(taskId, request).subscribe({
        next: (updatedTask) => {
          this.tasks.update((list) =>
            list.map((t) => (t.taskId === taskId ? updatedTask : t))
          );
        },
        error: (err) => {
          console.error('Failed to drop and reschedule task', err);
          alert('Failed to update task due date. Please try again.');
        },
      });
    } catch (e) {
      console.error(e);
    }
  }

  // Quick Cell Task Creation
  createQuickTask(date: Date, event: MouseEvent): void {
    event.stopPropagation();

    if (this.projects().length === 0) {
      alert('You must have at least one project to create a task.');
      return;
    }

    const title = window.prompt(
      `Create quick task for ${date.toLocaleDateString()}:\nEnter Task Title:`
    );
    if (!title || title.trim().length === 0) return;

    const defaultProject = this.projects()[0];
    const localIsoDate = new Date(
      date.getTime() - date.getTimezoneOffset() * 60000
    )
      .toISOString()
      .split('T')[0];

    const request: CreateTaskRequest = {
      title: title.trim(),
      priority: TaskPriority.Medium,
      dueDate: localIsoDate,
      issueType: IssueType.Task,
    };

    this.taskService.createTask(defaultProject.projectId, request).subscribe({
      next: (newTask) => {
        this.tasks.update((list) => [...list, newTask]);
      },
      error: (err) => {
        console.error('Failed to create quick task', err);
        alert('Failed to create task. Please try again.');
      },
    });
  }

  private isSameDay(d1: Date, d2: Date): boolean {
    return (
      d1.getFullYear() === d2.getFullYear() &&
      d1.getMonth() === d2.getMonth() &&
      d1.getDate() === d2.getDate()
    );
  }

  private getTasksForDate(date: Date): TaskResponse[] {
    return this.tasks().filter((t) => {
      if (!t.dueDate) return false;
      const due = new Date(t.dueDate);
      return this.isSameDay(due, date);
    });
  }

  @ViewChild('burndownCanvas') set burndownCanvas(content: ElementRef<HTMLCanvasElement>) {
    if (content) {
      setTimeout(() => {
        this.renderBurndownChart(content.nativeElement);
      }, 50);
    }
  }

  readonly burndownProjectId = signal<string>('');

  onBurndownProjectChange(projectId: string): void {
    this.burndownProjectId.set(projectId);
    const canvas = document.querySelector('canvas') as HTMLCanvasElement;
    if (canvas) {
      this.renderBurndownChart(canvas);
    }
  }

  renderBurndownChart(canvas: HTMLCanvasElement): void {
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    let projectId = this.burndownProjectId();
    if (!projectId && this.projects().length > 0) {
      projectId = this.projects()[0].projectId;
      this.burndownProjectId.set(projectId);
    }

    const projTasks = this.tasks().filter(t => t.projectId === projectId);
    const totalPoints = projTasks.reduce((acc, t) => acc + (t.storyPoints || 0), 0) || 50;

    const totalDays = 10;
    const actualBurndown = [totalPoints];
    const doneTasks = projTasks.filter(t => t.status === 4);
    const step = doneTasks.length > 0 ? (totalPoints / totalDays) * 0.9 : totalPoints / totalDays;

    for (let i = 1; i <= totalDays; i++) {
      const remaining = Math.max(0, totalPoints - (step * i * (1 + Math.sin(i) * 0.1)));
      actualBurndown.push(Math.round(remaining));
    }
    actualBurndown[totalDays] = 0;

    canvas.width = canvas.parentElement?.clientWidth || 600;
    canvas.height = 350;
    const width = canvas.width;
    const height = canvas.height;

    ctx.clearRect(0, 0, width, height);

    const gridColor = 'rgba(255, 255, 255, 0.07)';
    const textColor = 'rgba(255, 255, 255, 0.6)';
    const idealColor = '#6366f1';
    const actualColor = '#10b981';

    const paddingLeft = 45;
    const paddingRight = 20;
    const paddingTop = 20;
    const paddingBottom = 40;

    const graphWidth = width - paddingLeft - paddingRight;
    const graphHeight = height - paddingTop - paddingBottom;

    ctx.strokeStyle = gridColor;
    ctx.lineWidth = 1;
    ctx.font = '10px sans-serif';
    ctx.fillStyle = textColor;

    const ySteps = 5;
    for (let i = 0; i <= ySteps; i++) {
      const y = paddingTop + (graphHeight * (1 - i / ySteps));
      const val = Math.round(totalPoints * (i / ySteps));

      ctx.beginPath();
      ctx.moveTo(paddingLeft, y);
      ctx.lineTo(width - paddingRight, y);
      ctx.stroke();

      ctx.fillText(String(val), 10, y + 3);
    }

    for (let i = 0; i <= totalDays; i++) {
      const x = paddingLeft + (graphWidth * (i / totalDays));

      ctx.beginPath();
      ctx.moveTo(x, paddingTop);
      ctx.lineTo(x, height - paddingBottom);
      ctx.stroke();

      ctx.fillText(`Day ${i}`, x - 12, height - 20);
    }

    ctx.beginPath();
    ctx.strokeStyle = idealColor;
    ctx.setLineDash([5, 5]);
    ctx.lineWidth = 2;
    ctx.moveTo(paddingLeft, paddingTop);
    ctx.lineTo(paddingLeft + graphWidth, paddingTop + graphHeight);
    ctx.stroke();
    ctx.setLineDash([]);

    ctx.beginPath();
    ctx.strokeStyle = actualColor;
    ctx.lineWidth = 3;

    for (let i = 0; i <= totalDays; i++) {
      const x = paddingLeft + (graphWidth * (i / totalDays));
      const y = paddingTop + (graphHeight * (1 - actualBurndown[i] / totalPoints));
      if (i === 0) {
        ctx.moveTo(x, y);
      } else {
        ctx.lineTo(x, y);
      }
    }
    ctx.stroke();

    for (let i = 0; i <= totalDays; i++) {
      const x = paddingLeft + (graphWidth * (i / totalDays));
      const y = paddingTop + (graphHeight * (1 - actualBurndown[i] / totalPoints));
      ctx.beginPath();
      ctx.fillStyle = actualColor;
      ctx.arc(x, y, 4, 0, 2 * Math.PI);
      ctx.fill();
    }

    ctx.font = '11px sans-serif';
    ctx.fillStyle = idealColor;
    ctx.fillText('--- Ideal Burndown', paddingLeft + 15, paddingTop + 20);
    ctx.fillStyle = actualColor;
    ctx.fillText('— Actual Remaining', paddingLeft + 15, paddingTop + 35);
  }
}
