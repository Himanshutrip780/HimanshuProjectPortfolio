import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { InterviewService, InterviewDto } from '../../core/services/interview.service';
import { ApplicationService, ApplicationDto } from '../../core/services/application.service';
import { JobService } from '../../core/services/job.service';
import { AuthService } from '../../core/services/auth.service';
import { FullCalendarModule } from '@fullcalendar/angular';
import { CalendarOptions, EventClickArg } from '@fullcalendar/core';
import dayGridPlugin from '@fullcalendar/daygrid';
import interactionPlugin from '@fullcalendar/interaction';

@Component({
  selector: 'app-interviews',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    FullCalendarModule
  ],
  templateUrl: './interviews.component.html',
  styleUrls: ['./interviews.component.scss']
})
export class InterviewsComponent implements OnInit {
  private interviewService = inject(InterviewService);
  private applicationService = inject(ApplicationService);
  private jobService = inject(JobService);
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);

  public canManageInterviews = computed(() => {
    const role = this.authService.currentUser()?.role;
    return role === 'SuperAdmin' || role === 'Recruiter';
  });

  public interviews = signal<InterviewDto[]>([]);
  public applications = signal<ApplicationDto[]>([]);
  public loading = signal<boolean>(true);
  public showScheduleForm = signal<boolean>(false);
  public selectedInterview = signal<InterviewDto | null>(null);
  
  public scheduleForm!: FormGroup;
  public feedbackForm!: FormGroup;
  public submitting = signal<boolean>(false);
  public submittingFeedback = signal<boolean>(false);

  public interviewers: { id: string, name: string }[] = [];

  // FullCalendar Options
  public calendarOptions = signal<CalendarOptions>({
    plugins: [dayGridPlugin, interactionPlugin],
    initialView: 'dayGridMonth',
    headerToolbar: {
      left: 'prev,next today',
      center: 'title',
      right: 'dayGridMonth'
    },
    editable: false,
    selectable: true,
    events: [],
    eventClick: this.handleEventClick.bind(this)
  });

  constructor() {
    this.initForms();
  }

  public ngOnInit() {
    this.loadInterviews();
    this.loadApplications();
    this.loadInterviewers();
  }

  private initForms() {
    this.scheduleForm = this.fb.group({
      applicationId: ['', [Validators.required]],
      interviewerId: ['', [Validators.required]],
      scheduledTime: ['', [Validators.required]],
      durationMinutes: [45, [Validators.required]],
      meetingLink: ['https://meet.google.com/abc-defg-hij', [Validators.required]],
      notes: ['']
    });

    this.feedbackForm = this.fb.group({
      communicationScore: [3, [Validators.required]],
      problemSolvingScore: [3, [Validators.required]],
      codingScore: [3, [Validators.required]],
      systemDesignScore: [3, [Validators.required]],
      cultureFitScore: [3, [Validators.required]],
      feedbackText: ['', [Validators.required]],
      recommendation: [1, [Validators.required]] // Shortlist (1)
    });
  }

  public loadInterviewers() {
    this.jobService.getUsers('Interviewer').subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.interviewers = res.data.map((u: any) => ({
            id: u.id,
            name: `${u.firstName} ${u.lastName}`
          }));
          if (this.interviewers.length > 0) {
            this.scheduleForm.patchValue({
              interviewerId: this.interviewers[0].id
            });
          }
        }
      }
    });
  }

  public loadInterviews() {
    this.loading.set(true);
    this.interviewService.getInterviews().subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          const list = res.data as InterviewDto[];
          this.interviews.set(list);
          this.updateCalendarEvents(list);
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  private loadApplications() {
    this.applicationService.getApplications().subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.applications.set(res.data);
        }
      }
    });
  }

  private updateCalendarEvents(interviewsList: InterviewDto[]) {
    const events = interviewsList.map(int => {
      let statusClass = 'status-scheduled';
      if (int.status === 1) statusClass = 'status-completed';
      else if (int.status === 2) statusClass = 'status-cancelled';
      else if (int.status === 4) statusClass = 'status-rescheduled';

      return {
        id: int.id,
        title: `${int.candidateName} - ${int.jobTitle}`,
        start: int.scheduledTime,
        classNames: [statusClass],
        color: int.status === 2 ? '#ef4444' : (int.status === 1 ? '#10b981' : (int.status === 4 ? '#f59e0b' : '#4f46e5'))
      };
    });

    this.calendarOptions.update(options => ({
      ...options,
      events: events
    }));
  }

  public toggleScheduleForm() {
    this.showScheduleForm.update(val => !val);
    if (this.showScheduleForm()) {
      this.selectedInterview.set(null);
    }
  }

  public handleEventClick(clickInfo: EventClickArg) {
    const eventId = clickInfo.event.id;
    const interview = this.interviews().find(i => i.id === eventId);
    if (interview) {
      this.selectedInterview.set(interview);
      this.showScheduleForm.set(false);
      this.showRescheduleForm.set(false);
      
      // Reset feedback form
      this.feedbackForm.reset({
        communicationScore: 3,
        problemSolvingScore: 3,
        codingScore: 3,
        systemDesignScore: 3,
        cultureFitScore: 3,
        feedbackText: '',
        recommendation: 1
      });
    }
  }

  public closeDetails() {
    this.selectedInterview.set(null);
    this.showRescheduleForm.set(false);
  }

  public showRescheduleForm = signal<boolean>(false);
  public newScheduledTime = '';
  public newDurationMinutes = 45;

  public enableReschedule() {
    const interview = this.selectedInterview();
    if (interview) {
      this.showRescheduleForm.set(true);
      const d = new Date(interview.scheduledTime);
      const pad = (n: number) => n.toString().padStart(2, '0');
      this.newScheduledTime = `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
      this.newDurationMinutes = interview.durationMinutes;
    }
  }

  public disableReschedule() {
    this.showRescheduleForm.set(false);
  }

  public onCancelInterview() {
    const interview = this.selectedInterview();
    if (!interview) return;
    this.interviewService.cancelInterview(interview.id).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.loadInterviews();
          this.closeDetails();
        }
      }
    });
  }

  public onRescheduleInterview() {
    const interview = this.selectedInterview();
    if (!interview || !this.newScheduledTime) return;

    const payload = {
      scheduledTime: new Date(this.newScheduledTime).toISOString(),
      durationMinutes: this.newDurationMinutes
    };

    this.interviewService.rescheduleInterview(interview.id, payload).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.loadInterviews();
          this.closeDetails();
        }
      }
    });
  }

  public onSchedule() {
    if (this.scheduleForm.invalid) return;
    this.submitting.set(true);

    const formVal = this.scheduleForm.value;
    
    // Convert date input to ISO string
    const payload = {
      ...formVal,
      scheduledTime: new Date(formVal.scheduledTime).toISOString()
    };

    this.interviewService.scheduleInterview(payload).subscribe({
      next: (res) => {
        this.submitting.set(false);
        if (res.isSuccess) {
          this.loadInterviews();
          this.showScheduleForm.set(false);
        }
      },
      error: () => this.submitting.set(false)
    });
  }

  public onSubmitFeedback() {
    if (this.feedbackForm.invalid) return;
    this.submittingFeedback.set(true);

    const interview = this.selectedInterview();
    if (!interview) return;

    this.interviewService.submitFeedback(interview.id, this.feedbackForm.value).subscribe({
      next: (res) => {
        this.submittingFeedback.set(false);
        if (res.isSuccess) {
          this.loadInterviews();
          this.selectedInterview.set(null);
        }
      },
      error: () => this.submittingFeedback.set(false)
    });
  }

  public getStatusLabel(status: number): string {
    switch (status) {
      case 0: return 'Scheduled';
      case 1: return 'Completed';
      case 2: return 'Cancelled';
      case 3: return 'NoShow';
      case 4: return 'Rescheduled';
      default: return 'Scheduled';
    }
  }

  public getRecommendationLabel(rec: number): string {
    switch (rec) {
      case 0: return 'Strong Hire';
      case 1: return 'Hire';
      case 2: return 'No Hire';
      case 3: return 'Strong No Hire';
      default: return 'No Decision';
    }
  }
}
