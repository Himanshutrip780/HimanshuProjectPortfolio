import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TeamService } from '../../services/team.service';
import { UserService } from '../../../../core/services/user.service';
import { AuthService } from '../../../../core/services/auth.service';
import { TeamResponse, CreateTeamRequest } from '../../../../core/models/team.models';
import { UserDto } from '../../../../core/models/user.models';
import { getApiErrorMessage } from '../../../../core/utils/api-error.util';

@Component({
  selector: 'app-teams-list',
  standalone: true,
  imports: [RouterLink, FormsModule, CommonModule],
  templateUrl: './teams-list.component.html',
  styles: [`
    .teams-container {
      padding: 2rem;
      max-width: 1200px;
      margin: 0 auto;
    }

    .teams-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 2rem;

      .header-title-group {
        display: flex;
        align-items: center;
        gap: 1rem;

        .header-icon {
          font-size: 2.5rem;
          background: var(--primary-gradient);
          -webkit-background-clip: text;
          -webkit-text-fill-color: transparent;
        }

        h1 {
          margin: 0;
        }
      }
    }

    .toolbar {
      display: flex;
      gap: 1rem;
      align-items: center;
      margin-bottom: 2rem;

      .search-box {
        position: relative;
        flex: 1;

        .search-icon {
          position: absolute;
          left: 0.75rem;
          top: 50%;
          transform: translateY(-50%);
          color: var(--text-muted);
        }

        input {
          padding-left: 2.5rem;
        }
      }

      .filter-group {
        select {
          min-width: 180px;
        }
      }
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

      .empty-icon, .spinner, .text-danger {
        margin-bottom: 1.5rem;
      }

      .empty-icon, .text-danger {
        font-size: 4rem;
      }

      .text-danger {
        color: var(--danger-color, #ef4444);
      }

      h3 {
        margin-bottom: 0.5rem;
      }

      p {
        margin-bottom: 1.5rem;
      }
    }

    .teams-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(340px, 1fr));
      gap: 1.5rem;
    }

    .team-card {
      display: flex;
      flex-direction: column;
      background: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-xl);
      padding: 1.5rem;
      text-decoration: none;
      color: inherit;
      box-shadow: var(--shadow-sm);
      transition: all var(--transition-normal);

      &:hover {
        border-color: var(--primary-color);
        box-shadow: var(--shadow-hover);
      }

      &.archived {
        opacity: 0.7;
        background: repeating-linear-gradient(
          -45deg,
          var(--bg-card),
          var(--bg-card) 10px,
          var(--bg-hover) 10px,
          var(--bg-hover) 20px
        );
      }

      .team-card-header {
        display: flex;
        align-items: center;
        gap: 1rem;
        margin-bottom: 1rem;

        .team-avatar {
          width: 50px;
          height: 50px;
          border-radius: var(--radius-lg);
          object-fit: cover;
          border: 1px solid var(--border-color);
        }

        .team-avatar-fallback {
          width: 50px;
          height: 50px;
          border-radius: var(--radius-lg);
          color: white;
          font-weight: 700;
          font-size: 1.25rem;
          display: flex;
          align-items: center;
          justify-content: center;
          text-shadow: 0 1px 2px rgba(0, 0, 0, 0.2);
        }

        .team-title-details {
          display: flex;
          flex-direction: column;
          gap: 0.25rem;

          h3 {
            margin: 0;
            font-size: 1.15rem;
            font-weight: 600;
          }

          .badge-group {
            display: flex;
            gap: 0.35rem;
          }
        }
      }

      .team-desc {
        font-size: 0.925rem;
        color: var(--text-secondary);
        flex: 1;
        margin-bottom: 1.5rem;
        display: -webkit-box;
        -webkit-line-clamp: 2;
        -webkit-box-orient: vertical;
        overflow: hidden;
      }

      .team-card-footer {
        display: flex;
        justify-content: space-between;
        align-items: center;
        border-top: 1px solid var(--border-color);
        padding-top: 1rem;

        .lead-info {
          display: flex;
          flex-direction: column;
          gap: 0.1rem;

          .lead-name {
            font-size: 0.875rem;
            font-weight: 500;
            color: var(--text-primary);
          }
        }

        .members-pile {
          display: flex;
          align-items: center;

          .member-pile-avatar, .member-pile-avatar-fallback, .member-pile-count {
            width: 28px;
            height: 28px;
            border-radius: 50%;
            border: 2px solid var(--bg-card);
            margin-left: -8px;
            object-fit: cover;

            &:first-child {
              margin-left: 0;
            }
          }

          .member-pile-avatar-fallback {
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 0.65rem;
            font-weight: 700;
            color: white;
            text-shadow: 0 1px 1px rgba(0,0,0,0.15);
          }

          .member-pile-count {
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 0.7rem;
            font-weight: 600;
            color: var(--text-secondary);
            background: var(--bg-hover);
            border: 2px solid var(--bg-card);
          }
        }
      }
    }

    /* Modal Styling */
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

    .modal-card {
      background: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-xl);
      width: 100%;
      max-width: 500px;
      box-shadow: var(--shadow-lg);
      overflow: hidden;
      animation: slideUp var(--transition-normal) var(--motion-easing) forwards;

      .modal-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 1.25rem 1.5rem;
        border-bottom: 1px solid var(--border-color);

        h2 {
          margin: 0;
          font-size: 1.25rem;
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

          .required {
            color: var(--danger-color);
          }

          input.invalid {
            border-color: var(--danger-color);
            &:focus {
              box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.15);
            }
          }

          .error-msg {
            font-size: 0.75rem;
            color: var(--danger-color);
            margin-top: 0.1rem;
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

    .alert {
      padding: 0.75rem 1rem;
      border-radius: var(--radius-md);
      font-size: 0.875rem;
      margin-bottom: 0.5rem;

      &.alert-danger {
        background: rgba(239, 68, 68, 0.1);
        border: 1px solid rgba(239, 68, 68, 0.2);
        color: var(--danger-color);
      }
    }

    @keyframes fadeIn {
      from { opacity: 0; }
      to { opacity: 1; }
    }

    @keyframes slideUp {
      from { transform: translateY(20px); opacity: 0; }
      to { transform: translateY(0); opacity: 1; }
    }
  `]
})
export class TeamsListComponent implements OnInit {
  private readonly teamService = inject(TeamService);
  private readonly userService = inject(UserService);
  private readonly auth = inject(AuthService);

