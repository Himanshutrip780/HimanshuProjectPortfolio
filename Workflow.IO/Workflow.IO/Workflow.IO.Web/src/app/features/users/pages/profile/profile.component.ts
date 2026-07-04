import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';

import { AuthService } from '../../../../core/services/auth.service';
import { UserService } from '../../../../core/services/user.service';
import { ProjectService } from '../../../projects/services/project.service';
import { ActivityService } from '../../../../core/services/activity.service';
import { TeamService } from '../../../teams/services/team.service';
import { getApiErrorMessage } from '../../../../core/utils/api-error.util';
import { TeamResponse } from '../../../../core/models/team.models';
import { ActivityRecord } from '../../../../core/models/activity.models';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile.component.html',
  styles: `
    .page {
      width: 100%;
      margin: 0 auto;
      padding: 1.5rem;
    }

    h1 {
      font-size: 2rem;
      font-weight: 700;
      color: var(--text-primary);
      margin-bottom: 2rem;
    }

    .profile-grid {
      display: grid;
      grid-template-columns: 320px 1fr;
      gap: 2rem;
      align-items: start;
    }

    @media (max-width: 992px) {
      .profile-grid {
        grid-template-columns: 1fr;
      }
    }

    /* Left Sidebar */
    .sidebar-col {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    .card {
      background-color: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      padding: 1.5rem;
      box-shadow: var(--shadow-sm);
      transition: box-shadow var(--transition-fast);
    }

    .card:hover {
      box-shadow: var(--shadow-md);
    }

    .overview-card {
      text-align: center;
      display: flex;
      flex-direction: column;
      align-items: center;
    }

    .user-name {
      font-size: 1.25rem;
      font-weight: 700;
      color: var(--text-primary);
      margin: 0.75rem 0 0.25rem;
    }

    .user-email {
      font-size: 0.85rem;
      color: var(--text-muted);
      margin-bottom: 1rem;
    }

    .role-badge {
      display: inline-flex;
      align-items: center;
      padding: 0.25rem 0.75rem;
      border-radius: var(--radius-lg);
      font-size: 0.75rem;
      font-weight: 600;
      text-transform: uppercase;
      background-color: var(--primary-glow);
      color: var(--primary-color);
      margin-bottom: 1.5rem;
    }

    /* Gauge section */
    .gauge-section {
      width: 100%;
      border-top: 1px solid var(--border-color);
      padding-top: 1.25rem;
      margin-top: 0.5rem;
    }

    .gauge-label {
      display: flex;
      justify-content: space-between;
      font-size: 0.825rem;
      font-weight: 600;
      color: var(--text-secondary);
      margin-bottom: 0.5rem;
    }

    .gauge-track {
      width: 100%;
      height: 8px;
      background-color: var(--bg-hover);
      border-radius: var(--radius-sm);
      overflow: hidden;
    }

    .gauge-fill {
      height: 100%;
      background: var(--primary-gradient);
      border-radius: var(--radius-sm);
      transition: width var(--transition-slow);
    }

    /* Org Settings Helper */
    .org-card h3, .role-help-card h3 {
      font-size: 0.95rem;
      font-weight: 700;
      color: var(--text-primary);
      margin: 0 0 1rem;
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .org-card h3 .material-symbols-outlined,
    .role-help-card h3 .material-symbols-outlined {
      font-size: 1.25rem;
      color: var(--primary-color);
    }

    .info-list {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }

    .info-item {
      display: flex;
      justify-content: space-between;
      font-size: 0.85rem;
      border-bottom: 1px solid var(--border-color);
      padding-bottom: 0.5rem;
    }

    .info-item:last-child {
      border: none;
      padding-bottom: 0;
    }

    .info-label {
      color: var(--text-muted);
      font-weight: 500;
    }

    .info-value {
      color: var(--text-primary);
      font-weight: 600;
    }

    /* Role Help List */
    .privilege-item {
      display: flex;
      align-items: flex-start;
      gap: 0.5rem;
      font-size: 0.8rem;
      color: var(--text-secondary);
      line-height: 1.4;
      margin-bottom: 0.5rem;
    }

    .privilege-item:last-child {
      margin-bottom: 0;
    }

    .privilege-item .material-symbols-outlined {
      font-size: 1rem;
      color: var(--success-color);
      flex-shrink: 0;
      margin-top: 2px;
    }

    /* Right main column */
    .main-col {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    .section-title {
      font-size: 1.25rem;
      font-weight: 700;
      color: var(--text-primary);
      margin: 0 0 1.25rem;
      border-bottom: 2px solid var(--border-color);
      padding-bottom: 0.5rem;
    }

    /* Form layout */
    .form-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      gap: 1rem;
      margin-bottom: 0.5rem;
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
      margin-bottom: 1rem;
    }

    label {
      font-size: 0.825rem;
      font-weight: 600;
      color: var(--text-secondary);
    }

    input {
      padding: 0.65rem 0.85rem;
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      background-color: var(--bg-input);
      color: var(--text-primary);
      font-size: 0.9rem;
      transition: all var(--transition-fast);
    }

    input:focus {
      outline: none;
      border-color: var(--border-focus);
      box-shadow: 0 0 0 2px var(--primary-glow);
    }

    input[readonly] {
      background-color: var(--bg-hover);
      color: var(--text-muted);
      cursor: not-allowed;
    }

    .btn-submit {
      align-self: flex-start;
      padding: 0.65rem 1.25rem;
      background: var(--primary-gradient);
      color: var(--text-on-primary);
      border: none;
      border-radius: var(--radius-md);
      font-size: 0.9rem;
      font-weight: 600;
      cursor: pointer;
      transition: opacity var(--transition-fast);
      margin-top: 0.5rem;
    }

    .btn-submit:hover:not(:disabled) {
      opacity: 0.9;
    }

    .btn-submit:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .form-actions {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    /* Team Memberships */
    .teams-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
      gap: 1rem;
      margin-bottom: 1.5rem;
    }

    .team-item-card {
      display: flex;
      align-items: center;
      gap: 1rem;
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      padding: 0.85rem;
      background-color: var(--bg-panel);
    }

    .team-avatar {
      width: 2.5rem;
      height: 2.5rem;
      border-radius: var(--radius-md);
      background: var(--primary-gradient);
      color: var(--text-on-primary);
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 700;
      font-size: 1.1rem;
    }

    .team-details {
      display: flex;
      flex-direction: column;
      gap: 0.15rem;
    }

    .team-name {
      font-weight: 600;
      color: var(--text-primary);
      font-size: 0.9rem;
    }

    .team-meta {
      font-size: 0.75rem;
      color: var(--text-muted);
    }

    /* Contributions Timeline */
    .timeline {
      position: relative;
      padding-left: 1.5rem;
      border-left: 2px solid var(--border-color);
      margin-left: 0.5rem;
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    .timeline-item {
      position: relative;
    }

    .timeline-marker {
      position: absolute;
      left: -2rem;
      top: 0.25rem;
      width: 10px;
      height: 10px;
      border-radius: 50%;
      background-color: var(--primary-color);
      border: 2px solid var(--bg-panel);
    }

    .timeline-time {
      font-size: 0.75rem;
      color: var(--text-muted);
      font-weight: 500;
      margin-bottom: 0.25rem;
    }

    .timeline-desc {
      font-size: 0.875rem;
      color: var(--text-primary);
      line-height: 1.4;
      margin: 0;
    }

    .error {
      color: var(--danger-color);
      font-weight: 500;
    }

    .success {
      color: var(--success-color);
      font-weight: 500;
    }

    .avatar-upload-section {
      display: flex;
      flex-direction: column;
      align-items: center;
      margin-bottom: 1.5rem;
      gap: 0.5rem;
    }

    .avatar-container {
      width: 140px;
      height: 140px;
      border-radius: 50%;
      border: 3px dashed var(--border-color);
      display: flex;
      align-items: center;
      justify-content: center;
      cursor: pointer;
      position: relative;
      overflow: hidden;
      background-color: var(--bg-hover);
      transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    }

    .avatar-container:hover {
      border-color: var(--primary-color);
      transform: scale(1.02);
      box-shadow: 0 10px 15px -3px var(--primary-glow);
    }

    .avatar-container.has-avatar {
      border-style: solid;
      border-color: var(--border-color);
    }

    .uploaded-avatar {
      width: 100%;
      height: 100%;
      object-fit: cover;
      border-radius: 50%;
    }

    .avatar-overlay {
      position: absolute;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      background-color: rgba(15, 23, 42, 0.65);
      color: #fff;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      opacity: 0;
      transition: opacity 0.3s ease;
      backdrop-filter: blur(2px);
    }

    .avatar-container:hover .avatar-overlay {
      opacity: 1;
    }

    .overlay-text {
      font-size: 0.85rem;
      font-weight: 500;
      margin-top: 0.25rem;
    }

    .file-hint {
      font-size: 0.75rem;
      color: var(--text-muted);
      margin: 0;
      font-weight: 500;
    }

    .remove-avatar-btn {
      display: inline-flex;
      align-items: center;
      gap: 0.25rem;
      background: none;
      border: 1px solid var(--border-color);
      color: var(--danger-color);
      padding: 0.35rem 0.75rem;
      border-radius: var(--radius-md);
      font-size: 0.8rem;
      font-weight: 600;
      cursor: pointer;
      margin-top: 0.5rem;
      transition: all 0.2s ease;
    }

    .remove-avatar-btn:hover {
      background-color: rgba(239, 68, 68, 0.05);
      border-color: rgba(239, 68, 68, 0.2);
    }

    .avatar-initials-container {
      width: 100%;
      height: 100%;
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--primary-gradient);
      color: var(--text-on-primary);
    }

    .avatar-initials {
      font-size: 2.75rem;
      font-weight: 700;
      letter-spacing: 0.05em;
    }
  `,
})
export class ProfileComponent implements OnInit {
  private readonly userService = inject(UserService);
  protected readonly auth = inject(AuthService);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly projectService = inject(ProjectService);
  private readonly activityService = inject(ActivityService);
  private readonly teamService = inject(TeamService);

