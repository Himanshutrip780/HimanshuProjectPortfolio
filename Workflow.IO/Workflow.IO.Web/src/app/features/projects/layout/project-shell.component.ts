import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  ActivatedRoute,
  RouterLink,
  RouterLinkActive,
  RouterOutlet,
} from '@angular/router';

import {
  ProjectResponse,
  ProjectRole,
} from '../../../core/models/project.models';
import { getApiErrorMessage } from '../../../core/utils/api-error.util';
import { ProjectService } from '../services/project.service';

@Component({
  selector: 'app-project-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './project-shell.component.html',
  styles: `
    .project-shell-container {
      display: flex;
      height: 100%;
      width: 100%;
      margin: -2rem; /* Negate the parent layout padding */
    }

    /* Left Project sub-sidebar */
    .project-sidebar {
      width: 230px;
      min-width: 230px;
      background-color: var(--bg-body);
      border-right: 1px solid var(--border-color);
      display: flex;
      flex-direction: column;
      padding: 1.25rem 0.75rem;
      overflow-y: auto;
      transition: background-color var(--transition-normal), border-color var(--transition-normal);
    }

    .project-info {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
      margin-bottom: 1.5rem;
      padding-bottom: 1.25rem;
      border-bottom: 1px solid var(--border-color);
    }

    .back-link {
      display: flex;
      align-items: center;
      gap: 0.35rem;
      color: var(--text-secondary);
      text-decoration: none;
      font-size: 0.825rem;
      font-weight: 500;
      transition: color var(--transition-fast);
    }

    .back-link:hover {
      color: var(--primary-color);
    }

    .back-link span {
      font-size: 1.1rem;
    }

    .project-title-block {
      display: flex;
      align-items: center;
      gap: 0.65rem;
      margin-top: 0.25rem;
    }

    .project-avatar-icon {
      font-size: 2rem;
      color: var(--primary-color);
      background-color: var(--primary-glow);
      padding: 0.35rem;
      border-radius: var(--radius-md);
    }

    .project-meta-info {
      display: flex;
      flex-direction: column;
    }

    .project-name {
      font-size: 1rem;
      font-weight: 600;
      color: var(--text-primary);
      margin: 0;
      line-height: 1.2;
    }

    .project-key {
      font-size: 0.75rem;
      color: var(--text-muted);
      font-weight: 700;
      text-transform: uppercase;
      margin-top: 0.15rem;
    }

    .role-badge {
      font-size: 0.75rem;
      color: var(--primary-color);
      background-color: var(--primary-glow);
      padding: 0.25rem 0.6rem;
      border-radius: 9999px;
      font-weight: 600;
      align-self: flex-start;
    }

    /* Sub-sidebar Nav items */
    .project-nav {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
    }

    .proj-nav-item {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 0.55rem 0.75rem;
      color: var(--text-secondary);
      text-decoration: none;
      border-radius: var(--radius-md);
      font-size: 0.9rem;
      font-weight: 500;
      transition: all var(--transition-fast);
    }

    .proj-nav-item span.material-symbols-outlined {
      font-size: 1.25rem;
    }

    .proj-nav-item:hover {
      background-color: var(--bg-hover);
      color: var(--primary-color);
    }

    .proj-nav-item.active {
      background-color: var(--bg-panel);
      color: var(--primary-color);
      font-weight: 600;
      box-shadow: var(--shadow-sm);
    }

    /* Main Project Content View */
    .project-main-content {
      flex: 1;
      overflow-y: auto;
      padding: 2rem;
      background-color: var(--bg-body);
      transition: background-color var(--transition-normal);
    }

    /* Loading / Error States */
    .loading-container,
    .error-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      height: 60vh;
      width: 100%;
      gap: 1rem;
      color: var(--text-secondary);
    }

    .loading-spinner {
      font-size: 3rem;
      color: var(--primary-color);
      animation: spin 1.5s linear infinite;
    }

    .error-icon {
      font-size: 3.5rem;
      color: var(--danger-color);
    }

    .error {
      color: var(--danger-color);
      font-weight: 500;
      font-size: 1.1rem;
    }

    @keyframes spin {
      from { transform: rotate(0deg); }
      to { transform: rotate(360deg); }
    }
  `,
})
export class ProjectShellComponent implements OnInit {
  private readonly projectService = inject(ProjectService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly project = signal<ProjectResponse | null>(null);
  readonly myRole = signal<string | null>(null);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.route.paramMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => {
        const projectId = params.get('projectId');
        if (!projectId) {
          return;
        }

        this.projectService
          .getProjectById(projectId)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: (p) => this.project.set(p),
            error: (err) => this.error.set(getApiErrorMessage(err)),
          });

        this.projectService
          .getMyMembership(projectId)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: (m) =>
              this.myRole.set(ProjectRole[m.role] ?? String(m.role)),
            error: () => this.myRole.set(null),
          });
      });
  }
}
