import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApplicationService, ApplicationDto } from '../../core/services/application.service';
import { JobService, JobDto } from '../../core/services/job.service';
import { AnalyticsService } from '../../core/services/analytics.service';
import { DragDropModule, CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { CandidateNoteService } from '../../core/services/candidate-note.service';
import { InterviewService } from '../../core/services/interview.service';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

export interface EnrichedApplication extends ApplicationDto {
  currentRole: string;
  yearsOfExperience: number;
  expectedSalary: string;
  currentRecruiter: string;
  priorityLevel: 'High' | 'Medium' | 'Low';
  timeInStageDays: number;
  aiWarning?: string;
  suggestedAction?: string;
}

@Component({
  selector: 'app-pipeline',
  standalone: true,
  imports: [
    CommonModule,
    DragDropModule,
    FormsModule
  ],
  templateUrl: './pipeline.component.html',
  styleUrls: ['./pipeline.component.scss']
})
export class PipelineComponent implements OnInit {
  private applicationService = inject(ApplicationService);
  private jobService = inject(JobService);
  private candidateNoteService = inject(CandidateNoteService);
  private interviewService = inject(InterviewService);
  private sanitizer = inject(DomSanitizer);
  private analyticsService = inject(AnalyticsService);
  private router = inject(Router);
  private authService = inject(AuthService);

  public canMoveCandidate = computed(() => {
    const role = this.authService.currentUser()?.role;
    return role === 'SuperAdmin' || role === 'Recruiter' || role === 'HiringManager';
  });

  public canScheduleInterview = computed(() => {
    const role = this.authService.currentUser()?.role;
    return role === 'SuperAdmin' || role === 'Recruiter';
  });

  public canAddNote = computed(() => {
    const role = this.authService.currentUser()?.role;
    return role === 'SuperAdmin' || role === 'Recruiter' || role === 'HiringManager';
  });

  public overallConversion = signal<number>(8.2);
  public offerConversion = signal<number>(66.7);
  public interviewConversion = signal<number>(40.0);
  public hiringVelocity = signal<number>(14.5);
 
  // Modals signals
  public showNoteModal = signal<boolean>(false);
  public noteCandidate = signal<EnrichedApplication | null>(null);
  public newNoteText = signal<string>('');
  public noteSubmitting = signal<boolean>(false);
 
  public showResumeModal = signal<boolean>(false);
  public resumeCandidate = signal<EnrichedApplication | null>(null);
 
  public showScheduleModal = signal<boolean>(false);
  public scheduleCandidate = signal<EnrichedApplication | null>(null);
  public interviewers = signal<any[]>([]);
  public scheduling = signal<boolean>(false);
  public scheduleError = signal<string | null>(null);
 
  public scheduleForm = {
    title: '',
    interviewerId: '',
    type: 0,
    scheduledTime: '',
    durationMinutes: 45,
    videoLink: 'https://meet.google.com/abc-defg-hij'
  };

  public jobs = signal<JobDto[]>([]);
  public selectedJobId = signal<string>('');
  public loading = signal<boolean>(true);
  public searchQuery = signal<string>('');
  public minScore = signal<number>(0);
  public recruitersList: string[] = ['Himanshu Tripathi', 'Alice Smith', 'John Recruiter', 'Sarah Connor'];

  // 7-Stage Pipeline Columns
  public stages = ['Applied', 'Screening', 'Technical', 'Manager Round', 'HR', 'Offer', 'Hired'];
  
  // Visible limits per stage
  public visibleLimits = signal<{ [key: string]: number }>({
    'Applied': 10,
    'Screening': 10,
    'Technical': 10,
    'Manager Round': 10,
    'HR': 10,
    'Offer': 10,
    'Hired': 10
  });

  public showMore(stage: string) {
    this.visibleLimits.update(limits => ({
      ...limits,
      [stage]: (limits[stage] || 10) + 10
    }));
  }

  // Data lists mapped per stage
  public columns: { [key: string]: EnrichedApplication[] } = {
    'Applied': [],
    'Screening': [],
    'Technical': [],
    'Manager Round': [],
    'HR': [],
    'Offer': [],
    'Hired': []
  };

  private enrichApplication(app: ApplicationDto, index: number): EnrichedApplication {
    const name = app.candidateName || 'Candidate';
    const matchScore = app.aimatchScore || 70;
    
    // Deterministic Priority Level: High if score >= 85, Medium if >= 70, Low otherwise
    const priorityLevel: 'High' | 'Medium' | 'Low' = 
      matchScore >= 85 ? 'High' : (matchScore >= 72 ? 'Medium' : 'Low');

    // Experience (dynamic from candidate, fallback to matchScore heuristic)
    let yearsOfExperience = Math.max(1, (matchScore % 8) + 2);
    if (app.candidate?.yearsOfExperience) {
      const match = String(app.candidate.yearsOfExperience).match(/\d+/);
      if (match) {
        yearsOfExperience = parseInt(match[0], 10);
      }
    }

    // Expected Salary (dynamic from candidate, fallback to heuristic)
    let expectedSalary = `$${Math.round(85 + (matchScore * 0.6) + (yearsOfExperience * 4.5))}k Expected`;
    if (app.candidate?.expectedSalary) {
      const salVal = app.candidate.expectedSalary;
      if (salVal >= 100000) {
        expectedSalary = `INR ${Math.round(salVal / 100000)} LPA`;
      } else {
        expectedSalary = salVal > 1000 ? `$${Math.round(salVal / 1000)}k` : `$${salVal}k`;
      }
    }

    // Recruiter (dynamic from job, fallback to index-based recruiter list)
    const job = this.jobs().find(j => j.id === app.jobId);
    const currentRecruiter = job?.recruiterName || this.recruitersList[index % this.recruitersList.length];

    // Current Role (dynamic from candidate, fallback to job title)
    const currentRole = app.candidate?.currentTitle || app.jobTitle || 'Software Engineer';

    // Time in stage days (dynamic from backend)
    const timeInStageDays = app.timeInStageDays || 1;

    // Generate AI warnings for blocked candidates (time in stage >= 6 days)
    let aiWarning: string | undefined;
    let suggestedAction: string | undefined;

    if (app.stage === 'Technical Interview' && timeInStageDays >= 7) {
      aiWarning = `Candidate has been in Technical stage for ${timeInStageDays} days.`;
      suggestedAction = 'Schedule manager interview.';
    } else if (app.stage === 'Screening' && timeInStageDays >= 6) {
      aiWarning = `Candidate has been in Screening for ${timeInStageDays} days.`;
      suggestedAction = 'Send technical assessment.';
    } else if ((app.stage === 'Applied' || app.stage === 'Recruiter Review') && timeInStageDays >= 5) {
      aiWarning = `New application pending review for ${timeInStageDays} days.`;
      suggestedAction = 'Schedule recruiter screening.';
    } else if (app.stage === 'Final Interview' && timeInStageDays >= 4) {
      aiWarning = `No interview feedback logged for ${timeInStageDays} days.`;
      suggestedAction = 'Collect interviewer feedback.';
    } else if (app.stage === 'Offer' && timeInStageDays >= 5) {
      aiWarning = `Offer pending acceptance for ${timeInStageDays} days.`;
      suggestedAction = 'Follow up on offer letter status.';
    }

    return {
      ...app,
      currentRole,
      yearsOfExperience,
      expectedSalary,
      currentRecruiter,
      priorityLevel,
      timeInStageDays,
      aiWarning,
      suggestedAction
    };
  }

  public getFilteredApps(stage: string): EnrichedApplication[] {
    let list = this.columns[stage] || [];
    const query = this.searchQuery().toLowerCase().trim();
    const min = this.minScore();

    if (query) {
      list = list.filter(app => 
        app.candidateName.toLowerCase().includes(query) || 
        app.candidateEmail.toLowerCase().includes(query) ||
        (app.skillsMatch && app.skillsMatch.some(s => s.toLowerCase().includes(query))) ||
        (app.currentRole && app.currentRole.toLowerCase().includes(query))
      );
    }

    if (min > 0) {
      list = list.filter(app => (app.aimatchScore || 0) >= min);
    }

    // Sort by Priority: High > Medium > Low, then by aimatchScore descending
    const priorityWeight = { 'High': 3, 'Medium': 2, 'Low': 1 };
    list = [...list].sort((a, b) => {
      const pA = priorityWeight[a.priorityLevel] || 1;
      const pB = priorityWeight[b.priorityLevel] || 1;
      if (pA !== pB) return pB - pA;
      return (b.aimatchScore || 0) - (a.aimatchScore || 0);
    });

    return list;
  }

  public ngOnInit() {
    this.loadJobs();
    this.loadRecruiters();
    this.loadInterviewers();
    this.loadAnalyticsReport();
  }

  public loadAnalyticsReport() {
    this.analyticsService.getAnalyticsReport().subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.overallConversion.set(res.data.overallConversionRate);
          this.offerConversion.set(res.data.offerConversionRate);
          this.interviewConversion.set(res.data.interviewConversionRate);
          this.hiringVelocity.set(res.data.averageTimeToHireDays);
        }
      }
    });
  }
 
  public loadInterviewers() {
    this.jobService.getUsers().subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.interviewers.set(res.data);
        }
      }
    });
  }

  public loadRecruiters() {
    this.jobService.getUsers('Recruiter').subscribe({
      next: (res) => {
        if (res.isSuccess && res.data && res.data.length > 0) {
          this.recruitersList = res.data.map((u: any) => `${u.firstName} ${u.lastName}`);
        }
      }
    });
  }

  public loadJobs() {
    this.loading.set(true);
    this.jobService.getJobs().subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          const list = res.data.items || [];
          this.jobs.set(list);
          if (list.length > 0) {
            this.selectedJobId.set(list[0].id);
            this.loadApplications(list[0].id);
          } else {
            this.loading.set(false);
          }
        } else {
          this.loading.set(false);
        }
      },
      error: () => this.loading.set(false)
    });
  }

  public onJobChange(jobId: string) {
    this.selectedJobId.set(jobId);
    this.loadApplications(jobId);
  }

  public loadApplications(jobId: string) {
    this.loading.set(true);
    // Clear columns
    this.stages.forEach(stage => this.columns[stage] = []);

    this.applicationService.getApplications({ jobId }).subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          const apps = res.data as ApplicationDto[];
          apps.forEach((app, index) => {
            const backendStage = app.stage || 'Applied';
            let mappedStage = 'Applied';
            
            if (backendStage === 'Applied') {
              mappedStage = 'Applied';
            } else if (backendStage === 'Screening' || backendStage === 'Recruiter Review') {
              mappedStage = 'Screening';
            } else if (backendStage === 'Technical Interview') {
              mappedStage = 'Technical';
            } else if (backendStage === 'Hiring Manager Review' || backendStage.toLowerCase().includes('manager')) {
              mappedStage = 'Manager Round';
            } else if (backendStage === 'HR Interview' || backendStage === 'Final Interview' || backendStage.toLowerCase().includes('interview') || backendStage.toLowerCase().includes('hr')) {
              mappedStage = 'HR';
            } else if (backendStage === 'Offer') {
              mappedStage = 'Offer';
            } else if (backendStage === 'Hired') {
              mappedStage = 'Hired';
            } else {
              mappedStage = 'Applied';
            }
            
            if (this.columns[mappedStage]) {
              this.columns[mappedStage].push(this.enrichApplication(app, index));
            }
          });
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  public drop(event: CdkDragDrop<EnrichedApplication[]>) {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      const app = event.previousContainer.data[event.previousIndex];
      const targetColumn = event.container.id; 
      
      // Map columns back to backend stages
      let newBackendStage = 'Applied';
      if (targetColumn === 'Applied') newBackendStage = 'Applied';
      else if (targetColumn === 'Screening') newBackendStage = 'Screening';
      else if (targetColumn === 'Technical') newBackendStage = 'Technical Interview';
      else if (targetColumn === 'Manager Round') newBackendStage = 'Hiring Manager Review';
      else if (targetColumn === 'HR') newBackendStage = 'HR Interview';
      else if (targetColumn === 'Offer') newBackendStage = 'Offer';
      else if (targetColumn === 'Hired') newBackendStage = 'Hired';

      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );

      this.applicationService.updateStage(app.id, newBackendStage).subscribe({
        next: (res) => {
          if (!res.isSuccess) {
            transferArrayItem(
              event.container.data,
              event.previousContainer.data,
              event.currentIndex,
              event.previousIndex
            );
          } else {
            app.stage = newBackendStage;
          }
        },
        error: () => {
          transferArrayItem(
            event.container.data,
            event.previousContainer.data,
            event.currentIndex,
            event.previousIndex
          );
        }
      });
    }
  }

  public getScoreClass(score?: number): string {
    if (!score) return '';
    if (score >= 80) return 'text-emerald-600 dark:text-emerald-400 bg-emerald-500/10 border-emerald-500/20';
    if (score >= 60) return 'text-amber-600 dark:text-amber-400 bg-amber-500/10 border-amber-500/20';
    return 'text-red-600 dark:text-red-400 bg-red-500/10 border-red-500/20';
  }

  public getInitials(name: string): string {
    if (!name) return '';
    const parts = name.trim().split(' ');
    if (parts.length > 1) {
      return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
    }
    return parts[0].charAt(0).toUpperCase();
  }

  public getStageCount(stage: string): number {
    return this.columns[stage]?.length || 0;
  }

  // Stage-to-stage Conversion %
  public getConversionRate(stage: string): number {
    const stageIndex = this.stages.indexOf(stage);
    if (stageIndex <= 0) return 100; 
    
    const prevStage = this.stages[stageIndex - 1];
    const prevCount = this.columns[prevStage]?.length || 0;
    const currentCount = this.columns[stage]?.length || 0;
    
    if (prevCount === 0) return 0;
    return Math.round((currentCount / prevCount) * 100);
  }

  // Stage-to-stage Drop-off %
  public getDropOffRate(stage: string): number {
    const stageIndex = this.stages.indexOf(stage);
    if (stageIndex <= 0) return 0; 
    return 100 - this.getConversionRate(stage);
  }

  // Bottleneck Detection
  public isBottleneck(stage: string): boolean {
    const count = this.columns[stage]?.length || 0;
    const avgTime = this.getAvgTimeInStage(stage);
    
    if (stage === 'Screening') {
      return count >= 4 || avgTime > 5.0;
    }
    if (stage === 'Technical') {
      return count >= 3 || avgTime > 4.0;
    }
    return count >= 5 || avgTime > 5.0;
  }

  public getAvgTimeInStage(stage: string): number {
    const list = this.columns[stage] || [];
    if (list.length === 0) {
      const staticAverages: { [key: string]: number } = {
        'Applied': 1.2,
        'Screening': 5.6,
        'Technical': 4.2,
        'Manager Round': 3.1,
        'HR': 2.0,
        'Offer': 4.8,
        'Hired': 0.5
      };
      return staticAverages[stage] || 0;
    }
    
    const sum = list.reduce((acc, app) => acc + (app.timeInStageDays || 0), 0);
    return Math.round((sum / list.length) * 10) / 10;
  }

  public getOverallConversion(): number {
    const totalApps = this.stages.reduce((acc, st) => acc + (this.columns[st]?.length || 0), 0);
    const hiredApps = this.columns['Hired']?.length || 0;
    if (totalApps === 0) return this.overallConversion(); 
    return Math.round((hiredApps / totalApps) * 1000) / 10;
  }

  public getOfferConversion(): number {
    const offerCount = this.columns['Offer']?.length || 0;
    const hiredCount = this.columns['Hired']?.length || 0;
    const totalOffers = offerCount + hiredCount;
    if (totalOffers === 0) return this.offerConversion(); 
    return Math.round((hiredCount / totalOffers) * 100);
  }

  public getInterviewConversion(): number {
    const techCount = this.columns['Technical']?.length || 0;
    const managerCount = this.columns['Manager Round']?.length || 0;
    const hrCount = this.columns['HR']?.length || 0;
    const offerCount = this.columns['Offer']?.length || 0;
    const totalInterviews = techCount + managerCount + hrCount;
    if (totalInterviews === 0) return this.interviewConversion(); 
    return Math.round((offerCount / totalInterviews) * 100);
  }

  public getHiringVelocity(): number {
    return this.hiringVelocity();
  }

  public getBottleneckAlert(): string | null {
    for (const stage of this.stages) {
      if (this.isBottleneck(stage)) {
        const count = this.columns[stage]?.length || 0;
        return `${stage} backlog detected. ${count} candidates have been waiting more than 5 days.`;
      }
    }
    return null;
  }

  // Recruiter Direct Actions
  public onMoveCandidate(app: EnrichedApplication) {
    const currentIdx = this.stages.indexOf(app.stage);
    if (currentIdx >= 0 && currentIdx < this.stages.length - 1) {
      const nextStage = this.stages[currentIdx + 1];
      
      this.columns[app.stage] = this.columns[app.stage].filter(a => a.id !== app.id);
      app.stage = nextStage;
      this.columns[nextStage].push(app);

      let newBackendStage = 'Applied';
      if (nextStage === 'Applied') newBackendStage = 'Applied';
      else if (nextStage === 'Screening') newBackendStage = 'Screening';
      else if (nextStage === 'Technical') newBackendStage = 'Technical Interview';
      else if (nextStage === 'Manager Round') newBackendStage = 'Hiring Manager Review';
      else if (nextStage === 'HR') newBackendStage = 'HR Interview';
      else if (nextStage === 'Offer') newBackendStage = 'Offer';
      else if (nextStage === 'Hired') newBackendStage = 'Hired';

      this.applicationService.updateStage(app.id, newBackendStage).subscribe();
    }
  }

  public onScheduleInterview(app: EnrichedApplication) {
    this.scheduleCandidate.set(app);
    this.scheduleError.set(null);
    this.scheduleForm = {
      title: `${app.candidateName} - Technical Interview`,
      interviewerId: this.interviewers().length > 0 ? this.interviewers()[0].id : '',
      type: 0,
      scheduledTime: new Date(new Date().getTime() + 24 * 60 * 60 * 1000).toISOString().slice(0, 16),
      durationMinutes: 45,
      videoLink: 'https://meet.google.com/abc-defg-hij'
    };
    this.showScheduleModal.set(true);
  }
 
  public submitSchedule() {
    const app = this.scheduleCandidate();
    if (!app) return;
 
    this.scheduling.set(true);
    this.scheduleError.set(null);
 
    const payload = {
      applicationId: app.id,
      interviewerId: this.scheduleForm.interviewerId,
      title: this.scheduleForm.title,
      type: Number(this.scheduleForm.type),
      scheduledTime: new Date(this.scheduleForm.scheduledTime).toISOString(),
      durationMinutes: Number(this.scheduleForm.durationMinutes),
      videoLink: this.scheduleForm.videoLink
    };
 
    this.interviewService.scheduleInterview(payload).subscribe({
      next: (res) => {
        this.scheduling.set(false);
        if (res.isSuccess) {
          this.showScheduleModal.set(false);
          this.scheduleCandidate.set(null);
          this.loadApplications(this.selectedJobId());
        } else {
          this.scheduleError.set(res.message || 'Failed to schedule interview.');
        }
      },
      error: (err) => {
        this.scheduling.set(false);
        this.scheduleError.set(err.error?.message || 'Error scheduling interview.');
      }
    });
  }
 
  public onViewResume(app: EnrichedApplication) {
    this.resumeCandidate.set(app);
    this.showResumeModal.set(true);
  }
 
  public getResumeUrl(app: EnrichedApplication): string {
    if (!app.candidate?.resumePath) return '';
    const base = window.location.hostname.includes('localhost') ? 'http://localhost:5000' : window.location.origin;
    return `${base}/${app.candidate.resumePath}`;
  }
 
  public getSafeResumeUrl(app: EnrichedApplication): SafeResourceUrl {
    const url = this.getResumeUrl(app);
    return this.sanitizer.bypassSecurityTrustResourceUrl(url);
  }
 
  public onAddNote(app: EnrichedApplication) {
    this.router.navigate(['/candidates'], { queryParams: { id: app.candidateId, tab: 'notes' } });
  }
 
  public submitNote() {
    const app = this.noteCandidate();
    const text = this.newNoteText().trim();
    if (!app || !text) return;
 
    this.noteSubmitting.set(true);
    this.candidateNoteService.createNote({
      candidateId: app.candidateId,
      applicationId: app.id,
      text: text
    }).subscribe({
      next: (res) => {
        this.noteSubmitting.set(false);
        if (res.isSuccess) {
          this.showNoteModal.set(false);
          this.noteCandidate.set(null);
          this.newNoteText.set('');
        }
      },
      error: () => this.noteSubmitting.set(false)
    });
  }

  public onRejectCandidate(app: EnrichedApplication) {
    if (confirm(`Are you sure you want to reject ${app.candidateName}?`)) {
      this.columns[app.stage] = this.columns[app.stage].filter(a => a.id !== app.id);
      this.applicationService.updateStage(app.id, 'Rejected').subscribe();
    }
  }
}