  readonly loading = signal(true);
  readonly savingProfile = signal(false);
  readonly savingPassword = signal(false);
  readonly profileSaved = signal(false);
  readonly passwordSaved = signal(false);
  readonly error = signal<string | null>(null);

  // Teams & Contributions State
  readonly myTeams = signal<TeamResponse[]>([]);
  readonly contributions = signal<ActivityRecord[]>([]);
  readonly loadingContributions = signal(true);

  // Profile Completion Gauge Calculation
  readonly profileCompletion = computed(() => {
    const user = this.auth.currentUser();
    const teams = this.myTeams();
    if (!user) return 0;
    
    let score = 0;
    if (user.firstName && user.firstName.trim().length > 0) score += 25;
    if (user.lastName && user.lastName.trim().length > 0) score += 25;
    if (this.auth.userAvatar()) score += 25;
    if (teams.length > 0) score += 25;
    
    return score;
  });

  userInitials(user: any): string {
    if (user?.firstName && user?.lastName) {
      return `${user.firstName.charAt(0)}${user.lastName.charAt(0)}`.toUpperCase();
    } else if (user?.firstName) {
      return user.firstName.charAt(0).toUpperCase();
    }
    return user?.email ? user.email.substring(0, 2).toUpperCase() : 'US';
  }

