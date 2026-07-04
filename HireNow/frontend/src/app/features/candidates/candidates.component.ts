import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CandidateService, CandidateDto } from '../../core/services/candidate.service';
import { JobService, JobDto } from '../../core/services/job.service';
import { ApplicationService, ApplicationDto, ActivityLogDto } from '../../core/services/application.service';
import { InterviewService, InterviewDto } from '../../core/services/interview.service';
import { CandidateNoteService } from '../../core/services/candidate-note.service';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-candidates',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule
  ],
  templateUrl: './candidates.component.html',
  styleUrls: ['./candidates.component.scss']
})
export class CandidatesComponent implements OnInit {
  private candidateService = inject(CandidateService);
  private jobService = inject(JobService);
  private applicationService = inject(ApplicationService);
  private interviewService = inject(InterviewService);
  private candidateNoteService = inject(CandidateNoteService);
  private authService = inject(AuthService);

  public canUploadResume = computed(() => {
    const role = this.authService.currentUser()?.role;
    return role === 'SuperAdmin' || role === 'Recruiter';
  });

  public canManageNotes = computed(() => {
    const role = this.authService.currentUser()?.role;
    return role === 'SuperAdmin' || role === 'Recruiter' || role === 'HiringManager';
  });

  public canDeleteNotes = computed(() => {
    const role = this.authService.currentUser()?.role;
    return role === 'SuperAdmin' || role === 'Recruiter';
  });

  public candidates = signal<CandidateDto[]>([]);
  public jobs = signal<JobDto[]>([]);
  public selectedCandidate = signal<CandidateDto | null>(null);
  public selectedApplication = signal<ApplicationDto | null>(null);
  public candidateApplications = signal<ApplicationDto[]>([]);
  public loading = signal<boolean>(true);
  public loadingDetails = signal<boolean>(false);
  
  // Upload properties
  public uploading = signal<boolean>(false);
  public selectedJobId = signal<string>('');
  public uploadError = signal<string | null>(null);
  public uploadSuccess = signal<string | null>(null);

  // Search, Filters & Sorting
  public searchQuery = signal<string>('');
  public selectedJobFilter = signal<string>('');
  public aiMatchFilter = signal<string>('all');
  public sortBy = signal<string>('date');
  public pageIndex = signal<number>(1);
  public pageSize = signal<number>(30);

  // Parsed metadata object helper
  public parsedResumeData = signal<any>(null);

  // CRM Tab Sheets state
  public activeTab = signal<string>('overview');
  public notes = signal<{ id?: string; candidateId?: string; applicationId?: string; text: string; date: string; author: string }[]>([]);
  public noteText = signal<string>('');
  public timeline = signal<ActivityLogDto[]>([]);
  public interviews = signal<InterviewDto[]>([]);
  public loadingTimeline = signal<boolean>(false);

  // Candidate Duplicates States
  public duplicates = signal<CandidateDto[]>([]);
  public loadingDuplicates = signal<boolean>(false);

  // Scorecard Submission States
  public showFeedbackForm = signal<boolean>(false);
  public feedbackInterviewId = signal<string>('');
  public communicationScore = signal<number>(3);
  public problemSolvingScore = signal<number>(3);
  public codingScore = signal<number>(3);
  public systemDesignScore = signal<number>(3);
  public cultureFitScore = signal<number>(3);
  public feedbackText = signal<string>('');
  public recommendation = signal<number>(2); // 2 = Neutral

  private route = inject(ActivatedRoute);

  public ngOnInit() {
    this.loadCandidates();
    this.loadJobs();

    this.route.queryParams.subscribe(params => {
      const candidateId = params['id'];
      const tab = params['tab'];
      if (candidateId) {
        this.viewCandidateDetails(candidateId);
        if (tab) {
          this.activeTab.set(tab);
        }
      }
    });
  }

