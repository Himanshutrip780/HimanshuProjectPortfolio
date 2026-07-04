import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { JobService, JobDto } from '../../core/services/job.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-jobs',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule
  ],
  templateUrl: './jobs.component.html',
  styleUrls: ['./jobs.component.scss']
})
export class JobsComponent implements OnInit {
  private jobService = inject(JobService);
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);

  public canManageJobs = computed(() => {
    const role = this.authService.currentUser()?.role;
    return role === 'SuperAdmin' || role === 'Recruiter';
  });

  public jobs = signal<JobDto[]>([]);
  public loading = signal<boolean>(true);
  public showCreateForm = signal<boolean>(false);
  public submitting = signal<boolean>(false);
  
  public jobForm!: FormGroup;

  // Search, Filters & Sorting state
  public searchQuery = signal<string>('');
  public selectedDeptFilter = signal<string>('');
  public selectedStatusFilter = signal<string>('');
  public sortBy = signal<string>('title');
  public sortOrder = signal<'asc' | 'desc'>('asc');
  public pageIndex = signal<number>(1);
  public pageSize = signal<number>(30);
  
  // Selection and Bulk Actions state
  public selectedJobIds = signal<Set<string>>(new Set());

  public departments: any[] = [];

  public jobStatuses = [
    { value: 0, label: 'Draft' },
    { value: 1, label: 'Approval' },
    { value: 2, label: 'Published' },
    { value: 3, label: 'Closed' },
    { value: 4, label: 'Archived' }
  ];

  public jobTypes = [
    { value: 0, label: 'FullTime' },
    { value: 1, label: 'PartTime' },
    { value: 2, label: 'Contract' },
    { value: 3, label: 'Internship' }
  ];

  public currencies: { value: string, label: string }[] = [];

  constructor() {
    this.initForm();
  }

  public ngOnInit() {
    this.loadJobs();
    this.loadDepartments();
    this.loadCurrencies();
  }

  private initForm() {
    this.jobForm = this.fb.group({
      title: ['', [Validators.required]],
      description: ['', [Validators.required]],
      responsibilities: ['', [Validators.required]],
      qualifications: ['', [Validators.required]],
      departmentId: ['', [Validators.required]],
      location: ['', [Validators.required]],
      type: [0, [Validators.required]], // FullTime
      salaryMin: [null, [Validators.required]],
      salaryMax: [null, [Validators.required]],
      currency: ['USD', [Validators.required]]
    });
  }

  public loadCurrencies() {
    this.jobService.getCurrencies().subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.currencies = res.data;
        }
      }
    });
  }

  public loadDepartments() {
    this.jobService.getDepartments().subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.departments = res.data;
          if (this.departments.length > 0) {
            this.jobForm.patchValue({
              departmentId: this.departments[0].id
            });
          }
        }
      }
    });
  }

  public loadJobs() {
    this.loading.set(true);
    const statusVal = this.selectedStatusFilter() !== '' ? Number(this.selectedStatusFilter()) : undefined;
    const deptVal = this.selectedDeptFilter() !== '' ? this.selectedDeptFilter() : undefined;
    const searchVal = this.searchQuery() || undefined;

    this.jobService.getJobs({
      pageIndex: this.pageIndex(),
      pageSize: this.pageSize(),
      departmentId: deptVal,
      status: statusVal,
      searchTerm: searchVal
    }).subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.jobs.set(res.data.items || []);
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  public onSearchChanged(val: string) {
    this.searchQuery.set(val);
    this.pageIndex.set(1);
    this.loadJobs();
  }

  public onDeptFilterChange(val: string) {
    this.selectedDeptFilter.set(val);
    this.pageIndex.set(1);
    this.loadJobs();
  }

  public onStatusFilterChange(val: string) {
    this.selectedStatusFilter.set(val);
    this.pageIndex.set(1);
    this.loadJobs();
  }

  public prevPage() {
    if (this.pageIndex() > 1) {
      this.pageIndex.update(idx => idx - 1);
      this.loadJobs();
    }
  }

  public nextPage() {
    if (this.jobs().length === 30) {
      this.pageIndex.update(idx => idx + 1);
      this.loadJobs();
    }
  }

  public toggleCreateForm() {
    this.showCreateForm.update(val => !val);
    if (this.showCreateForm()) {
      this.jobForm.reset({
        title: '',
        description: '',
        responsibilities: '',
        qualifications: '',
        departmentId: this.departments[0]?.id || '',
        location: '',
        type: 0,
        salaryMin: null,
        salaryMax: null,
        currency: 'USD'
      });
    }
  }

  public onSubmit() {
    if (this.jobForm.invalid) return;
    this.submitting.set(true);

    this.jobService.createJob(this.jobForm.value).subscribe({
      next: (res) => {
        this.submitting.set(false);
        if (res.isSuccess) {
          this.loadJobs();
          this.showCreateForm.set(false);
        }
      },
      error: () => this.submitting.set(false)
    });
  }

  public getStatusLabel(status: number): string {
    return this.jobStatuses.find(s => s.value === status)?.label || 'Unknown';
  }

  public getStatusClass(status: number): string {
    switch(status) {
      case 0: return 'text-slate-600 dark:text-slate-400 bg-secondary/80 border-border';
      case 1: return 'text-amber-700 dark:text-amber-400 bg-amber-500/10 border-amber-500/20';
      case 2: return 'text-emerald-700 dark:text-emerald-400 bg-emerald-500/10 border-emerald-500/20';
      case 3: return 'text-red-700 dark:text-red-400 bg-red-500/10 border-red-500/20';
      case 4: return 'text-slate-500 dark:text-slate-500 bg-slate-500/10 border-slate-500/20';
      default: return '';
    }
  }

  public getJobTypeLabel(type: number): string {
    return this.jobTypes.find(t => t.value === type)?.label || 'FullTime';
  }

  public changeJobStatus(jobId: string, status: number) {
    this.jobService.updateJobStatus(jobId, status).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.jobs.update(items => 
            items.map(item => item.id === jobId ? { ...item, status } : item)
          );
        }
      }
    });
  }

  // Advanced Table Filter & Sorting Methods
  public getFilteredJobs(): JobDto[] {
    let list = this.jobs() || [];

    // Sorting logic
    if (list.length > 0) {
      const field = this.sortBy();
      const order = this.sortOrder() === 'asc' ? 1 : -1;
      
      list = [...list].sort((a: any, b: any) => {
        let valA = a[field];
        let valB = b[field];
        
        if (field === 'department') {
          valA = a.departmentName || '';
          valB = b.departmentName || '';
        } else if (field === 'applicantCount') {
          valA = a.applicantCount || 0;
          valB = b.applicantCount || 0;
        }

        if (typeof valA === 'string') {
          return valA.localeCompare(valB) * order;
        }
        if (typeof valA === 'number') {
          return (valA - valB) * order;
        }
        return 0;
      });
    }

    return list;
  }

  public toggleSort(field: string) {
    if (this.sortBy() === field) {
      this.sortOrder.update(order => order === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortBy.set(field);
      this.sortOrder.set('asc');
    }
  }

  // Checkbox Select Row Methods
  public toggleSelectAll() {
    const currentFiltered = this.getFilteredJobs();
    const selected = this.selectedJobIds();
    const allSelected = currentFiltered.every(job => selected.has(job.id));
    const newSelected = new Set(selected);
    
    if (allSelected) {
      currentFiltered.forEach(job => newSelected.delete(job.id));
    } else {
      currentFiltered.forEach(job => newSelected.add(job.id));
    }
    this.selectedJobIds.set(newSelected);
  }

  public toggleSelectJob(jobId: string) {
    const selected = this.selectedJobIds();
    const newSelected = new Set(selected);
    if (newSelected.has(jobId)) {
      newSelected.delete(jobId);
    } else {
      newSelected.add(jobId);
    }
    this.selectedJobIds.set(newSelected);
  }

  public bulkChangeStatus(status: number) {
    const ids = Array.from(this.selectedJobIds());
    if (ids.length === 0) return;
    
    let completed = 0;
    ids.forEach(id => {
      this.jobService.updateJobStatus(id, status).subscribe({
        next: (res) => {
          completed++;
          if (res.isSuccess) {
            this.jobs.update(items => 
              items.map(item => item.id === id ? { ...item, status } : item)
            );
          }
          if (completed === ids.length) {
            this.selectedJobIds.set(new Set());
          }
        },
        error: () => {
          completed++;
          if (completed === ids.length) {
            this.selectedJobIds.set(new Set());
          }
        }
      });
    });
  }
}