  readonly profileForm = this.fb.group({
    email: [{ value: '', disabled: true }],
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
  });

  readonly passwordForm = this.fb.group({
    currentPassword: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', Validators.required],
  });

  ngOnInit(): void {
    this.userService.getMe().subscribe({
      next: (user) => {
        this.profileForm.patchValue({
          email: user.email,
          firstName: user.firstName,
          lastName: user.lastName,
        });
        this.loading.set(false);
        this.loadDashboardData();
      },
      error: (err) => {
        this.error.set(getApiErrorMessage(err));
        this.loading.set(false);
      },
    });
  }

  private loadDashboardData(): void {
    const userId = this.auth.currentUser()?.userId;
    if (!userId) return;

    // Load Teams user belongs to
    this.teamService.getTeams().subscribe({
      next: (teams) => {
        const userTeams = teams.filter((t) =>
          t.members.some((m) => m.userId === userId)
        );
        this.myTeams.set(userTeams.length > 0 ? userTeams : teams);
      },
      error: () => this.myTeams.set([]),
    });

    // Fetch and merge activities for active projects
    this.loadingContributions.set(true);
    this.projectService
      .getMyProjects()
      .pipe(
        switchMap((projects) => {
          if (!projects || projects.length === 0) {
            return of([]);
          }

          const activityRequests = projects.map((p) =>
            this.activityService.getProjectActivities(p.projectId, 50).pipe(
              catchError(() => of([] as ActivityRecord[]))
            )
          );

          return forkJoin(activityRequests).pipe(
            map((activitiesArrays) => {
              const allActivities = activitiesArrays.flat();
              const myActivities = allActivities.filter(
                (a) => a.actorId === userId
              );
              // Deduplicate by activityRecordId
              const uniqueActivities = Array.from(
                new Map(myActivities.map((a) => [a.activityRecordId, a])).values()
              );
              // Sort by date descending
              return uniqueActivities
                .sort(
                  (a, b) =>
                    new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
                )
                .slice(0, 10);
            })
          );
        })
      )
      .subscribe({
        next: (activities) => {
          this.contributions.set(activities);
          this.loadingContributions.set(false);
        },
        error: () => {
          this.contributions.set([]);
          this.loadingContributions.set(false);
        },
      });
  }