  public loadCandidates() {
    this.loading.set(true);
    this.candidateService.getCandidates({
      searchTerm: this.searchQuery(),
      pageIndex: this.pageIndex(),
      pageSize: this.pageSize()
    }).subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.candidates.set(res.data.items || []);
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  public onSearchChanged(val: string) {
    this.searchQuery.set(val);
    this.pageIndex.set(1);
    this.loadCandidates();
  }

  public prevPage() {
    if (this.pageIndex() > 1) {
      this.pageIndex.update(idx => idx - 1);
      this.loadCandidates();
    }
  }

  public nextPage() {
    if (this.candidates().length === 30) {
      this.pageIndex.update(idx => idx + 1);
      this.loadCandidates();
    }
  }

  public getFilteredCandidates(): CandidateDto[] {
    let list = this.candidates() || [];
    const jobId = this.selectedJobFilter();
    const aiScore = this.aiMatchFilter();
    const sort = this.sortBy();

    // Filter by Job Requisition
    if (jobId) {
      const job = this.jobs().find(j => j.id === jobId);
      if (job) {
        list = list.filter(c => c.latestApplicationJobTitle === job.title);
      }
    }

    // Filter by AI Match Score
    if (aiScore === '80+') {
      list = list.filter(c => (c.latestApplicationMatchScore || 0) >= 80);
    } else if (aiScore === '60+') {
      list = list.filter(c => (c.latestApplicationMatchScore || 0) >= 60);
    }

    // Sorting
    if (sort === 'name') {
      list = [...list].sort((a, b) => {
        const nameA = `${a.firstName} ${a.lastName}`.toLowerCase();
        const nameB = `${b.firstName} ${b.lastName}`.toLowerCase();
        return nameA.localeCompare(nameB);
      });
    } else if (sort === 'date') {
      list = [...list].sort((a, b) => {
        return new Date(b.createdDate).getTime() - new Date(a.createdDate).getTime();
      });
    } else if (sort === 'score') {
      list = [...list].sort((a, b) => {
        return (b.latestApplicationMatchScore || 0) - (a.latestApplicationMatchScore || 0);
      });
    }

    return list;
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

  public onFileDropped(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    if (event.dataTransfer && event.dataTransfer.files.length > 0) {
      this.handleResumeUpload(event.dataTransfer.files[0]);
    }
  }

  public onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
  }

  public onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.handleResumeUpload(file);
    }
  }

  private handleResumeUpload(file: File) {
    this.uploading.set(true);
    this.uploadError.set(null);
    this.uploadSuccess.set(null);

    const jobIdVal = this.selectedJobId();

    this.candidateService.uploadResume(file, jobIdVal ? jobIdVal : undefined).subscribe({
      next: (res) => {
        this.uploading.set(false);
        if (res.isSuccess) {
          this.uploadSuccess.set(`Resume parsed successfully for ${file.name}!`);
          this.loadCandidates();
          
          if (res.data) {
            this.viewCandidateDetails(res.data);
          }
        } else {
          this.uploadError.set(res.message || 'Failed to parse resume.');
        }
      },
      error: (err) => {
        this.uploading.set(false);
        this.uploadError.set(err.error?.message || 'Error uploading file.');
      }
    });
  }

  public viewCandidateDetails(candidateId: string) {
    this.loadingDetails.set(true);
    this.selectedCandidate.set(null);
    this.selectedApplication.set(null);
    this.parsedResumeData.set(null);
    this.activeTab.set('overview');
    this.timeline.set([]);
    this.interviews.set([]);
    this.duplicates.set([]);

    this.candidateService.getCandidate(candidateId).subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          const candidate = res.data;
          this.selectedCandidate.set(candidate);
          this.loadNotes(candidate.id);
          this.loadDuplicates(candidate.id);
          
          if (candidate.parsedDataJson) {
            try {
              this.parsedResumeData.set(JSON.parse(candidate.parsedDataJson));
            } catch {
              this.parsedResumeData.set(null);
            }
          }

          // Fetch applications for this candidate
          this.loadCandidateApplication(candidateId);
        } else {
          this.loadingDetails.set(false);
        }
      },
      error: () => this.loadingDetails.set(false)
    });
  }

  private loadCandidateApplication(candidateId: string) {
    this.applicationService.getApplications({ candidateId }).subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.candidateApplications.set(res.data);
          if (res.data.length > 0) {
            const app = res.data[0];
            this.selectedApplication.set(app);
            this.loadTimeline(app.id);
            this.loadInterviews(app.id);
            this.loadNotes(candidateId);
          } else {
            this.selectedApplication.set(null);
            this.timeline.set([]);
            this.interviews.set([]);
          }
        }
        this.loadingDetails.set(false);
      },
      error: () => {
        this.candidateApplications.set([]);
        this.selectedApplication.set(null);
        this.loadingDetails.set(false);
      }
    });
  }

  public onApplicationChange(appId: string) {
    const app = this.candidateApplications().find(a => a.id === appId);
    if (app) {
      this.selectedApplication.set(app);
      this.loadTimeline(app.id);
      this.loadInterviews(app.id);
      this.loadNotes(this.selectedCandidate()!.id);
    }
  }

  public loadTimeline(applicationId: string) {
    this.loadingTimeline.set(true);
    this.applicationService.getTimeline(applicationId).subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.timeline.set(res.data);
        }
        this.loadingTimeline.set(false);
      },
      error: () => this.loadingTimeline.set(false)
    });
  }

  public loadInterviews(applicationId: string) {
    this.interviewService.getInterviews({ applicationId }).subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.interviews.set(res.data);
        }
      }
    });
  }

  public loadDuplicates(candidateId: string) {
    this.loadingDuplicates.set(true);
    this.candidateService.getDuplicates(candidateId).subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.duplicates.set(res.data);
        }
        this.loadingDuplicates.set(false);
      },
      error: () => this.loadingDuplicates.set(false)
    });
  }

  public assignTalentPool(poolName: string) {
    const cand = this.selectedCandidate();
    if (!cand || !poolName) return;

    this.candidateService.assignTalentPool(cand.id, poolName).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.showToast(`Candidate successfully assigned to Talent Pool: ${poolName}`);
          this.viewCandidateDetails(cand.id);
          this.loadCandidates();
        } else {
          this.showToast(res.message || 'Failed to assign talent pool.', 'error');
        }
      },
      error: () => this.showToast('Error occurred during pool assignment.', 'error')
    });
  }

  public openFeedbackForm(interviewId: string) {
    this.feedbackInterviewId.set(interviewId);
    this.showFeedbackForm.set(true);
  }

  public closeFeedbackForm() {
    this.showFeedbackForm.set(false);
    this.feedbackText.set('');
    this.communicationScore.set(3);
    this.problemSolvingScore.set(3);
    this.codingScore.set(3);
    this.systemDesignScore.set(3);
    this.cultureFitScore.set(3);
    this.recommendation.set(2);
  }

  public submitFeedback() {
    const interviewId = this.feedbackInterviewId();
    if (!interviewId) return;

    const payload = {
      communicationScore: this.communicationScore(),
      problemSolvingScore: this.problemSolvingScore(),
      codingScore: this.codingScore(),
      systemDesignScore: this.systemDesignScore(),
      cultureFitScore: this.cultureFitScore(),
      feedbackText: this.feedbackText(),
      recommendation: Number(this.recommendation())
    };

    this.interviewService.submitFeedback(interviewId, payload).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.showToast('Scorecard feedback submitted successfully!');
          this.closeFeedbackForm();
          if (this.selectedApplication()) {
            this.loadInterviews(this.selectedApplication()!.id);
            this.loadTimeline(this.selectedApplication()!.id);
          }
        } else {
          this.showToast(res.message || 'Failed to submit scorecard.', 'error');
        }
      },
      error: () => this.showToast('Error occurred while submitting scorecard.', 'error')
    });
  }

  public loadNotes(candidateId: string) {
    this.candidateNoteService.getNotes(candidateId, this.selectedApplication()?.id).subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.notes.set(res.data.map((n: any) => ({
            id: n.id,
            candidateId: n.candidateId,
            applicationId: n.applicationId,
            text: n.text,
            author: n.authorName,
            date: n.createdDate
          })));
        } else {
          this.notes.set([]);
        }
      },
      error: () => this.notes.set([])
    });
  }

  public addNote() {
    const text = this.noteText().trim();
    const candidate = this.selectedCandidate();
    if (!text || !candidate) return;

    const payload = {
      candidateId: candidate.id,
      applicationId: this.selectedApplication()?.id || undefined,
      text: text
    };

    this.candidateNoteService.createNote(payload).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.loadNotes(candidate.id);
          this.noteText.set('');
        }
      }
    });
  }

  public deleteNote(noteId: string) {
    const candidate = this.selectedCandidate();
    if (!candidate || !noteId) return;

    this.candidateNoteService.deleteNote(noteId).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.loadNotes(candidate.id);
        }
      }
    });
  }

  public getCandidateExperience(cand: CandidateDto): string {
    return cand.yearsOfExperience || '3+ yrs';
  }

  public getCandidateRole(cand: CandidateDto): string {
    return cand.currentTitle || 'Software Professional';
  }

  public getCandidateMatchScore(cand: CandidateDto): number {
    return cand.latestApplicationMatchScore || 0;
  }

  public getCandidateStage(cand: CandidateDto): string {
    return cand.latestApplicationStage || 'Talent Pool';
  }

  public closeDetails() {
    this.selectedCandidate.set(null);
    this.selectedApplication.set(null);
    this.parsedResumeData.set(null);
    this.activeTab.set('overview');
  }

  public getScoreColorClass(score: number): string {
    if (score >= 80) return 'text-emerald-600 dark:text-emerald-400 bg-emerald-500/10 border-emerald-500/20';
    if (score >= 60) return 'text-amber-600 dark:text-amber-400 bg-amber-500/10 border-amber-500/20';
    return 'text-red-600 dark:text-red-400 bg-red-500/10 border-red-500/20';
  }

  public getRecommendationLabel(type: number): string {
    const labels = ['Strong No Hire', 'No Hire', 'Neutral', 'Hire', 'Strong Hire'];
    return labels[type] || 'Neutral';
  }

  public getResumeFileName(path: string | undefined): string {
    if (!path) return 'resume.pdf';
    const parts = path.split('/');
    const fileNameWithGuid = parts[parts.length - 1];
    const underscoreIndex = fileNameWithGuid.indexOf('_');
    if (underscoreIndex !== -1) {
      return fileNameWithGuid.substring(underscoreIndex + 1);
    }
    return fileNameWithGuid;
  }

  // Premium Toast signal
  public toast = signal<{ message: string; type: 'success' | 'error' } | null>(null);

  public showToast(message: string, type: 'success' | 'error' = 'success') {
    this.toast.set({ message, type });
    setTimeout(() => {
      this.toast.set(null);
    }, 4000);
  }

  public downloadCandidateResume(candidate: CandidateDto) {
    if (!candidate.resumePath) {
      this.showToast('No resume file is associated with this candidate.', 'error');
      return;
    }

    this.candidateService.downloadResume(candidate.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = this.getResumeFileName(candidate.resumePath);
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        console.error('Failed to download resume', err);
        this.showToast('Failed to download the resume file.', 'error');
      }
    });
  }
}
