import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardStore } from './dashboard.store';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { JobService, JobDto } from '../../core/services/job.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  public store = inject(DashboardStore);
  private router = inject(Router);
  public authService = inject(AuthService);
  private jobService = inject(JobService);

  public canViewOffers = computed(() => {
    const role = this.authService.currentUser()?.role;
    return role === 'SuperAdmin' || role === 'Recruiter';
  });

  public canViewCandidates = computed(() => {
    const role = this.authService.currentUser()?.role;
    return role === 'SuperAdmin' || role === 'Recruiter' || role === 'HiringManager';
  });

  public canViewPipeline = computed(() => {
    const role = this.authService.currentUser()?.role;
    return role === 'SuperAdmin' || role === 'Recruiter' || role === 'HiringManager';
  });

  // Live data signals
  public jobs = signal<JobDto[]>([]);
  public departments = signal<any[]>([]);
  public users = signal<any[]>([]);

  // Search & Sorting state for Requisitions Table
  public searchQuery = signal<string>('');
  public sortBy = signal<string>('title');
  public sortOrder = signal<'asc' | 'desc'>('asc');

  // Create Job Modal State
  public isCreateJobModalOpen = signal<boolean>(false);
  public newJobTitle = signal<string>('');
  public newJobDepartment = signal<string>('Engineering');
  public newJobManager = signal<string>('');

  // Workspace lists
  public departmentsList: string[] = [];
  public managersList: string[] = [];

  public ngOnInit() {
    this.store.loadAll();
    this.loadJobs();
    
    this.jobService.getDepartments().subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.departments.set(res.data);
          this.departmentsList = res.data.map((d: any) => d.name);
          if (this.departmentsList.length > 0) {
            this.newJobDepartment.set(this.departmentsList[0]);
          }
        }
      }
    });

    this.jobService.getUsers('HiringManager').subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.users.set(res.data);
          const names = res.data.map((u: any) => `${u.firstName} ${u.lastName}`);
          this.managersList = names;
          
          const user = this.authService.currentUser();
          if (user) {
            const name = `${user.firstName} ${user.lastName}`;
            this.newJobManager.set(name);
            if (!this.managersList.includes(name)) {
              this.managersList.unshift(name);
            }
          }
        }
      }
    });
  }

  public loadJobs() {
    this.jobService.getJobs().subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.jobs.set(res.data.items || []);
        }
      }
    });
  }

  // Modal Action Handlers
  public openCreateJobModal() {
    this.newJobTitle.set('');
    if (this.departmentsList.length > 0) {
      this.newJobDepartment.set(this.departmentsList[0]);
    } else {
      this.newJobDepartment.set('Engineering');
    }
    const user = this.authService.currentUser();
    this.newJobManager.set(user ? `${user.firstName} ${user.lastName}` : 'Himanshu Tripathi');
    this.isCreateJobModalOpen.set(true);
  }

  public closeCreateJobModal() {
    this.isCreateJobModalOpen.set(false);
  }

  // Premium Toast signal
  public toast = signal<{ message: string; type: 'success' | 'error' } | null>(null);

  public showToast(message: string, type: 'success' | 'error' = 'success') {
    this.toast.set({ message, type });
    setTimeout(() => {
      this.toast.set(null);
    }, 4000);
  }

  public submitCreateJob() {
    if (this.departmentsList.length === 0 || this.managersList.length === 0) {
      this.showToast('Cannot submit job requisition. Departments or Hiring Managers lists are empty in the database.', 'error');
      return;
    }
    const title = this.newJobTitle().trim();
    if (!title) return;

    const dept = this.departments().find(d => d.name === this.newJobDepartment());
    const mgr = this.users().find(u => `${u.firstName} ${u.lastName}` === this.newJobManager());

    const command = {
      title: title,
      description: `Position description for ${title}.`,
      responsibilities: `Core responsibilities for ${title}.`,
      qualifications: `Qualifications required for ${title}.`,
      departmentId: dept ? dept.id : this.departments()[0]?.id,
      hiringManagerId: mgr ? mgr.id : this.authService.currentUser()?.id,
      location: 'San Francisco, CA',
      employmentType: 0,
      salaryMin: 90000,
      salaryMax: 140000,
      skills: []
    };

    this.jobService.createJob(command).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.loadJobs();
          this.store.loadAll();
        }
      }
    });
    this.closeCreateJobModal();
  }

  // Navigation Click Handlers
  public onViewPipelineClick() {
    this.router.navigate(['/pipeline']);
  }

  public onJoinMeeting(link?: string) {
    if (link) {
      window.open(link, '_blank');
    }
  }

  public onViewCandidate(id: string) {
    this.router.navigate(['/candidates'], { queryParams: { id } });
  }

  public onNavigateToInterviews() {
    this.router.navigate(['/interviews']);
  }

  public onNavigateToOffers() {
    this.router.navigate(['/offers']);
  }

  public onNavigateToCandidates() {
    this.router.navigate(['/candidates']);
  }

  public toggleSort(field: string) {
    if (this.sortBy() === field) {
      this.sortOrder.update(order => order === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortBy.set(field);
      this.sortOrder.set('asc');
    }
  }

  // Table Data filter methods
  public getFilteredJobs() {
    const original = this.jobs() || [];
    let list = original.map((j) => {
      const statusStr = j.status === 1 ? 'Published' : (j.status === 0 ? 'Draft' : 'Closed');
      return {
        id: j.id,
        title: j.title,
        candidatesCount: j.applicantCount || 0,
        department: j.departmentName || 'Engineering',
        stage: statusStr,
        manager: j.hiringManagerName || 'Unassigned',
        status: statusStr
      };
    });

    // Apply Search Query
    const query = this.searchQuery().toLowerCase().trim();
    if (query) {
      list = list.filter(job => 
        job.title.toLowerCase().includes(query) || 
        job.manager.toLowerCase().includes(query) ||
        job.department.toLowerCase().includes(query) ||
        job.stage.toLowerCase().includes(query)
      );
    }

    // Apply Sorting
    const field = this.sortBy();
    const order = this.sortOrder() === 'asc' ? 1 : -1;
    list.sort((a: any, b: any) => {
      let valA = a[field];
      let valB = b[field];
      if (typeof valA === 'string') {
        return valA.localeCompare(valB) * order;
      }
      if (typeof valA === 'number') {
        return (valA - valB) * order;
      }
      return 0;
    });

    return list;
  }

  public getBarHeight(item: any): number {
    const maxVal = Math.max(...this.store.monthlyTrends().map((t: any) => t.applicationsCount), 1);
    const scale = 130 / maxVal;
    return Math.max(5, Math.round(item.applicationsCount * scale));
  }

  public getBarValue(item: any): number {
    return item.applicationsCount;
  }

  public getYLabel(index: number): number {
    const maxVal = Math.max(...this.store.monthlyTrends().map((t: any) => t.applicationsCount), 1);
    return Math.round((maxVal / 3) * (3 - index));
  }
}
