import { Component, DestroyRef, effect, inject, OnInit, signal, HostListener, ViewChild, ElementRef, computed } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';
import { RealtimeService } from '../services/realtime.service';
import { UserService } from '../services/user.service';
import { ProjectService } from '../../features/projects/services/project.service';
import { TaskService } from '../../features/tasks/services/task.service';
import { ProjectResponse } from '../models/project.models';
import { TaskResponse, TaskStatus } from '../models/task.models';
import { UserDto } from '../models/user.models';
import { BackButtonService } from '../services/back-button.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, ReactiveFormsModule],
  templateUrl: './app-shell.component.html',
  styles: `
    .shell {
      display: flex;
      height: 100vh;
      width: 100vw;
      overflow: hidden;
      background-color: var(--bg-body);
    }

    .main-wrapper {
      display: flex;
      flex-direction: column;
      flex: 1;
      height: 100vh;
      overflow: hidden;
      min-width: 0;
    }

    /* Left Sidebar */
    .sidebar {
      width: 240px;
      background-color: var(--bg-sidebar);
      border-right: 1px solid var(--border-color);
      display: flex;
      flex-direction: column;
      height: 100vh;
      flex-shrink: 0;
      transition: width var(--transition-normal) var(--motion-easing);
      z-index: 50;
      overflow: hidden;
    }

    .sidebar-header {
      height: 64px;
      display: flex;
      align-items: center;
      padding: 0 1rem;
      border-bottom: 1px solid var(--border-color);
      position: relative;
    }

    .workspace-header {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      width: 100%;
      padding: 0.5rem;
      border-radius: var(--radius-md);
      opacity: 0.9;
    }

    .workspace-icon {
      width: 32px;
      height: 32px;
      border-radius: var(--radius-md);
      background: linear-gradient(135deg, var(--primary-color) 0%, #4f46e5 100%);
      display: flex;
      align-items: center;
      justify-content: center;
      box-shadow: 0 2px 5px rgba(99, 102, 241, 0.3);
    }

    .workspace-icon-text {
      color: white;
      font-weight: 700;
      font-size: 1rem;
    }

    .workspace-info {
      display: flex;
      flex-direction: column;
      flex: 1;
      overflow: hidden;
    }

    .workspace-name {
      font-weight: 600;
      font-size: 0.85rem;
      color: var(--text-primary);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .workspace-plan {
      font-size: 0.65rem;
      color: var(--text-secondary);
      font-weight: 500;
    }

    .workspace-chevron {
      color: var(--text-muted);
      font-size: 1.1rem;
    }

    /* Workspace Dropdown */
    .workspace-dropdown {
      position: absolute;
      top: calc(100% + 4px);
      left: 1rem;
      width: 260px;
      background-color: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      box-shadow: var(--shadow-lg);
      z-index: 1000;
      padding: 0.5rem;
      display: flex;
      flex-direction: column;
      animation: fadeInDown 0.15s ease-out;
    }

    .dropdown-section-title {
      font-size: 0.65rem;
      font-weight: 700;
      text-transform: uppercase;
      color: var(--text-muted);
      letter-spacing: 0.05em;
      padding: 0.5rem 0.5rem 0.25rem;
    }

    .workspace-option {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 0.5rem;
      background: transparent;
      border: none;
      border-radius: var(--radius-md);
      cursor: pointer;
      width: 100%;
      text-align: left;
      transition: background-color var(--transition-fast);
      color: var(--text-secondary);
    }

    .workspace-option:hover {
      background-color: var(--bg-hover);
      color: var(--text-primary);
    }

    .workspace-option.active {
      background-color: var(--primary-glow);
      color: var(--primary-color);
    }

    .workspace-icon-sm {
      width: 24px;
      height: 24px;
      border-radius: var(--radius-sm);
      background-color: var(--bg-panel);
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 0.75rem;
      font-weight: 700;
      color: var(--text-primary);
      border: 1px solid var(--border-color);
    }

    .workspace-option.active .workspace-icon-sm {
      background: linear-gradient(135deg, var(--primary-color) 0%, #4f46e5 100%);
      color: white;
      border: none;
    }

    .option-name {
      flex: 1;
      font-size: 0.8rem;
      font-weight: 500;
    }

    .check-icon {
      font-size: 1rem;
      color: var(--primary-color);
    }

    .dropdown-divider {
      height: 1px;
      background-color: var(--border-color);
      margin: 0.5rem 0;
    }

    .dropdown-action-btn {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.5rem;
      background: transparent;
      border: none;
      border-radius: var(--radius-md);
      cursor: pointer;
      width: 100%;
      text-align: left;
      color: var(--text-secondary);
      font-size: 0.8rem;
      font-weight: 500;
      transition: all var(--transition-fast);
    }

    .dropdown-action-btn:hover {
      background-color: var(--bg-hover);
      color: var(--text-primary);
    }

    .dropdown-action-btn span.material-symbols-outlined {
      font-size: 1.1rem;
    }

    .workspace-dropdown-backdrop {
      position: fixed;
      top: 0;
      left: 0;
      width: 100vw;
      height: 100vh;
      z-index: 999;
    }

    @keyframes fadeInDown {
      from { opacity: 0; transform: translateY(-10px); }
      to { opacity: 1; transform: translateY(0); }
    }

    /* Stunning Logo Styles */
    .sidebar-brand-header {
      padding: 1.25rem 1.25rem 0.75rem;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .brand-link {
      display: flex;
      align-items: center;
      gap: 1.2rem;
      text-decoration: none;
      position: relative;
    }

    .brand-logo-container {
      position: relative;
      width: 2.2rem;
      height: 2.2rem;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .brand-logo-img {
      width: 100%;
      height: 100%;
      object-fit: contain;
      filter: drop-shadow(0 4px 8px rgba(var(--primary-color-rgb), 0.4));
      z-index: 2;
      transition: transform 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
    }

    .brand-glow {
      position: absolute;
      top: 50%;
      left: 50%;
      width: 140%;
      height: 140%;
      transform: translate(-50%, -50%);
      background: radial-gradient(circle, rgba(var(--primary-color-rgb), 0.4) 0%, transparent 70%);
      z-index: 1;
      opacity: 0.8;
      animation: pulseGlow 3s infinite alternate ease-in-out;
    }

    .brand-link:hover .brand-logo-img {
      transform: scale(1.15) rotate(5deg);
    }

    .brand-text {
      font-weight: 800;
      font-size: 1.35rem;
      letter-spacing: -0.04em;
      color: var(--text-primary);
      background: linear-gradient(135deg, var(--text-primary) 0%, var(--primary-color) 100%);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      transition: all 0.3s ease;
    }

    .text-accent {
      color: var(--primary-color);
      background: none;
      -webkit-text-fill-color: var(--primary-color);
      font-weight: 900;
    }

    @keyframes pulseGlow {
      0% { opacity: 0.4; transform: translate(-50%, -50%) scale(0.9); }
      100% { opacity: 0.9; transform: translate(-50%, -50%) scale(1.2); }
    }

    .sidebar-scroll-area {
      flex: 1;
      overflow-y: auto;
      padding: 1.25rem 0.75rem;
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    /* Sidebar Scrollbar */
    .sidebar-scroll-area::-webkit-scrollbar {
      width: 4px;
    }
    .sidebar-scroll-area::-webkit-scrollbar-thumb {
      background: transparent;
    }
    .sidebar-scroll-area:hover::-webkit-scrollbar-thumb {
      background: var(--border-color);
    }

    .nav-section {
      display: flex;
      flex-direction: column;
      gap: 0.15rem;
    }

    .nav-section-title {
      font-size: 0.65rem;
      font-weight: 700;
      text-transform: uppercase;
      color: var(--text-muted);
      letter-spacing: 0.05em;
      padding: 0 0.5rem 0.35rem;
    }

    .nav-item {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 0.45rem 0.65rem;
      color: var(--text-secondary);
      text-decoration: none;
      border-radius: var(--radius-md);
      font-size: 0.85rem;
      font-weight: 500;
      transition: all var(--transition-fast);
    }

    .nav-item span.material-symbols-outlined {
      font-size: 1.2rem;
      color: var(--text-secondary);
      transition: color var(--transition-fast);
    }

    .nav-item:hover {
      background-color: var(--bg-hover);
      color: var(--text-primary);
    }

    .nav-item:hover span.material-symbols-outlined {
      color: var(--text-primary);
    }

    .nav-item.active {
      background-color: var(--primary-glow);
      color: var(--primary-color);
      font-weight: 600;
    }

    .nav-item.active span.material-symbols-outlined {
      color: var(--primary-color);
    }

    .notif-link {
      position: relative;
    }

    .badge-count {
      margin-left: auto;
      background-color: var(--primary-color);
      color: #fff;
      font-size: 0.7rem;
      font-weight: 700;
      padding: 0.1rem 0.45rem;
      border-radius: 9999px;
    }



    /* Profile Footer */
    .sidebar-user-footer {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0.75rem;
      border-top: 1px solid var(--border-color);
      background-color: var(--bg-sidebar);
    }

    .user-info-wrapper {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      min-width: 0;
      cursor: pointer;
      flex: 1;
      border-radius: var(--radius-md);
      padding: 0.25rem;
      transition: background-color var(--transition-fast);
    }

    .user-info-wrapper:hover {
      background-color: var(--bg-hover);
    }

    .user-avatar-img {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      object-fit: cover;
      border: 1px solid var(--border-color);
    }

    .user-avatar-initial {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      background: var(--primary-gradient);
      color: #fff;
      font-size: 0.85rem;
      font-weight: 700;
      display: flex;
      align-items: center;
      justify-content: center;
      text-transform: uppercase;
    }

    .user-details {
      display: flex;
      flex-direction: column;
      min-width: 0;
    }

    .user-name {
      font-size: 0.8rem;
      font-weight: 600;
      color: var(--text-primary);
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
      line-height: 1.2;
    }

    .user-email-sub {
      font-size: 0.7rem;
      color: var(--text-secondary);
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .btn-user-menu {
      background: transparent;
      border: none;
      color: var(--text-muted);
      cursor: pointer;
      padding: 0.25rem;
      display: flex;
      align-items: center;
      border-radius: var(--radius-md);
    }

    .btn-user-menu:hover {
      background-color: var(--bg-hover);
      color: var(--text-primary);
    }

    .user-footer-actions {
      display: flex;
      align-items: center;
      gap: 0.25rem;
    }

    .btn-collapse {
      background: transparent;
      border: none;
      color: var(--text-muted);
      cursor: pointer;
      display: flex;
      align-items: center;
      padding: 0.25rem;
      border-radius: var(--radius-md);
    }

    .btn-collapse:hover {
      background-color: var(--bg-hover);
      color: var(--text-primary);
    }

    /* Top Bar */
    .top-bar {
      height: 56px;
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0 1.5rem;
      background-color: var(--bg-header);
      border-bottom: 1px solid var(--border-color);
      z-index: 40;
      transition: background-color var(--transition-normal), border-color var(--transition-normal);
    }

    .top-bar-left {
      display: flex;
      align-items: center;
    }

    .global-search-container {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      background-color: var(--bg-hover);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      padding: 0.35rem 0.75rem;
      width: 320px;
      transition: all var(--transition-fast);
      user-select: none;
    }

    .global-search-container:hover {
      border-color: var(--text-muted);
      background-color: var(--bg-panel);
      box-shadow: var(--shadow-sm);
    }

    .search-icon {
      font-size: 1.1rem;
      color: var(--text-muted);
    }

    .global-search-placeholder {
      font-size: 0.8rem;
      color: var(--text-muted);
      flex: 1;
      text-align: left;
    }

    .search-shortcut {
      font-size: 0.65rem;
      font-weight: 600;
      color: var(--text-muted);
      background-color: var(--bg-card);
      border: 1px solid var(--border-color);
      padding: 0.1rem 0.35rem;
      border-radius: var(--radius-sm);
      margin-left: auto;
    }

    /* Search Overlay Styles */
    .search-overlay-backdrop {
      position: fixed;
      top: 0;
      left: 0;
      width: 100vw;
      height: 100vh;
      background-color: rgba(9, 9, 11, 0.7);
      backdrop-filter: blur(8px);
      z-index: 1000;
      display: flex;
      align-items: flex-start;
      justify-content: center;
      padding-top: 15vh;
      animation: fadeInOverlay 0.15s ease-out;
    }

    .search-overlay-card {
      width: 600px;
      max-width: 90%;
      background-color: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-xl);
      box-shadow: var(--shadow-lg), 0 20px 25px -5px rgba(0, 0, 0, 0.15);
      display: flex;
      flex-direction: column;
      max-height: 70vh;
      overflow: hidden;
      animation: slideDownOverlay 0.2s cubic-bezier(0.16, 1, 0.3, 1);
    }

    .search-overlay-header {
      display: flex;
      align-items: center;
      padding: 0.75rem 1.25rem;
      border-bottom: 1px solid var(--border-color);
      gap: 0.75rem;
    }

    .search-overlay-icon {
      color: var(--text-muted);
      font-size: 1.5rem;
    }

    .search-overlay-input {
      border: none !important;
      background: transparent !important;
      font-size: 1.05rem;
      color: var(--text-primary);
      width: 100%;
      outline: none;
      padding: 0.5rem 0;
      box-shadow: none !important;
    }

    .btn-close-overlay {
      background-color: var(--bg-hover) !important;
      border: 1px solid var(--border-color) !important;
      color: var(--text-muted) !important;
      font-size: 0.65rem;
      font-weight: 700;
      padding: 0.2rem 0.5rem;
      border-radius: var(--radius-sm);
    }

    .search-overlay-results {
      flex: 1;
      overflow-y: auto;
      padding: 1rem;
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }

    .search-tip, .search-empty {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 3rem 1.5rem;
      text-align: center;
      color: var(--text-secondary);
      gap: 0.75rem;
    }

    .search-tip span, .search-empty span {
      font-size: 2.5rem;
      color: var(--text-muted);
    }

    .search-tip p, .search-empty p {
      font-size: 0.85rem;
      margin: 0;
      max-width: 320px;
    }

    .result-category {
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
    }

    .category-title {
      font-size: 0.7rem;
      font-weight: 700;
      text-transform: uppercase;
      color: var(--text-muted);
      letter-spacing: 0.05em;
      padding-left: 0.5rem;
      margin-bottom: 0.25rem;
    }

    .result-item {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 0.6rem 0.75rem;
      border-radius: var(--radius-lg);
      text-decoration: none;
      transition: background-color var(--transition-fast);
    }

    .result-item:hover {
      background-color: var(--bg-hover);
    }

    .item-icon {
      font-size: 1.25rem;
    }

    .font-indigo { color: #6366f1; }
    .font-emerald { color: #10b981; }
    .font-amber { color: #f59e0b; }

    .item-info {
      display: flex;
      flex-direction: column;
      min-width: 0;
    }

    .item-title {
      font-size: 0.85rem;
      font-weight: 600;
      color: var(--text-primary);
    }

    .item-meta {
      font-size: 0.75rem;
      color: var(--text-secondary);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    @keyframes fadeInOverlay {
      from { opacity: 0; }
      to { opacity: 1; }
    }

    @keyframes slideDownOverlay {
      from { transform: scale(0.97) translateY(-8px); opacity: 0; }
      to { transform: scale(1) translateY(0); opacity: 1; }
    }

    .top-bar-right {
      display: flex;
      align-items: center;
      gap: 0.85rem;
    }

    .icon-btn {
      background: transparent;
      border: none;
      color: var(--text-secondary);
      cursor: pointer;
      padding: 0.45rem;
      border-radius: var(--radius-md);
      display: flex;
      align-items: center;
      justify-content: center;
      transition: background-color var(--transition-fast), color var(--transition-fast);
      position: relative;
    }

    .icon-btn:hover {
      background-color: var(--bg-hover);
      color: var(--text-primary);
    }

    .badge-dot {
      position: absolute;
      top: 6px;
      right: 6px;
      width: 6px;
      height: 6px;
      border-radius: 50%;
      background-color: var(--danger-color);
    }

    .top-user-menu {
      display: flex;
      align-items: center;
      gap: 0.45rem;
      padding: 0.25rem 0.65rem;
      border-radius: 9999px;
      border: 1px solid var(--border-color);
      background-color: var(--bg-hover);
      cursor: pointer;
      font-size: 0.8rem;
      font-weight: 500;
      color: var(--text-secondary);
      transition: all var(--transition-fast);
    }

    .top-user-menu:hover {
      border-color: var(--text-muted);
      background-color: var(--bg-panel);
      color: var(--text-primary);
    }

    .user-tag {
      width: 20px;
      height: 20px;
      border-radius: 50%;
      background: var(--primary-color);
      color: #fff;
      font-size: 0.65rem;
      font-weight: 700;
      display: flex;
      align-items: center;
      justify-content: center;
      text-transform: uppercase;
    }

    .user-email-text {
      max-width: 140px;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .expand-icon {
      font-size: 0.95rem;
      color: var(--text-muted);
    }

    .btn-signout {
      background-color: transparent;
      border: 1px solid var(--border-color);
      color: var(--text-secondary);
      font-size: 0.8rem;
      padding: 0.4rem 0.8rem;
      gap: 0.4rem;
      display: inline-flex;
      align-items: center;
      transition: all var(--transition-fast);
    }

    .btn-signout:hover {
      background-color: var(--danger-color);
      color: #fff;
      border-color: var(--danger-color);
    }

    /* Content */
    .content {
      flex: 1;
      overflow-y: auto;
      background-color: var(--bg-body);
    }

    .fade-in-container {
      padding: 1.75rem 2rem;
      animation: fadeIn 0.3s ease-out;
    }

    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(4px); }
      to { opacity: 1; transform: translateY(0); }
    }

    /* Sidebar Collapsed Styles */
    .sidebar-collapsed .sidebar {
      width: 56px;
    }

    .sidebar-collapsed .nav-label,
    .sidebar-collapsed .nav-section-title,
    .sidebar-collapsed .upgrade-card,
    .sidebar-collapsed .user-details,
    .sidebar-collapsed .btn-user-menu {
      display: none;
    }

    .sidebar-collapsed .sidebar-user-footer {
      flex-direction: column;
      gap: 0.5rem;
      align-items: center;
      padding: 0.75rem 0.5rem;
    }

    .sidebar-collapsed .sidebar-header {
      padding: 0;
      justify-content: center;
    }

    .sidebar-collapsed .brand-text {
      display: none;
    }

    .sidebar-collapsed .brand {
      justify-content: center;
      width: 100%;
    }

    .sidebar-collapsed .nav-item {
      justify-content: center;
      padding: 0.45rem;
    }
  `,
})
export class AppShellComponent implements OnInit {
  @ViewChild('searchInput') searchInputRef?: ElementRef<HTMLInputElement>;

