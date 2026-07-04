import { Injectable } from '@angular/core';
import { Observable, of, delay } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';

export interface FunnelStageDto {
  stage: string;
  count: number;
  percentage: number;
  conversionRate?: number;
}

export interface JobMetricDto {
  jobTitle: string;
  applicantCount: number;
}

export interface MonthlyTrendDto {
  month: string;
  hiresCount: number;
  applicationsCount: number;
}

export interface DashboardMetricsDto {
  activeJobsCount: number;
  totalCandidatesCount: number;
  interviewsScheduledThisWeek: number;
  offersExtendedCount: number;
  averageTimeToHireDays: number;
  offerAcceptanceRate: number;
  hiringFunnelStages: FunnelStageDto[];
  applicantsPerJob: JobMetricDto[];
  monthlyTrends: MonthlyTrendDto[];
}

export interface UpcomingInterviewDto {
  id: string;
  candidateName: string;
  candidateAvatar?: string;
  roleTitle: string;
  roundTitle: string;
  roundType: 'technical' | 'hr' | 'general';
  scheduledTime: string;
  meetingLink?: string;
}

export interface ActivityLogDto {
  id: string;
  userName: string;
  userInitials: string;
  action: string;
  target: string;
  time: string;
  type: 'interview' | 'moved' | 'approved' | 'parsed' | 'applied';
  colorClass: string;
  statusColor: string;
}

export interface AIInsightDto {
  id: string;
  title: string;
  volumePercent: number;
  recommendation: string;
  badgeText: string;
}

export interface HiringHealthDto {
  score: number;
  status: string;
  trendText: string;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private apiUrl = 'http://localhost:5000/api/dashboard';

  constructor(private http: HttpClient) {}
  
  public getDashboardMetrics(): Observable<DashboardMetricsDto> {
    return this.http.get<any>(`${this.apiUrl}/metrics`).pipe(
      map(res => {
        if (res.isSuccess && res.data) {
          const d = res.data;
          
          const stages = ['Applied', 'Screening', 'Technical', 'Interview', 'Offer', 'Hired'];
          const funnelStages: FunnelStageDto[] = stages.map(stage => {
            let count = 0;
            if (stage === 'Applied') {
              count = d.funnelStages['Applied'] || 0;
            } else if (stage === 'Screening') {
              count = d.funnelStages['Screening'] || 0;
            } else if (stage === 'Technical') {
              count = d.funnelStages['Technical Interview'] || 0;
            } else if (stage === 'Interview') {
              count = (d.funnelStages['HR Interview'] || 0) + 
                      (d.funnelStages['Final Interview'] || 0) + 
                      (d.funnelStages['Recruiter Review'] || 0) + 
                      (d.funnelStages['Hiring Manager Review'] || 0);
            } else if (stage === 'Offer') {
              count = d.funnelStages['Offer'] || 0;
            } else if (stage === 'Hired') {
              count = d.funnelStages['Hired'] || 0;
            }
            return {
              stage,
              count,
              percentage: 100
            };
          });

          return {
            activeJobsCount: d.openPositions,
            totalCandidatesCount: d.activeCandidates,
            interviewsScheduledThisWeek: d.interviewsToday,
            offersExtendedCount: d.hiresThisMonth,
            averageTimeToHireDays: d.averageTimeToHireDays,
            offerAcceptanceRate: d.offerAcceptanceRate,
            hiringFunnelStages: funnelStages,
            applicantsPerJob: [],
            monthlyTrends: d.monthlyTrends || []
          };
        }
        throw new Error('Failed to load dashboard metrics');
      })
    );
  }

  public getUpcomingInterviews(): Observable<UpcomingInterviewDto[]> {
    return this.http.get<any>('http://localhost:5000/api/interviews').pipe(
      map(res => {
        if (res.isSuccess && res.data) {
          return res.data.map((i: any) => ({
            id: i.id,
            candidateName: i.candidateName,
            roleTitle: i.jobTitle,
            roundTitle: i.title || `${i.type} Round`,
            roundType: i.type === 0 ? 'technical' : (i.type === 1 ? 'hr' : 'general'),
            scheduledTime: i.scheduledTime,
            meetingLink: i.videoLink
          }));
        }
        return [];
      })
    );
  }

  public getRecentActivity(): Observable<ActivityLogDto[]> {
    return this.http.get<any>('http://localhost:5000/api/notifications').pipe(
      map(res => {
        if (res.isSuccess && res.data) {
          return res.data.map((n: any) => {
            let userName = 'System';
            let userInitials = 'SYS';
            
            const byIndex = n.message.lastIndexOf(' by ');
            if (byIndex !== -1) {
              const name = n.message.substring(byIndex + 4).replace('.', '').trim();
              if (name) {
                userName = name;
                const parts = name.split(' ');
                userInitials = parts.map((p: string) => p.charAt(0)).join('').toUpperCase().substring(0, 2);
              }
            }

            return {
              id: n.id,
              userName: userName,
              userInitials: userInitials,
              action: n.title,
              target: n.message.split(' was moved')[0].split(' accepted')[0].split(' scheduled to interview ')[0] || n.message,
              time: new Date(n.createdDate).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
              type: n.title.includes('Interview') ? 'interview' : (n.title.includes('Offer') ? 'approved' : 'moved'),
              colorClass: n.title.includes('Interview') ? 'bg-violet-500/10 text-violet-600 border-violet-500/20' : (n.title.includes('Offer') ? 'bg-emerald-500/10 text-emerald-600 border-emerald-500/20' : 'bg-sky-500/10 text-sky-600 border-sky-500/20'),
              statusColor: n.title.includes('Interview') ? 'bg-violet-500' : (n.title.includes('Offer') ? 'bg-emerald-500' : 'bg-sky-500')
            };
          });
        }
        return [];
      })
    );
  }

  public getAIInsights(): Observable<AIInsightDto> {
    const mockInsight: AIInsightDto = {
      id: '1',
      title: 'Frontend Engineer requisition',
      volumePercent: 32,
      recommendation: 'Schedule two additional screening sessions this week.',
      badgeText: 'New'
    };

    return of(mockInsight).pipe(delay(100));
  }

  public getHiringHealth(): Observable<HiringHealthDto> {
    const mockHealth: HiringHealthDto = {
      score: 92,
      status: 'Excellent',
      trendText: '↑ 8% from last month'
    };

    return of(mockHealth).pipe(delay(100));
  }
}
