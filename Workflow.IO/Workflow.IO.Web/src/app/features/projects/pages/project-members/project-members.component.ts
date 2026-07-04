import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { catchError, forkJoin, map, of } from 'rxjs';

import { ProjectMember, ProjectRole } from '../../../../core/models/project.models';
import { UserLookup } from '../../../../core/models/user.models';
import { UserService } from '../../../../core/services/user.service';
import { getApiErrorMessage } from '../../../../core/utils/api-error.util';
import { UserPickerComponent } from '../../../../shared/components/user-picker/user-picker.component';
import { ProjectService } from '../../services/project.service';

interface MemberRow extends ProjectMember {
  label: string;
}

@Component({
  selector: 'app-project-members',
  standalone: true,
  imports: [ReactiveFormsModule, UserPickerComponent],
  template: `
    <div class="members-container">
      <header class="page-header">
        <div class="header-main">
          <span class="material-symbols-outlined header-icon">groups</span>
          <div>
            <h1>Project Members</h1>
            <p class="subtitle">Manage user access roles and invite new members to collaborate on this project.</p>
          </div>
        </div>
      </header>

      @if (error()) {
        <div class="error-banner">
          <span class="material-symbols-outlined">error</span>
          <span>{{ error() }}</span>
        </div>
      }

      <div class="members-layout">
        <!-- Members List -->
        <div class="members-list-panel">
          @if (loading()) {
            <div class="state-container">
              <div class="spinner"></div>
              <p>Loading members...</p>
            </div>
          } @else if (memberRows().length === 0) {
            <div class="state-container">
              <span class="material-symbols-outlined" style="font-size: 2.5rem; color: var(--text-muted);">person_off</span>
              <p>No members found in this project.</p>
            </div>
          } @else {
            @for (m of memberRows(); track m.projectMemberId) {
              <div class="member-item">
                <div class="member-info">
                  <div class="member-avatar" [style.background-color]="getAvatarColor(m.label)">
                    {{ getUserInitials(m.label) }}
                  </div>
                  <div class="member-details">
                    <span class="member-name">{{ m.label }}</span>
                    <span class="member-email">ID: {{ m.userId }}</span>
                  </div>
                </div>
                <div class="member-actions">
                  <select
                    class="role-select"
                    [value]="m.role"
                    (change)="changeRole(m.userId, $event)"
                  >
                    <option [value]="ProjectRole.Owner">Owner</option>
                    <option [value]="ProjectRole.Admin">Admin</option>
                    <option [value]="ProjectRole.Member">Member</option>
                    <option [value]="ProjectRole.Viewer">Viewer</option>
                  </select>
                  <button 
                    type="button" 
                    class="btn-remove" 
                    (click)="remove(m.userId)"
                    title="Remove member"
                  >
                    <span class="material-symbols-outlined" style="font-size: 1.2rem;">delete</span>
                  </button>
                </div>
              </div>
            }
          }
        </div>

        <!-- Add Member Form -->
        <section class="panel add-member-panel">
          <h3>Invite Collaborator</h3>
          <p class="subtitle" style="font-size: 0.8rem; margin-bottom: 0.75rem;">Search for a user by email to add them to this project's workspace.</p>
          
          <div class="add-form-group">
            <app-user-picker
              label="Collaborator email"
              (userSelected)="onUserPicked($event)"
            />

            <form [formGroup]="addForm" (ngSubmit)="addMember()" style="display: flex; flex-direction: column; gap: 0.75rem; width: 100%;">
              <div style="display: flex; flex-direction: column; gap: 0.25rem;">
                <label style="font-size: 0.8rem; font-weight: 600; color: var(--text-secondary);">Assign Role</label>
                <select formControlName="role">
                  <option [value]="ProjectRole.Member">Member</option>
                  <option [value]="ProjectRole.Admin">Admin</option>
                  <option [value]="ProjectRole.Viewer">Viewer</option>
                </select>
              </div>

              <div class="add-actions">
                <button type="submit" [disabled]="!selectedUser()">
                  <span class="material-symbols-outlined" style="font-size: 1.1rem;">person_add</span> Add member
                </button>
              </div>
            </form>
          </div>
        </section>
      </div>
    </div>
  `,
  styles: `
    .members-container {
      padding: 2rem;
      max-width: 900px;
      margin: 0 auto;
      display: flex;
      flex-direction: column;
      gap: 2rem;
    }

    .page-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
    }

    .header-main {
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .header-icon {
      font-size: 2.5rem;
      color: var(--primary-color);
      background: var(--primary-glow);
      padding: 0.5rem;
      border-radius: var(--radius-lg);
    }

    .subtitle {
      color: var(--text-secondary);
      font-size: 0.95rem;
      margin-top: 0.25rem;
    }

    .members-layout {
      display: grid;
      grid-template-columns: 1fr;
      gap: 2rem;
    }

    @media (min-width: 768px) {
      .members-layout {
        grid-template-columns: 1.2fr 0.8fr;
        align-items: start;
      }
    }

    .members-list-panel {
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }

    .member-item {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 1rem;
      background: var(--bg-panel);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      gap: 1rem;
      box-shadow: var(--shadow-sm);
      transition: border-color var(--transition-fast), box-shadow var(--transition-fast);
    }

    .member-item:hover {
      border-color: var(--primary-color);
      box-shadow: var(--shadow-md);
    }

    .member-info {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      min-width: 0;
    }

    .member-avatar {
      width: 2.5rem;
      height: 2.5rem;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 0.95rem;
      font-weight: 700;
      color: #ffffff;
      flex-shrink: 0;
    }

    .member-details {
      display: flex;
      flex-direction: column;
      min-width: 0;
    }

    .member-name {
      font-size: 0.95rem;
      font-weight: 600;
      color: var(--text-primary);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .member-email {
      font-size: 0.8rem;
      color: var(--text-muted);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .member-actions {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      flex-shrink: 0;
    }

    .role-select {
      width: auto;
      min-width: 100px;
      padding: 0.35rem 0.6rem;
      font-size: 0.85rem;
    }

    .btn-remove {
      background: transparent;
      border: 1px solid transparent;
      color: var(--text-muted);
      padding: 0.35rem;
      border-radius: var(--radius-md);
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: color var(--transition-fast), background var(--transition-fast);
    }

    .btn-remove:hover {
      color: var(--danger-color);
      background: rgba(239, 68, 68, 0.08);
      border-color: rgba(239, 68, 68, 0.1);
    }

    .add-member-panel {
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }

    .add-member-panel h3 {
      font-size: 1.15rem;
      font-weight: 600;
      margin: 0;
      color: var(--text-primary);
    }

    .add-form-group {
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }

    .add-actions {
      display: flex;
      gap: 0.5rem;
      margin-top: 0.5rem;
    }

    .add-actions button {
      flex: 1;
    }

    .error-banner {
      background-color: rgba(239, 68, 68, 0.1);
      color: var(--danger-color);
      border: 1px solid rgba(239, 68, 68, 0.2);
      border-radius: var(--radius-md);
      padding: 0.75rem 1rem;
      font-size: 0.875rem;
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .state-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 4rem 2rem;
      background: var(--bg-panel);
      border: 1px dashed var(--border-color);
      border-radius: var(--radius-lg);
      text-align: center;
      color: var(--text-secondary);
      gap: 1rem;
      width: 100%;
    }

    .spinner {
      width: 36px;
      height: 36px;
      border: 3px solid var(--border-color);
      border-top-color: var(--primary-color);
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }
  `,
})
export class ProjectMembersComponent implements OnInit {
  readonly ProjectRole = ProjectRole;
  private readonly projectService = inject(ProjectService);
  private readonly userService = inject(UserService);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly memberRows = signal<MemberRow[]>([]);
  readonly selectedUser = signal<UserLookup | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  readonly addForm = this.fb.group({
    role: [ProjectRole.Member, Validators.required],
  });