  protected readonly auth = inject(AuthService);
  private readonly notifications = inject(NotificationService);
  private readonly realtime = inject(RealtimeService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly projectService = inject(ProjectService);
  private readonly taskService = inject(TaskService);
  private readonly userService = inject(UserService);
  private readonly backButtonService = inject(BackButtonService);

  readonly unreadCount = signal(0);
  readonly collapsed = signal(false);
  readonly isDarkMode = signal(false);

  readonly showSearchOverlay = signal(false);
  readonly searchOverlayControl = new FormControl('', { nonNullable: true });

  readonly allProjects = signal<ProjectResponse[]>([]);
  readonly allTasks = signal<TaskResponse[]>([]);
  readonly allUsers = signal<UserDto[]>([]);

  readonly matchedProjects = computed(() => {
    const query = this.searchOverlayControl.value.trim().toLowerCase();
    if (!query) return [];
    return this.allProjects().filter(p => 
      p.name.toLowerCase().includes(query) || 
      p.key.toLowerCase().includes(query)
    ).slice(0, 5);
  });

  readonly matchedTasks = computed(() => {
    const query = this.searchOverlayControl.value.trim().toLowerCase();
    if (!query) return [];
    return this.allTasks().filter(t => 
      t.title.toLowerCase().includes(query) || 
      t.issueKey.toLowerCase().includes(query)
    ).slice(0, 5);
  });

  readonly matchedUsers = computed(() => {
    const query = this.searchOverlayControl.value.trim().toLowerCase();
    if (!query) return [];
    return this.allUsers().filter(u => 
      `${u.firstName} ${u.lastName}`.toLowerCase().includes(query) || 
      u.email.toLowerCase().includes(query)
    ).slice(0, 5);
  });

  readonly hasNoResults = computed(() => {
    const query = this.searchOverlayControl.value.trim();
    if (!query) return false;
    return this.matchedProjects().length === 0 && 
           this.matchedTasks().length === 0 && 
           this.matchedUsers().length === 0;
  });

  private readonly notificationRefreshEffect = effect(() => {
    const event = this.realtime.latestEvent();
    if (event?.eventType === 'NotificationReceived') {
      this.refreshUnreadCount();
    }
  });

  private searchOverlayCleanup: (() => void) | null = null;
  private readonly searchOverlayEffect = effect(() => {
    const isOpen = this.showSearchOverlay();
    if (isOpen) {
      this.searchOverlayCleanup = this.backButtonService.registerHandler(
        'SearchOverlay',
        10,
        () => {
          this.closeSearchOverlay();
          return true; // Consumed
        }
      );
    } else {
      if (this.searchOverlayCleanup) {
        this.searchOverlayCleanup();
        this.searchOverlayCleanup = null;
      }
    }
  });

  ngOnInit(): void {
    void this.realtime.connect();
    this.refreshUnreadCount();
    
    // Load theme setting
    const savedTheme = localStorage.getItem('theme') || 'light';
    this.isDarkMode.set(savedTheme === 'dark');
    this.applyTheme(savedTheme);
  }

  openSearchOverlay(): void {
    this.showSearchOverlay.set(true);
    this.searchOverlayControl.setValue('');
    this.loadSearchableData();
    setTimeout(() => {
      this.searchInputRef?.nativeElement.focus();
    }, 100);
  }

  closeSearchOverlay(): void {
    this.showSearchOverlay.set(false);
  }

  @HostListener('window:keydown', ['$event'])
  handleKeyDown(event: KeyboardEvent) {
    if ((event.ctrlKey || event.metaKey) && event.key === 'k') {
      event.preventDefault();
      if (this.showSearchOverlay()) {
        this.closeSearchOverlay();
      } else {
        this.openSearchOverlay();
      }
    } else if (event.key === 'Escape' && this.showSearchOverlay()) {
      this.closeSearchOverlay();
    }
  }

  private loadSearchableData(): void {
    this.projectService.getMyProjects().subscribe(projects => this.allProjects.set(projects));
    this.taskService.getAllTasks().subscribe(tasks => this.allTasks.set(tasks));
    this.userService.getAllUsers().subscribe(users => this.allUsers.set(users));
  }

  getTaskStatusLabel(status: TaskStatus): string {
    return TaskStatus[status] || String(status);
  }

  toggleTheme(): void {
    const targetTheme = this.isDarkMode() ? 'light' : 'dark';
    this.isDarkMode.set(targetTheme === 'dark');
    localStorage.setItem('theme', targetTheme);
    this.applyTheme(targetTheme);
  }

  userInitials(user: any): string {
    if (user?.firstName && user?.lastName) {
      return `${user.firstName.charAt(0)}${user.lastName.charAt(0)}`.toUpperCase();
    } else if (user?.firstName) {
      return user.firstName.charAt(0).toUpperCase();
    }
    return user?.email ? user.email.substring(0, 2).toUpperCase() : 'US';
  }

  signOut(): void {
    this.auth.logout();
  }

  private applyTheme(theme: string): void {
    if (theme === 'dark') {
      document.documentElement.classList.add('dark-theme');
      document.documentElement.setAttribute('data-theme', 'dark');
    } else {
      document.documentElement.classList.remove('dark-theme');
      document.documentElement.setAttribute('data-theme', 'light');
    }
  }

  private refreshUnreadCount(): void {
    this.notifications
      .getUnreadCount()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (count) => this.unreadCount.set(count),
        error: () => this.unreadCount.set(0),
      });
  }
}
