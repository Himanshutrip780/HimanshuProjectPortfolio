import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { CareersService } from '../../core/services/careers.service';

@Component({
  selector: 'app-careers',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  template: `
    <div [style.--primary-color]="tenantConfig()?.primaryColor || '#4f46e5'" [style.font-family]="tenantConfig()?.fontFamily || 'inherit'" class="min-h-screen bg-background text-foreground transition-colors duration-300 font-sans pb-16">
      <style *ngIf="tenantConfig()?.customCss" [innerHTML]="customCss()"></style>
      
      <!-- Public Careers Header -->
      <header class="bg-card border-b border-border py-8 px-6 mb-8 shadow-level1">
        <div class="max-w-5xl mx-auto flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
          <div class="flex items-center gap-4">
            @if (tenantConfig()?.logoUrl) {
              <img [src]="tenantConfig()?.logoUrl" alt="Logo" class="h-12 w-auto object-contain">
            }
            <div>
              <h1 class="text-3xl font-extrabold tracking-tight text-foreground">Careers at {{ tenantConfig()?.companyName || 'Acme Corp' }}</h1>
              <p class="text-xs text-muted-foreground mt-1">Explore our open roles, submit your resume, and join our world-class team.</p>
            </div>
          </div>
          <div class="flex items-center gap-3 w-full md:w-auto">
            <input 
              [ngModel]="searchTerm()" 
              (ngModelChange)="onSearchChange($event)"
              placeholder="Search positions..." 
              class="input-field text-xs py-2 px-3 w-full md:w-64"
            >
          </div>
        </div>
      </header>

      <!-- Main Careers Container -->
      <main class="max-w-5xl mx-auto px-6">
        
        <!-- Job Details / Apply view -->
        @if (selectedJob()) {
          <div class="flex flex-col gap-6">
            <!-- Details Header -->
            <div class="flex items-center gap-3 pb-4 border-b border-border">
              <button 
                (click)="selectedJob.set(null); applySuccess.set(null); applyError.set(null); applyForm.reset()"
                class="btn-secondary py-1.5 px-3 flex items-center gap-1.5 text-xs font-semibold"
              >
                ← Back to open roles
              </button>
            </div>

            <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
              <!-- Left 2 columns: Description -->
              <div class="lg:col-span-2 space-y-6">
                <div class="space-y-2">
                  <h2 class="text-2xl font-bold text-foreground">{{ selectedJob()?.title }}</h2>
                  <div class="flex flex-wrap gap-2 text-xs text-muted-foreground">
                    <span class="px-2 py-0.5 bg-secondary border border-border rounded-md">📍 {{ selectedJob()?.location }}</span>
                    <span class="px-2 py-0.5 bg-secondary border border-border rounded-md">💼 {{ selectedJob()?.employmentType }}</span>
                    <span class="px-2 py-0.5 bg-secondary border border-border rounded-md">🏢 {{ selectedJob()?.departmentName }}</span>
                  </div>
                </div>

                <div class="prose prose-sm dark:prose-invert max-w-none text-xs text-foreground/90 space-y-4">
                  <h3 class="text-sm font-bold uppercase tracking-wider border-b border-border pb-1 select-none">Job Description</h3>
                  <div [innerHTML]="selectedJob()?.description"></div>

                  @if (selectedJob()?.responsibilities) {
                    <h3 class="text-sm font-bold uppercase tracking-wider border-b border-border pb-1 select-none pt-4">Responsibilities</h3>
                    <ul class="list-disc pl-5 space-y-1">
                      @for (resp of selectedJob()?.responsibilities?.split(';'); track resp) {
                        <li>{{ resp }}</li>
                      }
                    </ul>
                  }

                  @if (selectedJob()?.qualifications) {
                    <h3 class="text-sm font-bold uppercase tracking-wider border-b border-border pb-1 select-none pt-4">Qualifications</h3>
                    <ul class="list-disc pl-5 space-y-1">
                      @for (qual of selectedJob()?.qualifications?.split(';'); track qual) {
                        <li>{{ qual }}</li>
                      }
                    </ul>
                  }
                </div>
              </div>

              <!-- Right 1 column: Application Form -->
              <div class="bg-card border border-border rounded-[20px] p-6 shadow-level1 h-fit space-y-4">
                <h3 class="text-sm font-bold uppercase tracking-wider border-b border-border pb-2 select-none">Apply for this position</h3>

                @if (applySuccess()) {
                  <div class="p-3 bg-emerald-500/10 border border-emerald-500/20 text-emerald-600 dark:text-emerald-400 text-xs font-semibold rounded-[14px]">
                    {{ applySuccess() }}
                  </div>
                } @else {
                  <form [formGroup]="applyForm" (ngSubmit)="onSubmitApplication()" class="space-y-3.5 text-xs">
                    <div class="space-y-1">
                      <label class="block font-bold text-muted-foreground uppercase tracking-wider text-[10px]">First Name</label>
                      <input formControlName="firstName" class="input-field py-2 px-3 text-xs w-full">
                    </div>

                    <div class="space-y-1">
                      <label class="block font-bold text-muted-foreground uppercase tracking-wider text-[10px]">Last Name</label>
                      <input formControlName="lastName" class="input-field py-2 px-3 text-xs w-full">
                    </div>

                    <div class="space-y-1">
                      <label class="block font-bold text-muted-foreground uppercase tracking-wider text-[10px]">Email Address</label>
                      <input type="email" formControlName="email" class="input-field py-2 px-3 text-xs w-full">
                    </div>

                    <div class="space-y-1">
                      <label class="block font-bold text-muted-foreground uppercase tracking-wider text-[10px]">Phone Number</label>
                      <input formControlName="phone" class="input-field py-2 px-3 text-xs w-full">
                    </div>

                    <div class="space-y-1">
                      <label class="block font-bold text-muted-foreground uppercase tracking-wider text-[10px]">LinkedIn URL (Optional)</label>
                      <input formControlName="linkedInUrl" class="input-field py-2 px-3 text-xs w-full">
                    </div>

                    <div class="space-y-1">
                      <label class="block font-bold text-muted-foreground uppercase tracking-wider text-[10px]">Resume (PDF, DOCX, TXT)</label>
                      <div class="border border-dashed border-border rounded-[14px] p-4 text-center hover:border-primary/50 transition-colors relative cursor-pointer">
                        <span class="text-xs text-muted-foreground">
                          {{ resumeFile ? resumeFile.name : 'Click to select resume file' }}
                        </span>
                        <input type="file" accept=".pdf,.docx,.doc,.txt,.rtf" (change)="onFileSelected($event)" class="absolute inset-0 opacity-0 cursor-pointer">
                      </div>
                    </div>

                    @if (applyError()) {
                      <div class="p-2.5 bg-destructive/10 border border-destructive/20 text-destructive text-xs font-semibold rounded-[14px]">
                        {{ applyError() }}
                      </div>
                    }

                    <button 
                      type="submit" 
                      [disabled]="applyForm.invalid || !resumeFile || applying()" 
                      class="btn-primary w-full py-2.5 text-xs font-bold"
                    >
                      @if (applying()) {
                        <span class="animate-pulse">Submitting application...</span>
                      } @else {
                        <span>Submit Application</span>
                      }
                    </button>
                  </form>
                }
              </div>
            </div>
          </div>
        } 
        
        <!-- Open Positions List view -->
        @else {
          <div class="space-y-6">
            <h2 class="text-xl font-bold text-foreground select-none">Open Positions</h2>

            @if (loadingJobs()) {
              <div class="flex justify-center py-12">
                <span class="animate-spin border-2 border-primary/30 border-t-primary rounded-full w-6 h-6"></span>
              </div>
            } @else {
              <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                @for (job of jobs(); track job.id) {
                  <div 
                    (click)="viewJobDetails(job.id)"
                    class="card-level1 p-6 cursor-pointer hover:shadow-level2 transition-all duration-200 flex flex-col justify-between min-h-[160px]"
                  >
                    <div class="space-y-2">
                      <h3 class="text-base font-bold text-foreground group-hover:text-primary transition-colors">{{ job.title }}</h3>
                      <span class="text-xs text-muted-foreground font-semibold block">{{ job.departmentName }} • {{ job.location }}</span>
                    </div>

                    <div class="flex items-center justify-between border-t border-border pt-4 mt-auto">
                      <span class="text-[10px] uppercase font-bold text-muted-foreground tracking-wider">{{ job.employmentType }}</span>
                      <span class="text-xs font-bold text-primary hover:underline">Apply now →</span>
                    </div>
                  </div>
                }

                @if (jobs().length === 0) {
                  <div class="col-span-full bg-card border border-border rounded-[20px] p-12 text-center text-muted-foreground text-xs leading-normal">
                    No open positions match your search query. Please try adjusting your filters.
                  </div>
                }
              </div>
            }
          </div>
        }
      </main>
    </div>
  `,
  styles: [`
    .shadow-level1 {
      box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
    }
    .btn-primary {
      background-color: var(--primary-color, #4f46e5) !important;
      border-color: var(--primary-color, #4f46e5) !important;
      color: #ffffff !important;
    }
    .btn-primary:hover {
      filter: brightness(0.9);
    }
    .text-primary {
      color: var(--primary-color, #4f46e5) !important;
    }
    .border-primary {
      border-color: var(--primary-color, #4f46e5) !important;
    }
    .border-t-primary {
      border-top-color: var(--primary-color, #4f46e5) !important;
    }
  `]
})
export class CareersComponent implements OnInit {
  private careersService = inject(CareersService);
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private sanitizer = inject(DomSanitizer);