  ngOnInit(): void {
    this.loadMembers();
  }

  getUserInitials(name: string): string {
    if (!name) return '?';
    const parts = name.trim().split(/\s+/);
    return parts.map(p => p.charAt(0)).join('').toUpperCase().slice(0, 2);
  }

  getAvatarColor(name: string): string {
    if (!name) return '#6366f1';
    let hash = 0;
    for (let i = 0; i < name.length; i++) {
      hash = name.charCodeAt(i) + ((hash << 5) - hash);
    }
    const colors = ['#6366f1', '#10b981', '#f59e0b', '#3b82f6', '#8b5cf6', '#ec4899', '#f43f5e', '#06b6d4'];
    return colors[Math.abs(hash) % colors.length];
  }

  onUserPicked(user: UserLookup | null): void {
    this.selectedUser.set(user);
  }

  addMember(): void {
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    const user = this.selectedUser();
    if (!projectId || !user) {
      return;
    }

    const raw = this.addForm.getRawValue();
    this.projectService
      .addMember(projectId, {
        userId: user.userId,
        role: Number(raw.role),
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.selectedUser.set(null);
          this.addForm.reset({ role: ProjectRole.Member });
          this.loadMembers();
        },
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  changeRole(userId: string, event: Event): void {
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    if (!projectId) {
      return;
    }

    const role = Number((event.target as HTMLSelectElement).value);
    this.projectService
      .changeMemberRole(projectId, userId, { role })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.loadMembers(),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  remove(userId: string): void {
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    if (!projectId) {
      return;
    }

    this.projectService
      .removeMember(projectId, userId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.loadMembers(),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  private loadMembers(): void {
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    if (!projectId) {
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.projectService
      .getMembers(projectId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (members) => {
          if (members.length === 0) {
            this.memberRows.set([]);
            this.loading.set(false);
            return;
          }

          forkJoin(
            members.map((member) =>
              this.userService.getUserById(member.userId).pipe(
                map(
                  (profile) =>
                    ({
                      ...member,
                      label: `${profile.firstName} ${profile.lastName}`,
                    }) satisfies MemberRow,
                ),
                catchError(
                  () =>
                    of({
                      ...member,
                      label: member.userId,
                    } satisfies MemberRow),
                ),
              ),
            ),
          )
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
              next: (rows) => {
                this.memberRows.set(rows);
                this.loading.set(false);
              },
              error: (err) => {
                this.error.set(getApiErrorMessage(err));
                this.loading.set(false);
              },
            });
        },
        error: (err) => {
          this.error.set(getApiErrorMessage(err));
          this.loading.set(false);
        },
      });
  }
}
