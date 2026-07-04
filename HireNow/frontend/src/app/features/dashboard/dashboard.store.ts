import { Injectable, inject, signal, computed } from '@angular/core';
import { 
  DashboardService, 
  DashboardMetricsDto, 
  UpcomingInterviewDto, 
  ActivityLogDto, 
  AIInsightDto, 
  HiringHealthDto 
} from '../../core/services/dashboard.service';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class DashboardStore {
  private dashboardService = inject(DashboardService);
  private authService = inject(AuthService);

  // Core signals state
  public metrics = signal<DashboardMetricsDto | null>(null);
  public interviews = signal<UpcomingInterviewDto[]>([]);
  public activities = signal<ActivityLogDto[]>([]);
  public insight = signal<AIInsightDto | null>(null);
  public health = signal<HiringHealthDto | null>(null);
  
  public loading = signal<boolean>(true);
  public error = signal<string | null>(null);
  public loaded = signal<boolean>(false);

  // Computed helper signals reflecting new SaaS dashboard layouts
  public openPositionsCount = computed(() => this.metrics()?.activeJobsCount || 0);
  public activeApplicationsCount = computed(() => this.metrics()?.totalCandidatesCount || 0);
  public meanTimeToFillDays = computed(() => this.metrics()?.averageTimeToHireDays ?? 18);
  public offerAcceptanceRate = computed(() => this.metrics()?.offerAcceptanceRate ?? 92);
  public funnelStages = computed(() => {
    const stages = this.metrics()?.hiringFunnelStages || [];
    const total = stages.reduce((acc, curr) => acc + curr.count, 0) || 1;
    return stages.map(s => ({
      ...s,
      percentage: Math.round((s.count / total) * 100)
    }));
  });
  public overallConversionRate = computed(() => {
    const stages = this.funnelStages();
    const hired = stages.find(s => s.stage === 'Hired')?.count || 0;
    const total = stages.reduce((acc, curr) => acc + curr.count, 0) || 1;
    return Math.round((hired / total) * 1000) / 10 || 8.3;
  });
  public applicantsPerJob = computed(() => this.metrics()?.applicantsPerJob || []);
  public monthlyTrends = computed(() => this.metrics()?.monthlyTrends || []);

  public todayInterviews = computed(() => {
    const today = new Date();
    const year = today.getFullYear();
    const month = today.getMonth();
    const date = today.getDate();
    return this.interviews().filter(i => {
      if (!i.scheduledTime) return false;
      const d = new Date(i.scheduledTime);
      return d.getFullYear() === year && d.getMonth() === month && d.getDate() === date;
    });
  });

  public interviewsCount = computed(() => this.todayInterviews().length);
  public pendingOffersCount = computed(() => this.metrics()?.offersExtendedCount || 0);
  public urgentCandidatesCount = computed(() => this.funnelStages().find(s => s.stage === 'Screening')?.count || 0);

  public loadAll() {
    this.loading.set(true);
    this.error.set(null);

    forkJoin({
      metrics: this.dashboardService.getDashboardMetrics(),
      interviews: this.dashboardService.getUpcomingInterviews(),
      activities: this.dashboardService.getRecentActivity(),
      insight: this.dashboardService.getAIInsights(),
      health: this.dashboardService.getHiringHealth()
    }).subscribe({
      next: (data) => {
        this.metrics.set(data.metrics);
        this.interviews.set(data.interviews);
        this.activities.set(data.activities);
        this.insight.set(data.insight);
        this.health.set(data.health);
        
        this.loading.set(false);
        this.loaded.set(true);
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to load recruiting metrics');
        this.loading.set(false);
      }
    });
  }

  public dismissInsight() {
    this.insight.set(null);
  }

  // Mutator actions to enable dynamic functionality
  public addJob(title: string, department: string, manager: string) {
    const currentMetrics = this.metrics();
    if (currentMetrics) {
      // 1. Increment activeJobsCount (Open Positions)
      const updatedMetrics = {
        ...currentMetrics,
        activeJobsCount: currentMetrics.activeJobsCount + 1,
        totalCandidatesCount: currentMetrics.totalCandidatesCount + 12, // add mock applications
        applicantsPerJob: [
          { jobTitle: title, applicantCount: 12 },
          ...currentMetrics.applicantsPerJob
        ]
      };
      this.metrics.set(updatedMetrics);

      // 2. Insert timeline activity
      const user = this.authService.currentUser();
      const userName = user ? `${user.firstName} ${user.lastName}` : 'Himanshu Tripathi';
      const initials = user ? `${user.firstName.charAt(0)}${user.lastName.charAt(0)}` : 'HT';

      const newActivity: ActivityLogDto = {
        id: Math.random().toString(),
        userName: userName,
        userInitials: initials,
        action: 'created new position requisition',
        target: title,
        time: 'Just now',
        type: 'moved',
        colorClass: 'bg-violet-500/10 text-violet-600 border-violet-500/20',
        statusColor: 'bg-violet-500'
      };
      this.activities.update(list => [newActivity, ...list]);
    }
  }

  public completeInterview(id: string) {
    const interview = this.interviews().find(i => i.id === id);
    if (!interview) return;

    // 1. Remove from upcoming list
    this.interviews.update(list => list.filter(i => i.id !== id));

    // 2. Add activity log
    const user = this.authService.currentUser();
    const userName = user ? `${user.firstName} ${user.lastName}` : 'Himanshu Tripathi';
    const initials = user ? `${user.firstName.charAt(0)}${user.lastName.charAt(0)}` : 'HT';

    const newActivity: ActivityLogDto = {
      id: Math.random().toString(),
      userName: userName,
      userInitials: initials,
      action: 'completed interview round evaluation for',
      target: interview.candidateName,
      time: 'Just now',
      type: 'interview',
      colorClass: 'bg-emerald-500/10 text-emerald-600 border-emerald-500/20',
      statusColor: 'bg-emerald-500'
    };
    this.activities.update(list => [newActivity, ...list]);

    // 3. Update funnel conversions reactively
    const currentMetrics = this.metrics();
    if (currentMetrics) {
      const updatedFunnel = currentMetrics.hiringFunnelStages.map(stage => {
        if (stage.stage === 'Interview') {
          return { ...stage, count: Math.max(0, stage.count - 1) };
        }
        if (stage.stage === 'Offer') {
          return { ...stage, count: stage.count + 1 };
        }
        return stage;
      });
      this.metrics.set({
        ...currentMetrics,
        hiringFunnelStages: updatedFunnel
      });
    }
  }
}