  public jobs = signal<any[]>([]);
  public selectedJob = signal<any | null>(null);
  public loadingJobs = signal<boolean>(true);
  public searchTerm = signal<string>('');
  
  public companyId = signal<string>('');
  public tenantConfig = signal<any>(null);
  public customCss = signal<SafeHtml | null>(null);

  // Apply state
  public applyForm: FormGroup;
  public resumeFile: File | null = null;
  public applying = signal<boolean>(false);
  public applySuccess = signal<string | null>(null);
  public applyError = signal<string | null>(null);

  constructor() {
    this.applyForm = this.fb.group({
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required]],
      linkedInUrl: ['']
    });
  }

  public ngOnInit() {
    this.route.queryParams.subscribe(params => {
      const compId = params['companyId'] || '';
      this.companyId.set(compId);
      if (compId) {
        this.careersService.getPublicBranding(compId).subscribe({
          next: (res) => {
            if (res.isSuccess && res.data) {
              this.tenantConfig.set(res.data);
              if (res.data.customCss) {
                this.customCss.set(this.sanitizer.bypassSecurityTrustHtml(res.data.customCss));
              } else {
                this.customCss.set(null);
              }
            } else {
              this.tenantConfig.set(null);
              this.customCss.set(null);
            }
          },
          error: () => {
            this.tenantConfig.set(null);
            this.customCss.set(null);
          }
        });
      } else {
        this.tenantConfig.set(null);
        this.customCss.set(null);
      }
      this.loadJobs();
    });
  }

  public loadJobs() {
    this.loadingJobs.set(true);
    this.careersService.getPublicJobs({ searchTerm: this.searchTerm(), companyId: this.companyId() }).subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.jobs.set(res.data.items || []);
        }
        this.loadingJobs.set(false);
      },
      error: () => this.loadingJobs.set(false)
    });
  }

  public onSearchChange(term: string) {
    this.searchTerm.set(term);
    this.loadJobs();
  }

  public viewJobDetails(jobId: string) {
    this.careersService.getPublicJob(jobId).subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.selectedJob.set(res.data);
        }
      }
    });
  }

  public onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      const allowedExtensions = ['.pdf', '.docx', '.doc', '.txt', '.rtf'];
      const extension = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
      if (!allowedExtensions.includes(extension)) {
        this.applyError.set('Invalid file extension. Only .pdf, .docx, .doc, .txt, and .rtf files are allowed.');
        this.resumeFile = null;
        event.target.value = '';
        return;
      }
      this.applyError.set(null);
      this.resumeFile = file;
    }
  }

  public onSubmitApplication() {
    const job = this.selectedJob();
    if (this.applyForm.invalid || !this.resumeFile || !job) return;

    this.applying.set(true);
    this.applySuccess.set(null);
    this.applyError.set(null);

    const formData = new FormData();
    formData.append('firstName', this.applyForm.value.firstName);
    formData.append('lastName', this.applyForm.value.lastName);
    formData.append('email', this.applyForm.value.email);
    formData.append('phone', this.applyForm.value.phone);
    formData.append('linkedInUrl', this.applyForm.value.linkedInUrl || '');
    formData.append('file', this.resumeFile);

    this.careersService.apply(job.id, formData).subscribe({
      next: (res) => {
        this.applying.set(false);
        if (res.isSuccess) {
          this.applySuccess.set('Thank you! Your application has been successfully submitted.');
          this.resumeFile = null;
        } else {
          this.applyError.set(res.message || 'Failed to submit application.');
        }
      },
      error: (err) => {
        this.applying.set(false);
        this.applyError.set(err.error?.message || 'Error submitting application.');
      }
    });
  }
}
