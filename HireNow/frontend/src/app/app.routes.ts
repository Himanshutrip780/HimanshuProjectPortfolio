import { Routes } from '@angular/router';
import { AuthComponent } from './features/auth/auth.component';
import { LayoutComponent } from './features/layout/layout.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { JobsComponent } from './features/jobs/jobs.component';
import { CandidatesComponent } from './features/candidates/candidates.component';
import { PipelineComponent } from './features/pipeline/pipeline.component';
import { InterviewsComponent } from './features/interviews/interviews.component';
import { OffersComponent } from './features/offers/offers.component';
import { AnalyticsComponent } from './features/analytics/analytics.component';
import { ReportsComponent } from './features/reports/reports.component';
import { SettingsComponent } from './features/settings/settings.component';
import { CareersComponent } from './features/careers/careers.component';
import { PortalComponent } from './features/portal/portal.component';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  { path: 'auth/login', component: AuthComponent },
  { path: 'careers', component: CareersComponent },
  { path: 'portal', component: PortalComponent },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { 
        path: 'dashboard', 
        component: DashboardComponent 
      },
      { 
        path: 'jobs', 
        component: JobsComponent, 
        canActivate: [roleGuard], 
        data: { expectedRoles: ['SuperAdmin', 'Recruiter', 'HiringManager'] } 
      },
      { 
        path: 'candidates', 
        component: CandidatesComponent, 
        canActivate: [roleGuard], 
        data: { expectedRoles: ['SuperAdmin', 'Recruiter', 'HiringManager'] } 
      },
      { 
        path: 'pipeline', 
        component: PipelineComponent, 
        canActivate: [roleGuard], 
        data: { expectedRoles: ['SuperAdmin', 'Recruiter', 'HiringManager'] } 
      },
      { 
        path: 'interviews', 
        component: InterviewsComponent, 
        canActivate: [roleGuard], 
        data: { expectedRoles: ['SuperAdmin', 'Recruiter', 'HiringManager', 'Interviewer'] } 
      },
      { 
        path: 'offers', 
        component: OffersComponent, 
        canActivate: [roleGuard], 
        data: { expectedRoles: ['SuperAdmin', 'Recruiter'] } 
      },
      { 
        path: 'analytics', 
        component: AnalyticsComponent, 
        canActivate: [roleGuard], 
        data: { expectedRoles: ['SuperAdmin', 'Recruiter'] } 
      },
      { 
        path: 'reports', 
        component: ReportsComponent, 
        canActivate: [roleGuard], 
        data: { expectedRoles: ['SuperAdmin', 'Recruiter'] } 
      },
      { 
        path: 'settings', 
        component: SettingsComponent, 
        canActivate: [roleGuard], 
        data: { expectedRoles: ['SuperAdmin'] } 
      }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
