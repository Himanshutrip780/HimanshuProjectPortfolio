import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TeamService } from '../../services/team.service';
import { UserService } from '../../../../core/services/user.service';
import { AuthService } from '../../../../core/services/auth.service';
import { BackButtonService } from '../../../../core/services/back-button.service';
import { TeamResponse, TeamMemberResponse, TeamAnalytics } from '../../../../core/models/team.models';
import { UserDto } from '../../../../core/models/user.models';
import { TaskResponse, BoardResponse, TaskStatus, TaskPriority, IssueType } from '../../../../core/models/task.models';
import { getApiErrorMessage } from '../../../../core/utils/api-error.util';

@Component({
  selector: 'app-team-profile',
  standalone: true,
  imports: [RouterLink, FormsModule, CommonModule],
  templateUrl: './team-profile.component.html',
  styles: [`
    .team-profile-container {
      padding: 2rem;
      max-width: 1200px;
      margin: 0 auto;
    }

    .profile-nav-header {
      margin-bottom: 1.5rem;
      .btn-back {
        padding: 0.4rem 0.85rem;
        font-size: 0.875rem;
      }
    }

    .profile-banner {
      background: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-xl);
      padding: 2rem;
      box-shadow: var(--shadow-sm);
      margin-bottom: 2rem;

      &.archived {
        opacity: 0.8;
        background: repeating-linear-gradient(
          -45deg,
          var(--bg-card),
          var(--bg-card) 10px,
          var(--bg-hover) 10px,
          var(--bg-hover) 20px
        );
      }

      .banner-main {
        display: flex;
        gap: 2rem;
        align-items: flex-start;

        .banner-avatar {
          width: 96px;
          height: 96px;
          border-radius: var(--radius-xl);
          object-fit: cover;
          border: 2px solid var(--border-color);
        }

        .banner-avatar-fallback {
          width: 96px;
          height: 96px;
          border-radius: var(--radius-xl);
          color: white;
          font-weight: 700;
          font-size: 2.25rem;
          display: flex;
          align-items: center;
          justify-content: center;
          text-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
        }

        .banner-info {
          flex: 1;

          .title-row {
            display: flex;
            align-items: center;
            gap: 1rem;
            margin-bottom: 0.5rem;

            h1 {
              margin: 0;
              font-size: 1.75rem;
              font-weight: 700;
            }
          }

          .desc {
            font-size: 1rem;
            color: var(--text-secondary);
            margin-bottom: 1.25rem;
          }

          .meta-row {
            display: flex;
            gap: 1.5rem;
            flex-wrap: wrap;

            .meta-item {
              display: flex;
              align-items: center;
              gap: 0.4rem;
              font-size: 0.875rem;
              color: var(--text-secondary);

              span {
                font-size: 1.15rem;
                color: var(--text-muted);
              }
            }
          }
        }
      }
    }

    .tabs-nav {
      display: flex;
      gap: 0.5rem;
      border-bottom: 1px solid var(--border-color);
      margin-bottom: 2rem;

      button {
        background: transparent;
        border: none;
        border-bottom: 2px solid transparent;
        border-radius: 0;
        padding: 0.75rem 1.25rem;
        color: var(--text-secondary);
        font-weight: 500;
        cursor: pointer;
        transition: all var(--transition-fast);

        &:hover {
          color: var(--text-primary);
          background: var(--bg-hover);
        }

        &.active {
          color: var(--primary-color);
          border-bottom-color: var(--primary-color);
        }
      }
    }

    /* Dashboard Metrics Layout */
    .dashboard-grid {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    .metrics-cards-row {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 1.25rem;

      .metric-card {
        background: var(--bg-card);
        border: 1px solid var(--border-color);
        border-radius: var(--radius-xl);
        padding: 1.25rem;
        box-shadow: var(--shadow-sm);
        position: relative;
        display: flex;
        flex-direction: column;

        .metric-label {
          font-size: 0.75rem;
          font-weight: 700;
          color: var(--text-secondary);
          text-transform: uppercase;
          letter-spacing: 0.05em;
          margin-bottom: 0.5rem;
        }

        .metric-value {
          font-size: 1.75rem;
          font-weight: 700;
          color: var(--text-primary);
        }

        .metric-subtitle {
          font-size: 0.75rem;
          color: var(--text-muted);
          margin-top: 0.15rem;
        }

        .metric-icon {
          position: absolute;
          right: 1.25rem;
          bottom: 1.25rem;
          font-size: 1.5rem;
          color: var(--text-muted);
          opacity: 0.7;
        }
      }
    }

    .charts-row {
      display: grid;
      grid-template-columns: 1.5fr 1fr;
      gap: 1.5rem;

      @media (max-width: 900px) {
        grid-template-columns: 1fr;
      }
    }

    .chart-panel {
      padding: 1.5rem;

      h3 {
        font-size: 1.15rem;
        font-weight: 600;
        margin-bottom: 1.25rem;
      }

      .status-distribution-list {
        display: flex;
        flex-direction: column;
        gap: 1.15rem;

        .status-dist-item {
          .dist-header {
            display: flex;
            justify-content: space-between;
            margin-bottom: 0.4rem;
            font-size: 0.875rem;

            .dist-label {
              font-weight: 500;
              color: var(--text-primary);
            }

            .dist-count {
              color: var(--text-secondary);
            }
          }

          .progress-track {
            height: 8px;
            background: var(--bg-hover);
            border-radius: 99px;
            overflow: hidden;

            .progress-fill {
              height: 100%;
              border-radius: 99px;

              &.p-todo { background: var(--text-muted); }
              &.p-inprogress { background: var(--info-color); }
              &.p-review { background: var(--warning-color); }
              &.p-done { background: var(--success-color); }
              &.p-blocked { background: var(--danger-color); }
            }
          }
        }
      }

      .planning-info-list {
        display: flex;
        flex-direction: column;
        gap: 1.5rem;

        .planning-item {
          display: flex;
          align-items: center;
          gap: 1rem;

          span {
            font-size: 2rem;
            padding: 0.5rem;
            background: var(--bg-hover);
            border-radius: var(--radius-lg);
          }

          h4 {
            margin: 0;
            font-size: 0.95rem;
            font-weight: 600;
          }
        }
      }
    }

    /* Roster Table Layout */
    .tab-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;

      h3 {
        margin: 0;
      }

      .roster-actions {
        display: flex;
        gap: 0.75rem;
      }
    }

    .table-container {
      width: 100%;
      overflow-x: auto;
    }

    table {
      width: 100%;
      border-collapse: collapse;
      text-align: left;
      font-size: 0.9rem;

      th, td {
        padding: 0.85rem 1rem;
        border-bottom: 1px solid var(--border-color);
      }

      th {
        font-weight: 600;
        color: var(--text-secondary);
        background: var(--bg-hover);
      }
    }

    .member-row {
      cursor: pointer;
      transition: background-color var(--transition-fast);

      &:hover {
        background-color: var(--bg-hover);
      }
    }

    .member-cell-info {
      display: flex;
      align-items: center;
      gap: 0.75rem;

      .roster-avatar {
        width: 32px;
        height: 32px;
        border-radius: 50%;
        object-fit: cover;
      }

      .roster-avatar-fallback {
        width: 32px;
        height: 32px;
        border-radius: 50%;
        color: white;
        font-weight: 700;
        font-size: 0.8rem;
        display: flex;
        align-items: center;
        justify-content: center;
        text-shadow: 0 1px 1px rgba(0,0,0,0.15);
      }

      .member-name {
        font-weight: 500;
        color: var(--text-primary);
      }
    }

    .actions-header {
      text-align: right;
    }

    .actions-cell {
      text-align: right;

      .row-actions {
        display: inline-flex;
        align-items: center;
        gap: 0.5rem;

        select {
          padding: 0.25rem 0.5rem;
          font-size: 0.8rem;
          width: auto;
        }

        .btn-icon {
          background: transparent;
          border: none;
          padding: 0.25rem;
          cursor: pointer;
          border-radius: 50%;
          display: flex;
          align-items: center;
          justify-content: center;
          transition: background var(--transition-fast);

          &:hover {
            background: var(--bg-hover);
          }
        }
      }
    }

    /* Issues Table */
    .issue-key-link {
      font-weight: 600;
      color: var(--primary-color);
      text-decoration: none;
      &:hover {
        text-decoration: underline;
      }
    }

    .task-title-link {
      color: var(--text-primary);
      text-decoration: none;
      font-weight: 500;
      &:hover {
        color: var(--primary-color);
      }
    }

    .type-badge, .priority-badge, .status-pill {
      font-size: 0.75rem;
      padding: 0.2rem 0.5rem;
      border-radius: 4px;
      font-weight: 500;
    }

    .type-badge {
      background: var(--bg-hover);
      color: var(--text-primary);
      &.t-story { border-left: 3px solid #10b981; }
      &.t-task { border-left: 3px solid #3b82f6; }
      &.t-bug { border-left: 3px solid #ef4444; }
      &.t-subtask { border-left: 3px solid #a1a1aa; }
    }

    .priority-badge {
      &.p-low { background: rgba(16, 185, 129, 0.1); color: #10b981; }
      &.p-medium { background: rgba(59, 130, 246, 0.1); color: #3b82f6; }
      &.p-high { background: rgba(245, 158, 11, 0.1); color: #f59e0b; }
      &.p-critical { background: rgba(239, 68, 68, 0.1); color: #ef4444; }
    }

    .status-pill {
      display: inline-block;
      border-radius: 99px;
      font-weight: 600;
      font-size: 0.7rem;
      text-transform: uppercase;
      padding: 0.15rem 0.5rem;

      &.s-todo { background: var(--bg-hover); color: var(--text-secondary); }
      &.s-inprogress { background: rgba(59, 130, 246, 0.15); color: #3b82f6; }
      &.s-review { background: rgba(245, 158, 11, 0.15); color: #f59e0b; }
      &.s-done { background: rgba(16, 185, 129, 0.15); color: #10b981; }
      &.s-blocked { background: rgba(239, 68, 68, 0.15); color: #ef4444; }
    }

    .assignee-cell {
      display: flex;
      align-items: center;
      gap: 0.4rem;

      .mini-avatar {
        width: 20px;
        height: 20px;
        border-radius: 50%;
        object-fit: cover;
      }

      .mini-avatar-fallback {
        width: 20px;
        height: 20px;
        border-radius: 50%;
        font-size: 0.5rem;
        font-weight: 700;
        color: white;
        display: flex;
        align-items: center;
        justify-content: center;
      }
    }

    .overdue {
      color: var(--danger-color);
      font-weight: 600;
    }

    .empty-table-cell {
      text-align: center;
      padding: 3rem 1rem !important;
      color: var(--text-muted);

      span {
        font-size: 2rem;
        margin-bottom: 0.5rem;
      }
    }

    /* Linked Boards List */
    .linked-items-list {
      display: flex;
      flex-direction: column;
      gap: 1rem;

      .linked-item {
        display: flex;
        align-items: center;
        gap: 1rem;
        padding: 0.75rem 1rem;
        background: var(--bg-hover);
        border: 1px solid var(--border-color);
        border-radius: var(--radius-lg);

        .item-icon {
          font-size: 1.75rem;
        }

        h4 {
          margin: 0;
          font-size: 0.95rem;
          font-weight: 600;
        }
      }
    }

    /* Settings Tab */
    .settings-form {
      max-width: 600px;
      display: flex;
      flex-direction: column;
      gap: 1.25rem;

      .form-group {
        display: flex;
        flex-direction: column;
        gap: 0.4rem;

        label {
          font-size: 0.875rem;
          font-weight: 500;
          color: var(--text-secondary);
        }
      }
    }

    .danger-zone {
      .danger-row {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 2rem;

        h4 {
          margin: 0 0 0.25rem 0;
        }

        .transfer-box {
          display: flex;
          align-items: center;
          gap: 0.5rem;

          select {
            width: auto;
            min-width: 200px;
          }
        }
      }
    }

    /* Member Detail Modal unique styles */
    .member-detail-card {
      max-width: 420px;

      .detail-avatar {
        width: 80px;
        height: 80px;
        border-radius: 50%;
        object-fit: cover;
        border: 2px solid var(--border-color);
      }

      .detail-avatar-fallback {
        width: 80px;
        height: 80px;
        border-radius: 50%;
        color: white;
        font-weight: 700;
        font-size: 1.75rem;
        display: flex;
        align-items: center;
        justify-content: center;
        text-shadow: 0 2px 4px rgba(0,0,0,0.15);
      }

      .detail-badges {
        display: flex;
        justify-content: center;
      }
    }

    .member-task-item {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 0.6rem 0.75rem;
      background: var(--bg-hover);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      margin-bottom: 0.5rem;
      font-size: 0.85rem;

      .task-key {
        font-weight: 600;
        color: var(--primary-color);
        text-decoration: none;
      }

      .task-title {
        flex: 1;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
      }

      .status-pill.mini {
        font-size: 0.65rem;
        padding: 0.1rem 0.35rem;
      }
    }

    /* Modal Backdrop shared animations */
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
    }

    .modal-card {
      background: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-xl);
      width: 100%;
      max-width: 500px;
      box-shadow: var(--shadow-lg);
      overflow: hidden;

      .modal-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 1.25rem 1.5rem;
        border-bottom: 1px solid var(--border-color);

        h2 {
          margin: 0;
        }

        .btn-close {
          background: transparent;
          border: none;
          color: var(--text-muted);
          padding: 0.25rem;
          border-radius: 50%;
          cursor: pointer;

          &:hover {
            background: var(--bg-hover);
            color: var(--text-primary);
          }
        }
      }

      .form-body {
        padding: 1.5rem;
        display: flex;
        flex-direction: column;
        gap: 1.25rem;

        .form-group {
          display: flex;
          flex-direction: column;
          gap: 0.4rem;

          label {
            font-size: 0.875rem;
            font-weight: 500;
            color: var(--text-secondary);
          }
        }
      }

      .modal-footer {
        display: flex;
        justify-content: flex-end;
        gap: 0.75rem;
        padding: 1rem 1.5rem;
        background: var(--bg-hover);
        border-top: 1px solid var(--border-color);
      }
    }

    .col-6 {
      grid-column: span 6;
      @media (max-width: 768px) {
        grid-column: span 12;
      }
    }

    .d-flex { display: flex; }
    .flex-column { flex-direction: column; }
    .align-items-center { align-items: center; }
    .text-center { text-align: center; }
    .w-100 { width: 100%; }
    .text-start { text-align: left; }
    .mt-2 { margin-top: 0.5rem; }
    .mt-3 { margin-top: 0.75rem; }
    .mt-4 { margin-top: 1rem; }
    .mt-5 { margin-top: 1.5rem; }
    .pt-3 { padding-top: 0.75rem; }
    .pt-4 { padding-top: 1rem; }
    .py-3 { padding-top: 0.75rem; padding-bottom: 0.75rem; }
    .py-4 { padding-top: 1rem; padding-bottom: 1rem; }
    .ps-2 { padding-left: 0.5rem; }
    .ms-2 { margin-left: 0.5rem; }
    .me-2 { margin-right: 0.5rem; }
    .mb-4 { margin-bottom: 1rem; }
    .border-top { border-top: 1px solid var(--border-color); }
    .text-danger { color: var(--danger-color); }
    .text-success { color: var(--success-color); }
    .text-info { color: var(--info-color); }
    .text-primary { color: var(--primary-color); }
    .font-size-sm { font-size: 0.8rem; }
  `]
})
export class TeamProfileComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly teamService = inject(TeamService);
  private readonly userService = inject(UserService);
  private readonly auth = inject(AuthService);
  private readonly backButton = inject(BackButtonService);

  // States
  team = signal<TeamResponse | null>(null);
  users = signal<UserDto[]>([]);
  teamIssues = signal<TaskResponse[]>([]);
  teamBoards = signal<BoardResponse[]>([]);
  analytics = signal<TeamAnalytics | null>(null);
  
  loading = signal<boolean>(true);
  error = signal<string | null>(null);
  busy = signal<boolean>(false);
  
  activeTab = signal<string>('dashboard');

  // Roster user lookup map
  private userMap = new Map<string, UserDto>();

  // Member details modal
  showMemberDetailModal = signal<boolean>(false);
  selectedMemberDetails = signal<UserDto | null>(null);

  // Invite modal states
  showInviteModal = signal<boolean>(false);
  inviteUserId = '';
  inviteRole = 'Member';
  inviteError = signal<string | null>(null);

  // Edit Settings states
  editName = '';
  editDescription = '';
  editVisibility = 'Public';
  editAvatarUrl = '';
  newLeadId = '';
  settingsError = signal<string | null>(null);
  settingsSuccess = signal<string | null>(null);

  // Derived signals
  currentUserId = computed(() => this.auth.currentUser()?.userId || '');
  isLead = computed(() => this.team()?.leadId === this.currentUserId());
  isUserMember = computed(() => {
    const t = this.team();
    if (!t) return false;
    return t.members.some(m => m.userId === this.currentUserId());
  });

  activeSprintsCount = computed(() => {
    // Collect distinct sprints in done/inprogress tasks
    const sprints = this.teamIssues()
      .filter(task => task.sprintId != null)
      .map(task => task.sprintId);
    return new Set(sprints).size;
  });

  backlogCount = computed(() => {
    return this.teamIssues().filter(task => task.sprintId == null).length;
  });

  linkedProjects = computed(() => {
    const projects = new Map<string, { projectId: string, name: string, key: string }>();
    // Look up task projects
    this.teamIssues().forEach(task => {
      // Find matching board for this projectId to get project metadata if available
      const b = this.teamBoards().find(board => board.projectId === task.projectId);
      if (b && !projects.has(task.projectId)) {
        projects.set(task.projectId, {
          projectId: task.projectId,
          name: b.name.replace(' Board', ''),
          key: task.issueKey.split('-')[0]
        });
      }
    });
    return Array.from(projects.values());
  });

  ngOnInit(): void {
    const teamId = this.route.snapshot.paramMap.get('teamId');
    if (teamId) {
      this.loadTeamProfile(teamId);
    } else {
      this.error.set('No Team ID specified.');
      this.loading.set(false);
    }
  }

  loadTeamProfile(teamId: string): void {
    this.loading.set(true);
    this.error.set(null);

    // Fetch team details, workspace members, issues, boards, analytics
    this.userService.getAllUsers().subscribe({
      next: (usersList) => {
        this.users.set(usersList);
        this.userMap.clear();
        usersList.forEach(u => this.userMap.set(u.userId, u));

        this.fetchTeamDetails(teamId);
      },
      error: (err) => {
        this.error.set(getApiErrorMessage(err, 'Failed to load workspace members.'));
        this.loading.set(false);
      }
    });
  }

  fetchTeamDetails(teamId: string): void {
    this.teamService.getTeamById(teamId).subscribe({
      next: (teamData) => {
        this.team.set(teamData);

        // Prep settings edit variables
        this.editName = teamData.name;
        this.editDescription = teamData.description || '';
        this.editVisibility = teamData.visibility;
        this.editAvatarUrl = teamData.avatarUrl || '';

        // Fetch dependent data in parallel/sequential
        this.fetchTeamIssues(teamId);
        this.fetchTeamBoards(teamId);
        this.fetchTeamAnalytics(teamId);
      },
      error: (err) => {
        this.error.set(getApiErrorMessage(err, 'Failed to retrieve team profile details.'));
        this.loading.set(false);
      }
    });
  }

  fetchTeamIssues(teamId: string): void {
    this.teamService.getTeamIssues(teamId).subscribe({
      next: (issues) => {
        this.teamIssues.set(issues);
      }
    });
  }

  fetchTeamBoards(teamId: string): void {
    this.teamService.getTeamBoards(teamId).subscribe({
      next: (boards) => {
        this.teamBoards.set(boards);
      }
    });
  }

  fetchTeamAnalytics(teamId: string): void {
    this.teamService.getTeamAnalytics(teamId).subscribe({
      next: (stats) => {
        this.analytics.set(stats);
        this.loading.set(false);
      },
      error: () => {
        // Fallback or finish loading even if analytics fails
        this.loading.set(false);
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/teams']);
  }

  // Membership operations
  joinTeam(): void {
    const t = this.team();
    const currentId = this.currentUserId();
    if (!t || !currentId) return;

    this.busy.set(true);
    this.teamService.addMember(t.teamId, { userId: currentId, role: 'Member' }).subscribe({
      next: (member) => {
        this.team.update(teamObj => {
          if (!teamObj) return null;
          return {
            ...teamObj,
            members: [...teamObj.members, member]
          };
        });
        this.busy.set(false);
        this.fetchTeamAnalytics(t.teamId);
      },
      error: (err) => {
        alert(getApiErrorMessage(err, 'Failed to join team.'));
        this.busy.set(false);
      }
    });
  }

  leaveTeam(): void {
    const t = this.team();
    const currentId = this.currentUserId();
    if (!t || !currentId) return;

    if (!confirm('Are you sure you want to leave this team?')) return;

    this.busy.set(true);
    this.teamService.removeMember(t.teamId, currentId).subscribe({
      next: () => {
        this.team.update(teamObj => {
          if (!teamObj) return null;
          return {
            ...teamObj,
            members: teamObj.members.filter(m => m.userId !== currentId)
          };
        });
        this.busy.set(false);
        this.fetchTeamAnalytics(t.teamId);
      },
      error: (err) => {
        alert(getApiErrorMessage(err, 'Failed to leave team.'));
        this.busy.set(false);
      }
    });
  }

  openInviteModal(): void {
    this.inviteUserId = '';
    this.inviteRole = 'Member';
    this.inviteError.set(null);
    this.showInviteModal.set(true);
  }

  closeInviteModal(): void {
    this.showInviteModal.set(false);
  }

  inviteMember(): void {
    const t = this.team();
    if (!t || !this.inviteUserId) return;

    this.busy.set(true);
    this.inviteError.set(null);

    this.teamService.addMember(t.teamId, { userId: this.inviteUserId, role: this.inviteRole }).subscribe({
      next: (newMember) => {
        this.team.update(teamObj => {
          if (!teamObj) return null;
          return {
            ...teamObj,
            members: [...teamObj.members, newMember]
          };
        });
        this.busy.set(false);
        this.closeInviteModal();
        this.fetchTeamAnalytics(t.teamId);
      },
      error: (err) => {
        this.inviteError.set(getApiErrorMessage(err, 'Failed to invite member.'));
        this.busy.set(false);
      }
    });
  }

  removeMember(userId: string): void {
    const t = this.team();
    if (!t) return;

    if (!confirm(`Are you sure you want to remove this member?`)) return;

    this.busy.set(true);
    this.teamService.removeMember(t.teamId, userId).subscribe({
      next: () => {
        this.team.update(teamObj => {
          if (!teamObj) return null;
          return {
            ...teamObj,
            members: teamObj.members.filter(m => m.userId !== userId)
          };
        });
        this.busy.set(false);
        this.fetchTeamAnalytics(t.teamId);
      },
      error: (err) => {
        alert(getApiErrorMessage(err, 'Failed to remove member.'));
        this.busy.set(false);
      }
    });
  }

  changeMemberRole(userId: string, newRole: string): void {
    const t = this.team();
    if (!t) return;

    this.busy.set(true);
    this.teamService.changeMemberRole(t.teamId, userId, { role: newRole }).subscribe({
      next: (updatedMember) => {
        this.team.update(teamObj => {
          if (!teamObj) return null;
          return {
            ...teamObj,
            members: teamObj.members.map(m => m.userId === userId ? updatedMember : m)
          };
        });
        this.busy.set(false);
      },
      error: (err) => {
        alert(getApiErrorMessage(err, 'Failed to change role.'));
        this.busy.set(false);
      }
    });
  }

  // Settings modification
  updateTeamDetails(event: Event): void {
    event.preventDefault();
    const t = this.team();
    if (!t) return;

    this.busy.set(true);
    this.settingsError.set(null);
    this.settingsSuccess.set(null);

    this.teamService.updateTeam(t.teamId, {
      name: this.editName,
      description: this.editDescription,
      visibility: this.editVisibility,
      avatarUrl: this.editAvatarUrl,
      leadId: t.leadId
    }).subscribe({
      next: (updated) => {
        this.team.set(updated);
        this.settingsSuccess.set('Team details updated successfully.');
        this.busy.set(false);
      },
      error: (err) => {
        this.settingsError.set(getApiErrorMessage(err, 'Failed to update details.'));
        this.busy.set(false);
      }
    });
  }

  archiveTeam(): void {
    const t = this.team();
    if (!t) return;

    if (!confirm('Are you sure you want to archive this team? (Archived teams remain in reports but are hidden from search)')) return;

    this.busy.set(true);
    this.teamService.archiveTeam(t.teamId).subscribe({
      next: () => {
        this.team.update(teamObj => {
          if (!teamObj) return null;
          return {
            ...teamObj,
            isArchived: true
          };
        });
        this.busy.set(false);
      },
      error: (err) => {
        alert(getApiErrorMessage(err, 'Failed to archive team.'));
        this.busy.set(false);
      }
    });
  }

  restoreTeam(): void {
    const t = this.team();
    if (!t) return;

    this.busy.set(true);
    this.teamService.restoreTeam(t.teamId).subscribe({
      next: () => {
        this.team.update(teamObj => {
          if (!teamObj) return null;
          return {
            ...teamObj,
            isArchived: false
          };
        });
        this.busy.set(false);
      },
      error: (err) => {
        alert(getApiErrorMessage(err, 'Failed to restore team.'));
        this.busy.set(false);
      }
    });
  }

  transferOwnership(): void {
    const t = this.team();
    if (!t || !this.newLeadId) return;

    if (!confirm('Transferring ownership demotes you to Member and makes the selected user the Team Lead. Proceed?')) return;

    this.busy.set(true);
    this.teamService.transferOwnership(t.teamId, { newLeadId: this.newLeadId }).subscribe({
      next: (updated) => {
        this.team.set(updated);
        this.newLeadId = '';
        this.settingsSuccess.set('Ownership transferred successfully.');
        this.busy.set(false);
      },
      error: (err) => {
        this.settingsError.set(getApiErrorMessage(err, 'Failed to transfer ownership.'));
        this.busy.set(false);
      }
    });
  }

  // Roster click member detail modal
  openMemberDetailModal(member: TeamMemberResponse): void {
    const user = this.userMap.get(member.userId);
    if (user) {
      this.selectedMemberDetails.set(user);
      this.showMemberDetailModal.set(true);
    }
  }

  closeMemberDetailModal(): void {
    this.showMemberDetailModal.set(false);
    this.selectedMemberDetails.set(null);
  }

  getMemberTasks(userId: string): TaskResponse[] {
    return this.teamIssues().filter(task => task.assigneeId === userId);
  }

  // Roster helpers
  getUserName(userId: string): string {
    const user = this.userMap.get(userId);
    return user ? `${user.firstName} ${user.lastName}` : 'Unknown Member';
  }

  getUserEmail(userId: string): string {
    const user = this.userMap.get(userId);
    return user ? user.email : 'unknown@workflow.io.com';
  }

  getUserAvatar(userId: string): string | null {
    const user = this.userMap.get(userId);
    return user ? user.avatarUrl : null;
  }

  getUserInitials(userId: string): string {
    const user = this.userMap.get(userId);
    if (!user) return '??';
    return `${user.firstName.charAt(0)}${user.lastName.charAt(0)}`.toUpperCase();
  }

  isAlreadyMember(userId: string): boolean {
    const t = this.team();
    return t ? t.members.some(m => m.userId === userId) : false;
  }

  getGradient(name: string): string {
    let hash = 0;
    for (let i = 0; i < name.length; i++) {
      hash = name.charCodeAt(i) + ((hash << 5) - hash);
    }
    const h1 = Math.abs(hash % 360);
    const h2 = (h1 + 40) % 360;
    return `linear-gradient(135deg, hsl(${h1}, 70%, 45%) 0%, hsl(${h2}, 75%, 35%) 100%)`;
  }

  // Task lists helpers
  getIssueTypeName(type: IssueType): string {
    switch (type) {
      case IssueType.Story: return 'Story';
      case IssueType.Task: return 'Task';
      case IssueType.Bug: return 'Bug';
      case IssueType.SubTask: return 'Subtask';
      default: return 'Task';
    }
  }

  getIssueTypeClass(type: IssueType): string {
    switch (type) {
      case IssueType.Story: return 't-story';
      case IssueType.Task: return 't-task';
      case IssueType.Bug: return 't-bug';
      case IssueType.SubTask: return 't-subtask';
      default: return 't-task';
    }
  }

  getPriorityName(pri: TaskPriority): string {
    switch (pri) {
      case TaskPriority.Low: return 'Low';
      case TaskPriority.Medium: return 'Medium';
      case TaskPriority.High: return 'High';
      case TaskPriority.Critical: return 'Critical';
      default: return 'Medium';
    }
  }

  getPriorityClass(pri: TaskPriority): string {
    switch (pri) {
      case TaskPriority.Low: return 'p-low';
      case TaskPriority.Medium: return 'p-medium';
      case TaskPriority.High: return 'p-high';
      case TaskPriority.Critical: return 'p-critical';
      default: return 'p-medium';
    }
  }

  getStatusName(stat: TaskStatus): string {
    switch (stat) {
      case TaskStatus.Todo: return 'Todo';
      case TaskStatus.InProgress: return 'In Progress';
      case TaskStatus.Review: return 'In Review';
      case TaskStatus.Done: return 'Done';
      case TaskStatus.Blocked: return 'Blocked';
      default: return 'Todo';
    }
  }

  getStatusClass(stat: TaskStatus): string {
    switch (stat) {
      case TaskStatus.Todo: return 's-todo';
      case TaskStatus.InProgress: return 's-inprogress';
      case TaskStatus.Review: return 's-review';
      case TaskStatus.Done: return 's-done';
      case TaskStatus.Blocked: return 's-blocked';
      default: return 's-todo';
    }
  }

  getStatusColorClass(statusName: string): string {
    switch (statusName.toLowerCase()) {
      case 'todo': return 'p-todo';
      case 'inprogress': return 'p-inprogress';
      case 'review': return 'p-review';
      case 'done': return 'p-done';
      case 'blocked': return 'p-blocked';
      default: return 'p-todo';
    }
  }
}