  // States
  teams = signal<TeamResponse[]>([]);
  users = signal<UserDto[]>([]);
  loading = signal<boolean>(true);
  error = signal<string | null>(null);
  busy = signal<boolean>(false);

  // Search/Filters
  searchQuery = '';
  visibilityFilter = 'All';

  // Modal States
  showCreateModal = signal<boolean>(false);
  modalError = signal<string | null>(null);
  newTeam: CreateTeamRequest = {
    name: '',
    leadId: '',
    visibility: 'Public',
    description: '',
    avatarUrl: ''
  };

  // Cached User Map for fast lookup
  private userMap = new Map<string, UserDto>();

  // Filtered Teams List
  filteredTeams = computed(() => {
    const query = this.searchQuery.trim().toLowerCase();
    const vis = this.visibilityFilter;
    
    return this.teams().filter(t => {
      const matchesSearch = !query || 
        t.name.toLowerCase().includes(query) || 
        (t.description && t.description.toLowerCase().includes(query));
      
      const matchesVis = vis === 'All' || t.visibility === vis;
      
      return matchesSearch && matchesVis;
    });
  });

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading.set(true);
    this.error.set(null);

    // Fetch users first, then teams
    this.userService.getAllUsers().subscribe({
      next: (usersList) => {
        this.users.set(usersList);
        this.userMap.clear();
        usersList.forEach(u => this.userMap.set(u.userId, u));

        // Default new team lead to current user
        const currentUser = this.auth.currentUser();
        if (currentUser) {
          this.newTeam.leadId = currentUser.userId;
        } else if (usersList.length > 0) {
          this.newTeam.leadId = usersList[0].userId;
        }

        this.loadTeams();
      },
      error: (err) => {
        this.error.set(getApiErrorMessage(err, 'Failed to retrieve workspace members.'));
        this.loading.set(false);
      }
    });
  }

  loadTeams(): void {
    this.teamService.getTeams().subscribe({
      next: (teamsList) => {
        this.teams.set(teamsList);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(getApiErrorMessage(err, 'Failed to retrieve teams.'));
        this.loading.set(false);
      }
    });
  }

  openCreateModal(): void {
    const currentUser = this.auth.currentUser();
    this.newTeam = {
      name: '',
      leadId: currentUser?.userId || '',
      visibility: 'Public',
      description: '',
      avatarUrl: ''
    };
    this.modalError.set(null);
    this.showCreateModal.set(true);
  }

  closeCreateModal(): void {
    this.showCreateModal.set(false);
  }

  createTeam(event: Event): void {
    event.preventDefault();
    if (!this.newTeam.name.trim()) return;

    this.busy.set(true);
    this.modalError.set(null);

    this.teamService.createTeam(this.newTeam).subscribe({
      next: (created) => {
        this.teams.update(list => [...list, created]);
        this.busy.set(false);
        this.closeCreateModal();
      },
      error: (err) => {
        this.modalError.set(getApiErrorMessage(err, 'Failed to create team.'));
        this.busy.set(false);
      }
    });
  }

  // Helpers
  getUserName(userId: string): string {
    const user = this.userMap.get(userId);
    return user ? `${user.firstName} ${user.lastName}` : 'Unknown Member';
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

  getGradient(name: string): string {
    // Generate a beautiful consistent gradient based on the string name hash
    let hash = 0;
    for (let i = 0; i < name.length; i++) {
      hash = name.charCodeAt(i) + ((hash << 5) - hash);
    }
    const h1 = Math.abs(hash % 360);
    const h2 = (h1 + 40) % 360;
    return `linear-gradient(135deg, hsl(${h1}, 70%, 45%) 0%, hsl(${h2}, 75%, 35%) 100%)`;
  }
}
