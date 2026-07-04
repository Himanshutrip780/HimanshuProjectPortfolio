import { Routes } from '@angular/router';

import { AppShellComponent } from './core/layout/app-shell.component';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';
import { roleGuard } from './core/guards/role.guard';
import { subdomainGuard } from './core/guards/subdomain.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'projects' },
  {
    path: 'auth',
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'login' },
      {
        path: 'login',
        canActivate: [guestGuard],
        loadComponent: () =>
          import('./features/auth/pages/login/login.component').then(
            (m) => m.LoginComponent,
          ),
      },
      {
        path: 'register',
        canActivate: [guestGuard],
        loadComponent: () =>
          import('./features/auth/pages/register/register.component').then(
            (m) => m.RegisterComponent,
          ),
      },
    ],
  },
  {
    path: '',
    component: AppShellComponent,
    canActivate: [authGuard, subdomainGuard],
    children: [
      {
        path: 'projects',
        loadComponent: () =>
          import('./features/projects/pages/project-list/project-list.component').then(
            (m) => m.ProjectListComponent,
          ),
      },
      {
        path: 'projects/:projectId',
        loadComponent: () =>
          import('./features/projects/layout/project-shell.component').then(
            (m) => m.ProjectShellComponent,
          ),
        children: [
          { path: '', pathMatch: 'full', redirectTo: 'tasks' },
          {
            path: 'tasks',
            loadComponent: () =>
              import('./features/tasks/pages/task-list/task-list.component').then(
                (m) => m.TaskListComponent,
              ),
          },
          {
            path: 'board',
            loadComponent: () =>
              import('./features/tasks/pages/board/board.component').then(
                (m) => m.BoardComponent,
              ),
          },
          {
            path: 'backlog',
            loadComponent: () =>
              import('./features/tasks/pages/backlog/backlog.component').then(
                (m) => m.BacklogComponent,
              ),
          },
          {
            path: 'sprints',
            loadComponent: () =>
              import('./features/tasks/pages/sprints/sprints.component').then(
                (m) => m.SprintsComponent,
              ),
          },
          {
            path: 'planning',
            loadComponent: () =>
              import('./features/projects/pages/project-planning/project-planning.component').then(
                (m) => m.ProjectPlanningComponent,
              ),
          },
          {
            path: 'daily-updates',
            loadComponent: () =>
              import('./features/projects/pages/daily-updates/daily-updates.component').then(
                (m) => m.DailyUpdatesComponent,
              ),
          },
          {
            path: 'activity',
            loadComponent: () =>
              import('./features/projects/pages/project-activity/project-activity.component').then(
                (m) => m.ProjectActivityComponent,
              ),
          },
          {
            path: 'members',
            loadComponent: () =>
              import('./features/projects/pages/project-members/project-members.component').then(
                (m) => m.ProjectMembersComponent,
              ),
          },
          {
            path: 'analytics',
            loadComponent: () =>
              import('./features/analytics/pages/project-analytics/project-analytics.component').then(
                (m) => m.ProjectAnalyticsComponent,
              ),
          },
          {
            path: 'settings',
            loadComponent: () =>
              import('./features/projects/pages/project-settings/project-settings.component').then(
                (m) => m.ProjectSettingsComponent,
              ),
          },
        ],
      },
      {
        path: 'tasks',
        loadComponent: () =>
          import('./features/tasks/pages/global-tasks/global-tasks.component').then(
            (m) => m.GlobalTasksComponent,
          ),
      },
      {
        path: 'calendar',
        loadComponent: () =>
          import('./features/tasks/pages/calendar/calendar.component').then(
            (m) => m.CalendarComponent,
          ),
      },
      {
        path: 'timeline',
        loadComponent: () =>
          import('./features/tasks/pages/timeline/timeline.component').then(
            (m) => m.TimelineComponent,
          ),
      },
      {
        path: 'reports',
        loadComponent: () =>
          import('./features/analytics/pages/reports/reports.component').then(
            (m) => m.ReportsComponent,
          ),
      },
      {
        path: 'team',
        loadComponent: () =>
          import('./features/users/pages/team/team.component').then(
            (m) => m.TeamComponent,
          ),
      },
      {
        path: 'teams',
        loadComponent: () =>
          import('./features/teams/pages/teams-list/teams-list.component').then(
            (m) => m.TeamsListComponent,
          ),
      },
      {
        path: 'teams/:teamId',
        loadComponent: () =>
          import('./features/teams/pages/team-profile/team-profile.component').then(
            (m) => m.TeamProfileComponent,
          ),
      },
      {
        path: 'clients',
        loadComponent: () =>
          import('./features/projects/pages/clients/clients.component').then(
            (m) => m.ClientsComponent,
          ),
      },
      {
        path: 'tasks/:taskId',
        loadComponent: () =>
          import('./features/tasks/pages/task-detail/task-detail.component').then(
            (m) => m.TaskDetailComponent,
          ),
      },
      {
        path: 'profile',
        loadComponent: () =>
          import('./features/users/pages/profile/profile.component').then(
            (m) => m.ProfileComponent,
          ),
      },
      {
        path: 'notifications',
        loadComponent: () =>
          import('./features/notifications/pages/notification-list/notification-list.component').then(
            (m) => m.NotificationListComponent,
          ),
      },

      {
        path: 'admin/users',
        canActivate: [roleGuard],
        data: { roles: ['Admin'] },
        loadComponent: () =>
          import('./features/users/pages/admin-users/admin-users.component').then(
            (m) => m.AdminUsersComponent,
          ),
      },
    ],
  },
  { path: '**', redirectTo: 'auth/login' },
];

