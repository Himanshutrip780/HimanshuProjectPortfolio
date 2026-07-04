import { Component, inject, OnInit, signal, computed, effect } from '@angular/core';
import { RouterLink } from '@angular/router';
import { UserDto } from '../../../../core/models/user.models';
import { UserService } from '../../../../core/services/user.service';
import { ProjectService } from '../../../projects/services/project.service';
import { ProjectResponse } from '../../../../core/models/project.models';
import { BackButtonService } from '../../../../core/services/back-button.service';

@Component({
  selector: 'app-team',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './team.component.html',
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

    /* Toolbar Styles */
    .toolbar-panel {
      display: flex;
      justify-content: space-between;
      align-items: center;
      flex-wrap: wrap;
      gap: 1rem;
      background-color: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-xl);
      padding: 0.75rem 1.25rem;
    }

    .toolbar-left {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      flex: 1;
      min-width: 280px;
    }

    .search-input-wrapper {
      position: relative;
      flex: 1;
      max-width: 400px;
    }

    .search-input-wrapper input {
      width: 100%;
      padding: 0.45rem 0.75rem 0.45rem 2.2rem;
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      font-size: 0.85rem;
      background-color: var(--bg-hover);
      color: var(--text-primary);
      outline: none;
      transition: all var(--transition-fast);
    }

    .search-input-wrapper input:focus {
      border-color: var(--primary-color);
      background-color: var(--bg-panel);
    }

    .search-input-wrapper .search-icon {
      position: absolute;
      left: 0.65rem;
      top: 50%;
      transform: translateY(-50%);
      font-size: 1.15rem;
      color: var(--text-muted);
      pointer-events: none;
    }

    .filter-dropdown-wrapper {
      position: relative;
      display: flex;
      align-items: center;
    }

    .filter-dropdown-wrapper select {
      padding: 0.45rem 1.75rem 0.45rem 2rem;
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      font-size: 0.85rem;
      background-color: var(--bg-hover);
      color: var(--text-primary);
      outline: none;
      cursor: pointer;
      appearance: none;
      transition: all var(--transition-fast);
    }

    .filter-dropdown-wrapper select:focus {
      border-color: var(--primary-color);
      background-color: var(--bg-panel);
    }

    .filter-dropdown-wrapper .filter-icon {
      position: absolute;
      left: 0.65rem;
      top: 50%;
      transform: translateY(-50%);
      font-size: 1.15rem;
      color: var(--text-muted);
      pointer-events: none;
    }

    .filter-dropdown-wrapper::after {
      content: 'expand_more';
      font-family: 'Material Symbols Outlined';
      position: absolute;
      right: 0.65rem;
      top: 50%;
      transform: translateY(-50%);
      font-size: 1.15rem;
      color: var(--text-muted);
      pointer-events: none;
    }

    .toolbar-right {
      display: flex;
      align-items: center;
    }

    .view-toggle-group {
      display: flex;
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      overflow: hidden;
    }

    .btn-toggle {
      background: var(--bg-card);
      border: none;
      color: var(--text-secondary);
      padding: 0.45rem 0.65rem;
      cursor: pointer;
      display: flex;
      align-items: center;
      transition: all var(--transition-fast);
    }

    .btn-toggle:hover {
      background-color: var(--bg-hover);
      color: var(--text-primary);
    }

    .btn-toggle.active {
      background-color: var(--primary-glow);
      color: var(--primary-color);
    }

    /* Grid of members cards */
    .team-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 1.25rem;
    }

    .team-card {
      background-color: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-xl);
      padding: 1.25rem;
      box-shadow: var(--shadow-sm);
      display: flex;
      flex-direction: column;
      gap: 1.15rem;
      cursor: pointer;
      transition: border-color var(--transition-fast), box-shadow var(--transition-fast), transform var(--transition-fast);
    }

    .team-card:hover {
      border-color: var(--primary-color);
      box-shadow: var(--shadow-hover);
      transform: translateY(-2px);
    }

    .team-card-header {
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .avatar-large {
      width: 52px;
      height: 52px;
      border-radius: 50%;
      object-fit: cover;
      border: 2px solid var(--border-color);
    }

    .avatar-large-placeholder {
      width: 52px;
      height: 52px;
      border-radius: 50%;
      color: #fff;
      font-size: 1.25rem;
      font-weight: 700;
      display: flex;
      align-items: center;
      justify-content: center;
      text-transform: uppercase;
      border: 2px solid var(--border-color);
    }

    .user-info {
      display: flex;
      flex-direction: column;
      min-width: 0;
    }

    .user-info h3 {
      font-size: 0.95rem;
      font-weight: 700;
      color: var(--text-primary);
      margin: 0;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .user-email {
      font-size: 0.75rem;
      color: var(--text-secondary);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .team-card-body {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      background-color: var(--bg-hover);
      padding: 0.75rem;
      border-radius: var(--radius-lg);
    }

    .detail-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-size: 0.75rem;
    }

    .detail-label {
      color: var(--text-secondary);
      font-weight: 500;
    }

    .detail-value {
      color: var(--text-primary);
      font-weight: 600;
    }

    .role-badge {
      font-size: 0.65rem;
      font-weight: 700;
      background-color: rgba(99, 102, 241, 0.1);
      color: var(--primary-color);
      padding: 0.15rem 0.4rem;
      border-radius: var(--radius-sm);
      display: inline-block;
    }

    .role-badge.admin-role {
      background-color: rgba(245, 158, 11, 0.1);
      color: var(--warning-color);
    }

    .status-indicator {
      display: inline-flex;
      align-items: center;
      gap: 0.35rem;
    }

    .status-dot {
      width: 6px;
      height: 6px;
      border-radius: 50%;
      background-color: var(--success-color);
      display: inline-block;
    }

    /* Assigned Projects Styling */
    .assigned-projects-section {
      display: flex;
      flex-direction: column;
      gap: 0.45rem;
      border-top: 1px dashed var(--border-color);
      padding-top: 0.75rem;
    }

    .projects-title-label {
      font-size: 0.7rem;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      color: var(--text-muted);
    }

    .card-loader-mini {
      display: flex;
      align-items: center;
      padding: 0.2rem 0;
    }

    .spinner-tiny {
      width: 12px;
      height: 12px;
      border: 1.5px solid var(--border-color);
      border-top-color: var(--primary-color);
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }

    .no-projects-badge {
      font-size: 0.7rem;
      color: var(--text-muted);
      background-color: var(--bg-hover);
      padding: 0.15rem 0.45rem;
      border-radius: var(--radius-sm);
      width: max-content;
    }

    .card-projects-list, .list-projects-list {
      display: flex;
      flex-wrap: wrap;
      gap: 0.35rem;
    }

    .project-mini-badge {
      font-size: 0.65rem;
      font-weight: 700;
      color: var(--text-primary);
      background-color: var(--bg-hover);
      border: 1px solid var(--border-color);
      border-left: 3px solid var(--primary-color);
      padding: 0.1rem 0.4rem;
      border-radius: var(--radius-sm);
      transition: all var(--transition-fast);
    }

    .project-mini-badge:hover {
      border-color: var(--primary-color);
      background-color: var(--primary-glow);
    }

    .team-card-footer {
      border-top: 1px solid var(--border-color);
      padding-top: 0.75rem;
      text-align: center;
    }

    .member-since {
      font-size: 0.7rem;
      color: var(--text-muted);
    }

    /* List Layout Table Styles */
    .team-list-view {
      padding: 0;
      overflow: hidden;
      border-radius: var(--radius-xl);
      border: 1px solid var(--border-color);
      box-shadow: var(--shadow-sm);
    }

    .table-scroll {
      overflow-x: auto;
    }

    .members-table {
      width: 100%;
      border-collapse: collapse;
      font-size: 0.85rem;
      text-align: left;
    }

    .members-table th {
      background-color: var(--bg-hover);
      color: var(--text-secondary);
      font-weight: 600;
      padding: 0.75rem 1rem;
      border-bottom: 1px solid var(--border-color);
    }

    .members-table td {
      padding: 0.75rem 1rem;
      border-bottom: 1px solid var(--border-color);
      color: var(--text-primary);
      vertical-align: middle;
    }

    .member-row {
      transition: background-color var(--transition-fast);
      cursor: pointer;
    }

    .member-row:hover {
      background-color: var(--bg-hover);
    }

    .member-name-cell {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }

    .avatar-mini {
      width: 28px;
      height: 28px;
      border-radius: 50%;
      object-fit: cover;
      border: 1px solid var(--border-color);
    }

    .avatar-mini-placeholder {
      width: 28px;
      height: 28px;
      border-radius: 50%;
      color: #fff;
      font-size: 0.75rem;
      font-weight: 700;
      display: flex;
      align-items: center;
      justify-content: center;
      text-transform: uppercase;
      border: 1px solid var(--border-color);
    }

    .member-name {
      font-weight: 700;
      color: var(--text-primary);
    }

    .email-text {
      color: var(--text-secondary);
    }

    .no-projects-text-small {
      font-size: 0.75rem;
      color: var(--text-muted);
    }

    /* Modal Styles */
    .modal-overlay {
      position: fixed;
      top: 0;
      left: 0;
      width: 100vw;
      height: 100vh;
      background: rgba(15, 23, 42, 0.45);
      backdrop-filter: blur(4px);
      z-index: 1000;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .modal-container {
      background: var(--bg-panel, #fff);
      border-radius: var(--radius-xl);
      border: 1px solid var(--border-color);
      width: 90%;
      max-width: 480px;
      box-shadow: var(--shadow-lg);
    }

    .modal-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 1.25rem 1.5rem;
      border-bottom: 1px solid var(--border-color);
    }

    .modal-header h3 {
      margin: 0;
      font-size: 1.1rem;
      color: var(--text-primary);
    }

    .btn-close {
      background: none;
      border: none;
      font-size: 1.5rem;
      color: var(--text-muted);
      cursor: pointer;
    }

    .btn-close:hover {
      color: var(--text-primary);
    }

    .modal-body {
      padding: 1.5rem;
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    .profile-summary-header {
      display: flex;
      flex-direction: column;
      align-items: center;
      text-align: center;
      gap: 0.5rem;
    }

    .profile-modal-avatar {
      width: 80px;
      height: 80px;
      border-radius: 50%;
      object-fit: cover;
      border: 3px solid var(--primary-glow);
      box-shadow: var(--shadow-sm);
    }

    .profile-modal-avatar-placeholder {
      width: 80px;
      height: 80px;
      border-radius: 50%;
      color: #fff;
      font-size: 2rem;
      font-weight: 700;
      display: flex;
      align-items: center;
      justify-content: center;
      text-transform: uppercase;
      border: 3px solid var(--primary-glow);
      box-shadow: var(--shadow-sm);
    }

    .profile-summary-header h2 {
      font-size: 1.3rem;
      font-weight: 700;
      margin: 0.25rem 0 0;
      color: var(--text-primary);
    }

    .modal-role-badge {
      font-size: 0.7rem;
      font-weight: 700;
      background-color: var(--primary-glow);
      color: var(--primary-color);
      padding: 0.2rem 0.6rem;
      border-radius: var(--radius-md);
      margin-top: 0.25rem;
    }

    .modal-role-badge.admin-role {
      background-color: rgba(245, 158, 11, 0.1);
      color: var(--warning-color);
    }

    .modal-details-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
      background-color: var(--bg-hover);
      padding: 1rem;
      border-radius: var(--radius-lg);
      border: 1px solid var(--border-color);
    }

    .modal-info-item {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
    }

    .modal-info-item.full-width {
      grid-column: span 2;
      border-top: 1px solid var(--border-color);
      padding-top: 0.75rem;
      margin-top: 0.25rem;
    }

    .info-label {
      font-size: 0.7rem;
      color: var(--text-secondary);
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }

    .info-value {
      font-size: 0.85rem;
      color: var(--text-primary);
      font-weight: 600;
    }

    .email-link {
      color: var(--primary-color);
      text-decoration: none;
      display: inline-flex;
      align-items: center;
      gap: 0.25rem;
    }

    .email-link:hover {
      text-decoration: underline;
    }

    .icon-inline {
      font-size: 1rem;
    }

    .status-tag {
      display: inline-flex;
      align-items: center;
      gap: 0.35rem;
    }

    .no-projects-text {
      font-size: 0.8rem;
      color: var(--text-muted);
      margin: 0.25rem 0 0;
    }

    .projects-loader {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.8rem;
      color: var(--text-secondary);
      padding: 0.5rem 0;
    }

    .spinner-sm {
      width: 14px;
      height: 14px;
      border: 2px solid var(--border-color);
      border-top-color: var(--primary-color);
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }

    .projects-links-list {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      margin-top: 0.5rem;
      max-height: 150px;
      overflow-y: auto;
    }

    .project-link-card {
      display: flex;
      align-items: center;
      padding: 0.5rem;
      background: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      text-decoration: none;
      color: var(--text-primary);
      transition: all var(--transition-fast);
      gap: 0.5rem;
    }

    .project-link-card:hover {
      border-color: var(--primary-color);
      background-color: var(--primary-glow);
    }

    .proj-avatar-mini {
      width: 24px;
      height: 24px;
      border-radius: var(--radius-sm);
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-size: 0.65rem;
      font-weight: 700;
      text-transform: uppercase;
    }

    .proj-text {
      display: flex;
      flex-direction: column;
      flex: 1;
      min-width: 0;
    }

    .proj-name {
      font-size: 0.8rem;
      font-weight: 600;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .proj-key {
      font-size: 0.65rem;
      color: var(--text-muted);
    }

    .project-link-card .arrow-icon {
      font-size: 1.1rem;
      color: var(--text-muted);
    }

    .modal-footer {
      display: flex;
      justify-content: flex-end;
      padding: 1rem 1.5rem;
      border-top: 1px solid var(--border-color);
    }

    .btn {
      font-size: 0.8rem;
      font-weight: 600;
      padding: 0.5rem 1rem;
      border-radius: var(--radius-md);
      cursor: pointer;
      border: none;
      transition: all var(--transition-fast);
    }

    .btn-secondary {
      background-color: var(--bg-hover);
      color: var(--text-primary);
      border: 1px solid var(--border-color);
    }

    .btn-secondary:hover {
      background-color: var(--border-color);
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
      margin-top: 1rem;
    }

    .empty-state h3 {
      font-size: 1.1rem;
      font-weight: 700;
      margin: 0;
      color: var(--text-primary);
    }

    .empty-state p {
      font-size: 0.85rem;
      margin: 0;
      color: var(--text-secondary);
      max-width: 320px;
    }

    .empty-state span {
      font-size: 2.5rem;
      color: var(--text-muted);
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
export class TeamComponent implements OnInit {
  private readonly userService = inject(UserService);
  private readonly projectService = inject(ProjectService);
  private readonly backButtonService = inject(BackButtonService);

  readonly users = signal<UserDto[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  readonly selectedUser = signal<UserDto | null>(null);
  readonly userProjects = signal<ProjectResponse[]>([]);
  readonly loadingProjects = signal(false);

  // Search & Filter State Signals
  readonly searchQuery = signal('');
  readonly roleFilter = signal('');
  readonly viewMode = signal<'grid' | 'list'>('grid');

  // Mapped Projects per Member
  readonly memberProjectsMap = signal<Map<string, ProjectResponse[]>>(new Map());

  // Filtered Users computed signal
  readonly filteredUsers = computed(() => {
    let list = this.users();
    const query = this.searchQuery().toLowerCase().trim();
    const role = this.roleFilter();

    if (query) {
      list = list.filter(u => 
        u.firstName.toLowerCase().includes(query) ||
        u.lastName.toLowerCase().includes(query) ||
        u.email.toLowerCase().includes(query)
      );
    }

    if (role) {
      list = list.filter(u => u.role === role);
    }

    return list;
  });

  private detailsModalCleanup: (() => void) | null = null;
  private readonly detailsModalEffect = effect(() => {
    const user = this.selectedUser();
    if (user) {
      this.detailsModalCleanup = this.backButtonService.registerHandler(
        'TeamMemberDetailsModal',
        20,
        () => {
          this.closeDetailsModal();
          return true; // Consumed
        }
      );
    } else {
      if (this.detailsModalCleanup) {
        this.detailsModalCleanup();
        this.detailsModalCleanup = null;
      }
    }
  });

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading.set(true);
    this.error.set(null);

    this.userService.getAllUsers().subscribe({
      next: (users) => {
        this.users.set(users);
        this.loadMemberProjects(users);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load team members.');
        this.loading.set(false);
      }
    });
  }

  loadMemberProjects(users: UserDto[]): void {
    this.loadingProjects.set(true);
    this.projectService.getMyProjects().subscribe({
      next: (projects) => {
        const map = new Map<string, ProjectResponse[]>();
        
        if (projects.length === 0) {
          this.memberProjectsMap.set(map);
          this.loadingProjects.set(false);
          return;
        }

        let completedCalls = 0;
        projects.forEach((p) => {
          this.projectService.getMembers(p.projectId).subscribe({
            next: (members) => {
              members.forEach((m) => {
                if (!map.has(m.userId)) {
                  map.set(m.userId, []);
                }
                const userProjs = map.get(m.userId)!;
                if (!userProjs.some(proj => proj.projectId === p.projectId)) {
                  userProjs.push(p);
                }
              });
              completedCalls++;
              if (completedCalls === projects.length) {
                this.memberProjectsMap.set(map);
                this.loadingProjects.set(false);
              }
            },
            error: () => {
              completedCalls++;
              if (completedCalls === projects.length) {
                this.memberProjectsMap.set(map);
                this.loadingProjects.set(false);
              }
            }
          });
        });
      },
      error: () => {
        this.loadingProjects.set(false);
      }
    });
  }

  getMemberProjects(userId: string): ProjectResponse[] {
    return this.memberProjectsMap().get(userId) || [];
  }

  onSearchChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchQuery.set(input.value);
  }

  onRoleFilterChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.roleFilter.set(select.value);
  }

  selectUser(user: UserDto): void {
    this.selectedUser.set(user);
    const aligned = this.memberProjectsMap().get(user.userId) || [];
    this.userProjects.set(aligned);
  }

  closeDetailsModal(): void {
    this.selectedUser.set(null);
    this.userProjects.set([]);
  }

  getUserInitials(user: UserDto): string {
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

  getProjectColor(name: string): string {
    const colors = [
      '#6366f1, #4f46e5', // Indigo
      '#3b82f6, #2563eb', // Blue
      '#10b981, #059669', // Emerald
      '#f59e0b, #d97706', // Amber
      '#ec4899, #db2777', // Pink
      '#8b5cf6, #7c3aed', // Purple
    ];
    let hash = 0;
    for (let i = 0; i < name.length; i++) {
      hash = name.charCodeAt(i) + ((hash << 5) - hash);
    }
    const index = Math.abs(hash) % colors.length;
    return colors[index];
  }

  getProjectColorBorder(name: string): string {
    const colorPair = this.getProjectColor(name);
    return colorPair.split(',')[0].trim();
  }
}