  saveProfile(): void {
    if (this.profileForm.invalid) {
      return;
    }

    const raw = this.profileForm.getRawValue();
    this.savingProfile.set(true);
    this.profileSaved.set(false);
    this.error.set(null);

    this.userService
      .updateMe({
        firstName: raw.firstName,
        lastName: raw.lastName,
        avatarUrl: this.auth.userAvatar(),
      })
      .subscribe({
        next: () => {
          this.profileSaved.set(true);
          this.savingProfile.set(false);
          this.auth.syncProfile().subscribe();
        },
        error: (err) => {
          this.error.set(getApiErrorMessage(err));
          this.savingProfile.set(false);
        },
      });
  }

  changePassword(): void {
    if (this.passwordForm.invalid) {
      return;
    }

    const raw = this.passwordForm.getRawValue();
    if (raw.newPassword !== raw.confirmPassword) {
      this.error.set('New passwords do not match.');
      return;
    }

    this.savingPassword.set(true);
    this.passwordSaved.set(false);
    this.error.set(null);

    this.userService
      .changePassword({
        currentPassword: raw.currentPassword,
        newPassword: raw.newPassword,
      })
      .subscribe({
        next: () => {
          this.passwordSaved.set(true);
          this.savingPassword.set(false);
          this.passwordForm.reset();
        },
        error: (err) => {
          this.error.set(getApiErrorMessage(err));
          this.savingPassword.set(false);
        },
      });
  }

  removeAvatar(): void {
    this.auth.updateUserAvatar(null);
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) {
      return;
    }

    const file = input.files[0];
    const allowedTypes = ['image/png', 'image/jpeg', 'image/jpg'];
    const isFormatValid = allowedTypes.includes(file.type);
    const isSizeValid = file.size <= 2 * 1024 * 1024;

    if (!isFormatValid || !isSizeValid) {
      alert('File size exceeds 2MB limit or invalid format (PNG/JPG only).');
      input.value = '';
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      const base64Data = reader.result as string;
      this.auth.updateUserAvatar(base64Data);
    };
    reader.readAsDataURL(file);
  }
}
