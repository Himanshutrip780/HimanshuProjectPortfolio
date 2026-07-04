import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { Notification } from '../../../../core/models/notification.models';
import { NotificationService } from '../../../../core/services/notification.service';
import { getApiErrorMessage } from '../../../../core/utils/api-error.util';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-notification-list',
  standalone: true,
  imports: [CommonModule, PaginationComponent],
  templateUrl: './notification-list.component.html',
  styles: `
    .page {
      width: 100%;
      margin: 0 auto;
      padding: 1.5rem;
    }

    header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 2rem;
      border-bottom: 1px solid var(--border-color);
      padding-bottom: 1rem;
      flex-wrap: wrap;
      gap: 0.75rem;
    }

    h1 {
      font-size: 1.875rem;
      font-weight: 700;
      color: var(--text-primary);
      margin: 0;
    }

    .toolbar {
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .btn-mark-all {
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.5rem 1rem;
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      background-color: var(--bg-panel);
      color: var(--text-primary);
      font-size: 0.875rem;
      font-weight: 600;
      cursor: pointer;
      transition: all var(--transition-fast);
    }

    .btn-mark-all:hover:not(:disabled) {
      border-color: var(--primary-color);
      color: var(--primary-color);
      background-color: var(--bg-hover);
    }

    .btn-mark-all:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .filter-bar {
      display: flex;
      gap: 0.5rem;
      margin-bottom: 1.5rem;
      overflow-x: auto;
      padding-bottom: 0.5rem;
      border-bottom: 1px solid var(--border-color);
    }

    .filter-btn {
      padding: 0.5rem 1rem;
      border: 1px solid transparent;
      border-radius: var(--radius-lg);
      background-color: transparent;
      color: var(--text-secondary);
      font-size: 0.875rem;
      font-weight: 500;
      cursor: pointer;
      transition: all var(--transition-fast);
      white-space: nowrap;
    }

    .filter-btn:hover {
      background-color: var(--bg-hover);
      color: var(--text-primary);
    }

    .filter-btn.active {
      background-color: var(--primary-glow);
      color: var(--primary-color);
      border-color: rgba(99, 102, 241, 0.2);
      font-weight: 600;
    }

    .group-section {
      margin-bottom: 2rem;
    }

    .group-header {
      font-size: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      color: var(--text-muted);
      margin-bottom: 0.75rem;
      font-weight: 700;
    }

    .notification-list {
      list-style: none;
      padding: 0;
      margin: 0;
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }

    .notification-card {
      display: flex;
      justify-content: space-between;
      align-items: center;
      background-color: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      padding: 1rem;
      transition: all var(--transition-fast);
      box-shadow: var(--shadow-sm);
    }

    .notification-card:hover {
      box-shadow: var(--shadow-md);
      transform: translateY(-1px);
      border-color: var(--border-hover);
    }

    .notification-card.unread {
      border-left: 4px solid var(--primary-color);
      background-color: rgba(99, 102, 241, 0.02);
    }

    .notification-card:not(.unread) {
      opacity: 0.75;
    }

    .card-left {
      display: flex;
      align-items: flex-start;
      gap: 1rem;
      flex: 1;
    }

    .icon-wrapper {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 2.5rem;
      height: 2.5rem;
      border-radius: 50%;
      flex-shrink: 0;
    }

    .icon-mention {
      background-color: rgba(139, 92, 246, 0.1);
      color: #8b5cf6;
    }

    .icon-task {
      background-color: rgba(16, 185, 129, 0.1);
      color: #10b981;
    }

    .icon-project {
      background-color: rgba(59, 130, 246, 0.1);
      color: #3b82f6;
    }

    .icon-system {
      background-color: rgba(245, 158, 11, 0.1);
      color: #f59e0b;
    }

    .icon-wrapper .material-symbols-outlined {
      font-size: 1.25rem;
    }

    .card-content {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
    }

    .card-meta {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .entity-badge {
      font-size: 0.75rem;
      font-weight: 600;
      color: var(--text-muted);
      text-transform: uppercase;
    }

    .time-stamp {
      font-size: 0.75rem;
      color: var(--text-muted);
    }

    .notification-message {
      font-size: 0.925rem;
      color: var(--text-primary);
      line-height: 1.4;
      margin: 0;
    }

    .card-actions {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      margin-left: 1rem;
    }

    .btn-action-read {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 2rem;
      height: 2rem;
      border: 1px solid var(--border-color);
      border-radius: 50%;
      background-color: var(--bg-panel);
      color: var(--text-secondary);
      cursor: pointer;
      transition: all var(--transition-fast);
    }

    .btn-action-read:hover {
      border-color: var(--success-color);
      color: var(--success-color);
      background-color: rgba(16, 185, 129, 0.05);
    }

    .btn-action-read .material-symbols-outlined {
      font-size: 1rem;
    }

    .error {
      color: var(--danger-color);
      font-weight: 500;
      padding: 1rem;
      background-color: rgba(239, 68, 68, 0.05);
      border-radius: var(--radius-md);
      border: 1px solid rgba(239, 68, 68, 0.1);
    }

    .empty-state {
      text-align: center;
      padding: 3rem 1rem;
      color: var(--text-muted);
    }

    .empty-state .material-symbols-outlined {
      font-size: 3rem;
      margin-bottom: 1rem;
    }
  `,
})
export class NotificationListComponent implements OnInit {
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  readonly allItems = signal<Notification[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  // Pagination states
  readonly currentPage = signal(1);
  readonly pageSize = signal(10);

  // Category filter state
  readonly activeCategory = signal<string>('all');

  // Filtered list based on category selection
  readonly filteredItems = computed(() => {
    const items = this.allItems();
    const category = this.activeCategory();

    let result = items;
    if (category !== 'all') {
      result = items.filter((n) => {
        const entity = n.entityType?.toLowerCase() || '';
        const event = n.eventType?.toLowerCase() || '';
        if (category === 'unread') {
          return !n.isRead;
        }
        if (category === 'mentions') {
          return event.includes('mention') || n.message.includes('@');
        }
        if (category === 'tasks') {
          return entity === 'task' || event.includes('task');
        }
        if (category === 'projects') {
          return entity === 'project' || event.includes('project');
        }
        if (category === 'system') {
          return (
            entity === 'system' ||
            event.includes('system') ||
            (!entity && !event.includes('task') && !event.includes('project'))
          );
        }
        return true;
      });
    }

    return result.sort(
      (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
    );
  });

  // Paged slice of the filtered items
  readonly pagedItems = computed(() => {
    const items = this.filteredItems();
    const startIndex = (this.currentPage() - 1) * this.pageSize();
    return items.slice(startIndex, startIndex + this.pageSize());
  });

  // Grouped by Today, Yesterday, and Older
  readonly groupedItems = computed(() => {
    const items = this.pagedItems();
    const groups: { title: string; items: Notification[] }[] = [];

    const today: Notification[] = [];
    const yesterday: Notification[] = [];
    const older: Notification[] = [];

    const now = new Date();
    const todayStr = now.toDateString();

    const yesterdayDate = new Date();
    yesterdayDate.setDate(yesterdayDate.getDate() - 1);
    const yesterdayStr = yesterdayDate.toDateString();

    for (const n of items) {
      const date = new Date(n.createdAt);
      const dateStr = date.toDateString();
      if (dateStr === todayStr) {
        today.push(n);
      } else if (dateStr === yesterdayStr) {
        yesterday.push(n);
      } else {
        older.push(n);
      }
    }

    if (today.length > 0) groups.push({ title: 'Today', items: today });
    if (yesterday.length > 0) groups.push({ title: 'Yesterday', items: yesterday });
    if (older.length > 0) groups.push({ title: 'Older', items: older });

    return groups;
  });

  ngOnInit(): void {
    this.load();
  }

  setCategory(category: string): void {
    this.activeCategory.set(category);
    this.currentPage.set(1); // Reset to first page
  }

  onPageChange(page: number): void {
    this.currentPage.set(page);
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.currentPage.set(1);
  }

  markRead(notificationId: string): void {
    this.notificationService
      .markAsRead(notificationId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          // Optimistically update read state locally to avoid full list reload lag
          this.allItems.update((items) =>
            items.map((n) =>
              n.notificationId === notificationId ? { ...n, isRead: true } : n
            )
          );
        },
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  markAllRead(): void {
    this.notificationService
      .markAllAsRead()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.allItems.update((items) =>
            items.map((n) => ({ ...n, isRead: true }))
          );
        },
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  getIcon(n: Notification): string {
    const entity = n.entityType?.toLowerCase() || '';
    const event = n.eventType?.toLowerCase() || '';
    if (event.includes('mention') || n.message.includes('@')) {
      return 'alternate_email';
    }
    if (entity === 'task' || event.includes('task')) {
      return 'task_alt';
    }
    if (entity === 'project' || event.includes('project')) {
      return 'folder';
    }
    return 'notifications_active';
  }

  getIconClass(n: Notification): string {
    const entity = n.entityType?.toLowerCase() || '';
    const event = n.eventType?.toLowerCase() || '';
    if (event.includes('mention') || n.message.includes('@')) {
      return 'icon-mention';
    }
    if (entity === 'task' || event.includes('task')) {
      return 'icon-task';
    }
    if (entity === 'project' || event.includes('project')) {
      return 'icon-project';
    }
    return 'icon-system';
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);

    // Fetch all notifications; client-side computed filtering handles categorization dynamically
    this.notificationService
      .getMyNotifications(false)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (items) => {
          this.allItems.set(items);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(getApiErrorMessage(err));
          this.loading.set(false);
        },
      });
  }
}
