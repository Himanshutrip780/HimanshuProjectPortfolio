import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="space-y-6 animate-fade-in-up font-sans pb-8 select-none">
      <div class="flex flex-col gap-1 select-none">
        <h2 class="text-3xl font-black tracking-tight text-foreground">Interactive Reports Center</h2>
        <p class="text-xs text-muted-foreground">Configure filters, run pipeline audits, and download executive summaries.</p>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        
        <!-- Filter Form -->
        <div class="lg:col-span-1 glass-panel p-6 space-y-5 rounded-[24px] border border-border shadow-lg flex flex-col justify-between">
          <div class="space-y-4">
            <h3 class="text-sm font-black uppercase tracking-wider text-primary">Report Configuration</h3>
            
            <!-- Report Type Selector -->
            <div class="space-y-1.5">
              <label class="text-[10px] font-bold uppercase tracking-wider text-muted-foreground block">Report Type</label>
              <select 
                [(ngModel)]="reportType" 
                (change)="resetProgress()"
                class="input-field py-2 px-3 text-xs w-full bg-background appearance-none bg-[url('data:image/svg+xml;charset=utf-8,%3Csvg%20xmlns%3D%22http%3A%2F%2Fwww.w3.org%2F2000%2Fsvg%22%20viewBox%3D%220%200%2024%2024%22%20fill%3D%22none%22%20stroke%3D%22%2371717a%22%20stroke-width%3D%222%22%20stroke-linecap%3D%22round%22%20stroke-linejoin%3D%22round%22%3E%3Cpath%20d%3D%22m6%209%206%206%206-6%22%2F%3E%3C%2Fsvg%3E')] bg-[length:1.1rem] bg-[right_0.6rem_center] bg-no-repeat pr-8"
              >
                <option value="pipeline">Pipeline conversion audits</option>
                <option value="scorecard">Interviewer scorecards</option>
              </select>
            </div>

            <!-- Date Range Selector -->
            <div class="space-y-1.5">
              <label class="text-[10px] font-bold uppercase tracking-wider text-muted-foreground block">Date Range</label>
              <select 
                [(ngModel)]="dateRange" 
                (change)="resetProgress()"
                class="input-field py-2 px-3 text-xs w-full bg-background appearance-none bg-[url('data:image/svg+xml;charset=utf-8,%3Csvg%20xmlns%3D%22http%3A%2F%2Fwww.w3.org%2F2000%2Fsvg%22%20viewBox%3D%220%200%2024%2024%22%20fill%3D%22none%22%20stroke%3D%22%2371717a%22%20stroke-width%3D%222%22%20stroke-linecap%3D%22round%22%20stroke-linejoin%3D%22round%22%3E%3Cpath%20d%3D%22m6%209%206%206%206-6%22%2F%3E%3C%2Fsvg%3E')] bg-[length:1.1rem] bg-[right_0.6rem_center] bg-no-repeat pr-8"
              >
                <option value="30">Last 30 Days</option>
                <option value="90">Last 90 Days</option>
                <option value="365">Last Year</option>
                <option value="all">All Time</option>
              </select>
            </div>

            <!-- Sub Filter conditional fields -->
            @if (reportType() === 'pipeline') {
              <div class="space-y-1.5 animate-fade-in-up">
                <label class="text-[10px] font-bold uppercase tracking-wider text-muted-foreground block">Pipeline Stage</label>
                <select 
                  [(ngModel)]="subFilter" 
                  (change)="resetProgress()"
                  class="input-field py-2 px-3 text-xs w-full bg-background appearance-none bg-[url('data:image/svg+xml;charset=utf-8,%3Csvg%20xmlns%3D%22http%3A%2F%2Fwww.w3.org%2F2000%2Fsvg%22%20viewBox%3D%220%200%2024%2024%22%20fill%3D%22none%22%20stroke%3D%22%2371717a%22%20stroke-width%3D%222%22%20stroke-linecap%3D%22round%22%20stroke-linejoin%3D%22round%22%3E%3Cpath%20d%3D%22m6%209%206%206%206-6%22%2F%3E%3C%2Fsvg%3E')] bg-[length:1.1rem] bg-[right_0.6rem_center] bg-no-repeat pr-8"
                >
                  <option value="all">All Stages</option>
                  <option value="Applied">Applied</option>
                  <option value="Technical">Technical</option>
                  <option value="Hired">Hired</option>
                </select>
              </div>
            } @else {
              <div class="space-y-1.5 animate-fade-in-up">
                <label class="text-[10px] font-bold uppercase tracking-wider text-muted-foreground block">Scorecard Category</label>
                <select 
                  [(ngModel)]="subFilter" 
                  (change)="resetProgress()"
                  class="input-field py-2 px-3 text-xs w-full bg-background appearance-none bg-[url('data:image/svg+xml;charset=utf-8,%3Csvg%20xmlns%3D%22http%3A%2F%2Fwww.w3.org%2F2000%2Fsvg%22%20viewBox%3D%220%200%2024%2024%22%20fill%3D%22none%22%20stroke%3D%22%2371717a%22%20stroke-width%3D%222%22%20stroke-linecap%3D%22round%22%20stroke-linejoin%3D%22round%22%3E%3Cpath%20d%3D%22m6%209%206%206%206-6%22%2F%3E%3C%2Fsvg%3E')] bg-[length:1.1rem] bg-[right_0.6rem_center] bg-no-repeat pr-8"
                >
                  <option value="all">All Rounds</option>
                  <option value="Technical">Technical Round</option>
                  <option value="HR">HR Round</option>
                  <option value="Manager">Manager Round</option>
                </select>
              </div>
            }
          </div>

          <button 
            (click)="onGenerate()" 
            [disabled]="generating()"
            class="btn-primary transition-premium hover-scale w-full py-3 bg-violet-600 hover:bg-violet-700 text-white font-semibold rounded-xl shadow-md shadow-violet-500/20 flex items-center justify-center gap-2 cursor-pointer mt-4"
          >
            @if (generating()) {
              <span class="animate-spin border-2 border-white/30 border-t-white rounded-full w-4 h-4 shrink-0"></span>
              <span>Running Analysis...</span>
            } @else {
              <span>Generate Report</span>
            }
          </button>
        </div>

        <!-- Preview & Progress Status panel -->
        <div class="lg:col-span-2 glass-panel p-6 rounded-[24px] border border-border shadow-lg flex flex-col justify-between min-h-[350px]">
          
          <div class="space-y-4">
            <h3 class="text-sm font-black uppercase tracking-wider text-primary">Report Console</h3>
            
            @if (!generating() && !generated()) {
              <!-- Empty state -->
              <div class="flex flex-col items-center justify-center py-16 text-center text-xs text-muted-foreground space-y-2 select-none border border-dashed border-border rounded-xl bg-secondary/10">
                <svg class="w-8 h-8 text-muted-foreground/45 mx-auto mb-2" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M3 13.125C3 12.504 3.504 12 4.125 12h2.25c.621 0 1.125.504 1.125 1.125v5.25c0 .621-.504 1.125-1.125 1.125h-2.25A1.125 1.125 0 0 1 3 18.375v-5.25ZM9.75 8.625c0-.621.504-1.125 1.125-1.125h2.25c.621 0 1.125.504 1.125 1.125v9.75c0 .621-.504 1.125-1.125 1.125h-2.25a1.125 1.125 0 0 1-1.125-1.125v-9.75ZM16.5 4.125c0-.621.504-1.125 1.125-1.125h2.25c.621 0 1.125.504 1.125 1.125v14.25c0 .621-.504 1.125-1.125 1.125h-2.25a1.125 1.125 0 0 1-1.125-1.125V4.125Z"/></svg>
                <span class="font-semibold">No report generated yet</span>
                <span>Select options and click "Generate Report" to run analytics.</span>
              </div>
            }

            @if (generating()) {
              <!-- Loading Progression State -->
              <div class="space-y-5 py-8 animate-fade-in-up">
                <div class="flex items-center justify-between text-xs font-bold text-foreground">
                  <span class="flex items-center gap-2">
                    <span class="animate-pulse w-2 h-2 rounded-full bg-violet-500"></span>
                    Running Stage Audits...
                  </span>
                  <span class="font-mono text-violet-500">{{ progress() }}%</span>
                </div>
                
                <!-- Progress bar track -->
                <div class="w-full h-2 rounded-full bg-secondary overflow-hidden border border-border">
                  <div class="h-full bg-violet-500 rounded-full transition-all duration-100" [style.width]="progress() + '%'"></div>
                </div>

                <p class="text-[10px] text-muted-foreground leading-relaxed italic">
                  Collating historical pipeline snapshots, calculating stage transition conversion ratios, and preparing tabular structures...
                </p>
              </div>
            }

            @if (generated() && !generating()) {
              <!-- Generated State and Action buttons -->
              <div class="space-y-4 animate-fade-in-up">
                <div class="p-3.5 bg-emerald-500/10 border border-emerald-500/20 text-emerald-600 dark:text-emerald-400 text-xs rounded-xl flex items-center gap-2.5 shadow-2xs font-semibold">
                  <svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75 11.25 15 15 9.75M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z"/></svg>
                  <span>Report analysis compiled successfully!</span>
                </div>

                <!-- Structured mock data layout to make it look premium -->
                <div class="border border-border rounded-xl overflow-hidden bg-secondary/20">
                  <div class="bg-secondary/50 px-4 py-2 border-b border-border/80 text-[10px] font-bold uppercase tracking-wider text-muted-foreground flex justify-between">
                    <span>Report Details</span>
                    <span>Status: Cached</span>
                  </div>
                  <div class="p-4 space-y-2 text-xs">
                    <div class="flex justify-between border-b border-border/40 pb-2">
                      <span class="text-muted-foreground">Type:</span>
                      <strong class="text-foreground capitalize">{{ reportType() }} conversion audit</strong>
                    </div>
                    <div class="flex justify-between border-b border-border/40 pb-2">
                      <span class="text-muted-foreground">Range:</span>
                      <strong class="text-foreground">Last {{ dateRange() }} Days</strong>
                    </div>
                    <div class="flex justify-between border-b border-border/40 pb-2">
                      <span class="text-muted-foreground">Subfilter:</span>
                      <strong class="text-foreground capitalize">{{ subFilter() }}</strong>
                    </div>
                    <div class="flex justify-between">
                      <span class="text-muted-foreground">File Size:</span>
                      <strong class="text-foreground">{{ reportType() === 'pipeline' ? '124.5 KB' : '2.1 MB' }}</strong>
                    </div>
                  </div>
                </div>

                <!-- Success Alert when downloaded -->
                @if (downloaded()) {
                  <div class="p-3 bg-primary/10 border border-primary/20 text-primary text-xs rounded-xl flex items-center gap-2.5 shadow-2xs font-semibold animate-fade-in-up">
                    <svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M12 9v6m3-3H9m12 0a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z"/></svg>
                    <span>Download complete: <strong>HireNow_{{ reportType() }}_report.{{ downloadType() }}</strong></span>
                  </div>
                }
              </div>
            }

          </div>

          @if (generated() && !generating()) {
            <!-- Actions Footer -->
            <div class="flex gap-3 pt-6 border-t border-border mt-4 animate-fade-in-up">
              <button 
                (click)="onDownload('pdf')"
                class="btn-secondary transition-premium hover-scale flex-1 py-2.5 text-xs font-bold flex items-center justify-center gap-2"
              >
                <svg class="w-3.5 h-3.5 text-muted-foreground" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M19.5 14.25v-2.625a3.375 3.375 0 0 0-3.375-3.375h-1.5A1.125 1.125 0 0 1 13.5 7.125v-1.5a3.375 3.375 0 0 0-3.375-3.375H8.25m0 12.75h7.5m-7.5 3H12M10.5 2.25H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 0 0-9-9Z"/></svg>
                <span>Download PDF</span>
              </button>
              <button 
                (click)="onDownload('csv')"
                class="btn-primary transition-premium hover-scale flex-1 py-2.5 text-xs font-bold flex items-center justify-center gap-2"
              >
                <svg class="w-3.5 h-3.5 text-white" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M3.75 5.25h16.5m-16.5 4.5h16.5m-16.5 4.5h16.5m-16.5 4.5h16.5m-16.5-13.5h16.5m0 0v18m-16.5-18v18"/></svg>
                <span>Export CSV</span>
              </button>
            </div>
          }
        </div>
      </div>
    </div>
  `,
  styles: [`
    @keyframes fadeInUp {
      from { opacity: 0; transform: translateY(12px); }
      to { opacity: 1; transform: translateY(0); }
    }
    .animate-fade-in-up {
      animation: fadeInUp 0.4s cubic-bezier(0.16, 1, 0.3, 1) forwards;
    }
  `]
})
export class ReportsComponent implements OnInit {
  public reportType = signal<'pipeline' | 'scorecard'>('pipeline');
  public dateRange = signal<string>('30');
  public subFilter = signal<string>('all');
  
  public generating = signal<boolean>(false);
  public progress = signal<number>(0);
  public generated = signal<boolean>(false);
  public downloaded = signal<boolean>(false);
  public downloadType = signal<'pdf' | 'csv' | null>(null);

  public ngOnInit() {}

  public resetProgress() {
    this.generated.set(false);
    this.downloaded.set(false);
    this.downloadType.set(null);
    this.progress.set(0);
  }

  public onGenerate() {
    this.resetProgress();
    this.generating.set(true);
    this.runGenerationProgress();
  }

  private runGenerationProgress() {
    const interval = setInterval(() => {
      const current = this.progress();
      if (current >= 100) {
        clearInterval(interval);
        this.generating.set(false);
        this.generated.set(true);
      } else {
        this.progress.set(current + 10);
      }
    }, 150);
  }

  public onDownload(type: 'pdf' | 'csv') {
    this.downloaded.set(true);
    this.downloadType.set(type);
    
    // Auto reset the success alert after 4 seconds
    setTimeout(() => {
      this.downloaded.set(false);
      this.downloadType.set(null);
    }, 4000);
  }
}
