import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CompanyService } from '../../core/services/company.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="space-y-6 animate-fade-in font-sans pb-8 select-none relative">
      
      <!-- Toast/Status Banner -->
      @if (toast(); as t) {
        <div 
          [ngClass]="t.type === 'success' ? 'bg-emerald-500/10 border-emerald-500/30 text-emerald-600 dark:text-emerald-400' : 'bg-red-500/10 border-red-500/30 text-red-600 dark:text-red-400'"
          class="fixed bottom-6 right-6 z-50 glass-panel border px-5 py-3.5 rounded-[16px] shadow-lg flex items-center gap-3 animate-fade-in-up text-xs font-bold"
        >
          <span>{{ t.type === 'success' ? '✓' : '✗' }}</span>
          <span>{{ t.message }}</span>
        </div>
      }

      <!-- Title Header -->
      <div class="flex flex-col gap-1">
        <h2 class="text-2xl font-bold tracking-tight text-foreground">Workspace Settings</h2>
        <p class="text-xs text-muted-foreground">Manage your company profile, billing plans, audit trials, and external webhooks integrations</p>
      </div>

      <!-- Tab Navigation Buttons -->
      <div class="flex gap-2 border-b border-border pb-px">
        <button 
          (click)="activeTab.set('general')"
          [ngClass]="activeTab() === 'general' ? 'border-primary text-primary font-bold' : 'border-transparent text-muted-foreground hover:text-foreground'"
          class="px-4 py-2 border-b-2 text-xs uppercase tracking-wider transition-all cursor-pointer bg-transparent"
        >
          General & SSO
        </button>
        <button 
          (click)="activeTab.set('billing')"
          [ngClass]="activeTab() === 'billing' ? 'border-primary text-primary font-bold' : 'border-transparent text-muted-foreground hover:text-foreground'"
          class="px-4 py-2 border-b-2 text-xs uppercase tracking-wider transition-all cursor-pointer bg-transparent"
        >
          SaaS Billing & Plans
        </button>
        <button 
          (click)="activeTab.set('audit')"
          [ngClass]="activeTab() === 'audit' ? 'border-primary text-primary font-bold' : 'border-transparent text-muted-foreground hover:text-foreground'"
          class="px-4 py-2 border-b-2 text-xs uppercase tracking-wider transition-all cursor-pointer bg-transparent"
        >
          System Audit Logs
        </button>
        <button 
          (click)="activeTab.set('webhooks')"
          [ngClass]="activeTab() === 'webhooks' ? 'border-primary text-primary font-bold' : 'border-transparent text-muted-foreground hover:text-foreground'"
          class="px-4 py-2 border-b-2 text-xs uppercase tracking-wider transition-all cursor-pointer bg-transparent"
        >
          Developer Webhooks
        </button>
      </div>

      <div class="glass-panel p-8 shadow-level1">

        <!-- GENERAL TAB -->
        @if (activeTab() === 'general') {
          <div class="space-y-8 animate-fade-in">
            <!-- Section 1: General & Custom Branding -->
            <div class="space-y-6">
              <h3 class="text-sm font-black uppercase tracking-wider text-primary border-b border-border pb-2">General & Custom Branding</h3>
              
              <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div class="space-y-2">
                  <label class="text-[10px] font-bold uppercase tracking-wider text-muted-foreground">Company Name</label>
                  <input type="text" [(ngModel)]="companyName" class="input-field text-xs">
                </div>
                <div class="space-y-2">
                  <label class="text-[10px] font-bold uppercase tracking-wider text-muted-foreground">Primary Domain</label>
                  <input type="text" [(ngModel)]="companyDomain" class="input-field text-xs">
                </div>
              </div>

              <div class="grid grid-cols-1 md:grid-cols-3 gap-6 pt-2">
                <div class="space-y-2">
                  <label class="text-[10px] font-bold uppercase tracking-wider text-muted-foreground">Company Logo</label>
                  <div class="flex items-center gap-3">
                    <div class="w-10 h-10 border border-border rounded-[10px] bg-secondary/20 flex items-center justify-center overflow-hidden shrink-0">
                      <img *ngIf="logoUrl" [src]="logoUrl" class="w-full h-full object-cover">
                      <span *ngIf="!logoUrl" class="text-xs">🏢</span>
                    </div>
                    <div class="flex-1 flex flex-col gap-1.5">
                      <input type="text" [(ngModel)]="logoUrl" placeholder="https://... or Base64" class="input-field text-[11px] py-1 px-2.5">
                      <div class="flex items-center gap-2">
                        <label class="text-[9px] font-black uppercase text-violet-600 dark:text-violet-400 bg-violet-500/10 border border-violet-500/20 px-2 py-1 rounded cursor-pointer hover:bg-violet-500/20 transition-all select-none">
                          Upload Logo
                          <input type="file" (change)="onLogoSelected($event)" accept="image/*" class="hidden">
                        </label>
                        <button *ngIf="logoUrl" (click)="logoUrl = ''" class="text-[9px] font-black uppercase text-destructive hover:underline bg-transparent border-none p-0 cursor-pointer">Clear</button>
                      </div>
                    </div>
                  </div>
                </div>
                <div class="space-y-2">
                  <label class="text-[10px] font-bold uppercase tracking-wider text-muted-foreground">Primary Accent Color</label>
                  <div class="flex gap-2">
                    <input type="color" [(ngModel)]="primaryColor" class="w-10 h-9 p-1 bg-card border border-border rounded-[10px] cursor-pointer shrink-0">
                    <input type="text" [(ngModel)]="primaryColor" class="input-field text-xs flex-1">
                  </div>
                </div>
                <div class="space-y-2">
                  <label class="text-[10px] font-bold uppercase tracking-wider text-muted-foreground">Font Family</label>
                  <select [(ngModel)]="fontFamily" class="input-field text-xs appearance-none bg-[url('data:image/svg+xml;charset=utf-8,%3Csvg%20xmlns%3D%22http%3A%2F%2Fwww.w3.org%2F2000%2Fsvg%22%20viewBox%3D%220%200%2024%2024%22%20fill%3D%22none%22%20stroke%3D%22%2371717a%22%20stroke-width%3D%222%22%20stroke-linecap%3D%22round%22%20stroke-linejoin%3D%22round%22%3E%3Cpath%20d%3D%22m6%209%206%206%206-6%22%2F%3E%3C%2Fsvg%3E')] bg-[length:1.1rem] bg-[right_0.6rem_center] bg-no-repeat pr-8">
                    <option value="Inter">Inter (Default)</option>
                    <option value="Roboto">Roboto</option>
                    <option value="Outfit">Outfit</option>
                    <option value="monospace">Monospace</option>
                  </select>
                </div>
              </div>
            </div>

            <!-- Section 2: SSO -->
            <div class="space-y-6 border-t border-border pt-8">
              <div class="flex items-center justify-between border-b border-border pb-2">
                <h3 class="text-sm font-black uppercase tracking-wider text-primary">Enterprise Single Sign-On (SSO)</h3>
                <div class="flex items-center gap-2">
                  <span class="text-[10px] font-bold text-muted-foreground uppercase">Enable SSO</span>
                  <input type="checkbox" [(ngModel)]="ssoEnabled" class="w-4 h-4 rounded text-primary border-border focus:ring-primary cursor-pointer">
                </div>
              </div>

              @if (ssoEnabled) {
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6 animate-fade-in-up">
                  <div class="space-y-2">
                    <label class="text-[10px] font-bold uppercase tracking-wider text-muted-foreground">SSO Provider</label>
                    <select [(ngModel)]="ssoProvider" class="input-field text-xs">
                      <option value="OIDC">OpenID Connect (OIDC)</option>
                      <option value="SAML">SAML 2.0</option>
                      <option value="Okta">Okta Enterprise</option>
                    </select>
                  </div>
                  <div class="space-y-2">
                    <label class="text-[10px] font-bold uppercase tracking-wider text-muted-foreground">SSO Issuer URL</label>
                    <input type="text" [(ngModel)]="ssoIssuer" placeholder="https://identity.provider.com" class="input-field text-xs">
                  </div>
                  <div class="space-y-2">
                    <label class="text-[10px] font-bold uppercase tracking-wider text-muted-foreground">Client ID</label>
                    <input type="text" [(ngModel)]="ssoClientId" placeholder="client_id_value" class="input-field text-xs">
                  </div>
                  <div class="space-y-2">
                    <label class="text-[10px] font-bold uppercase tracking-wider text-muted-foreground">Redirect URL</label>
                    <input type="text" [(ngModel)]="ssoRedirectUrl" placeholder="https://localhost:4200/auth/callback" class="input-field text-xs">
                  </div>
                </div>
              }
            </div>

            <div class="flex justify-end pt-4 border-t border-border">
              <button (click)="saveSettings()" class="btn-primary py-2.5 px-8 text-xs font-bold shadow-md shadow-violet-500/10 cursor-pointer">
                Save Workspace Configuration
              </button>
            </div>
          </div>
        }

        <!-- BILLING TAB -->
        @if (activeTab() === 'billing') {
          <div class="space-y-8 animate-fade-in">
            <h3 class="text-sm font-black uppercase tracking-wider text-primary border-b border-border pb-2">Active Subscription Tiers</h3>
            
            <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
              <!-- Tier 1: Free -->
              <div 
                [ngClass]="currentPlan() === 'Free' ? 'border-primary bg-primary/5' : 'border-border bg-secondary/5'"
                class="p-6 border rounded-[16px] flex flex-col justify-between"
              >
                <div>
                  <h4 class="text-sm font-bold text-foreground">Free Tier</h4>
                  <p class="text-xs text-muted-foreground mt-1">Perfect for individual recruiters starting out.</p>
                  <div class="text-2xl font-bold text-foreground mt-4">$0 <span class="text-xs text-muted-foreground">/ month</span></div>
                  <ul class="text-[11px] text-muted-foreground space-y-2 mt-4">
                    <li>• 1 Active Job Requisition</li>
                    <li>• Basic Resume Parser</li>
                    <li>• Email Support</li>
                  </ul>
                </div>
                <button 
                  [disabled]="currentPlan() === 'Free'"
                  (click)="upgradePlan('Free')"
                  class="btn-primary mt-6 py-2 text-xs font-bold w-full justify-center cursor-pointer"
                >
                  {{ currentPlan() === 'Free' ? 'Active Plan' : 'Downgrade' }}
                </button>
              </div>

              <!-- Tier 2: Growth -->
              <div 
                [ngClass]="currentPlan() === 'Growth' ? 'border-primary bg-primary/5' : 'border-border bg-secondary/5'"
                class="p-6 border rounded-[16px] flex flex-col justify-between relative overflow-hidden"
              >
                <div class="absolute top-0 right-0 bg-primary text-white text-[9px] uppercase tracking-wider font-bold px-3 py-1 rounded-bl-[12px]">POPULAR</div>
                <div>
                  <h4 class="text-sm font-bold text-foreground">Growth Plan</h4>
                  <p class="text-xs text-muted-foreground mt-1">For expanding teams requiring automation.</p>
                  <div class="text-2xl font-bold text-foreground mt-4">$49 <span class="text-xs text-muted-foreground">/ month</span></div>
                  <ul class="text-[11px] text-muted-foreground space-y-2 mt-4">
                    <li>• 5 Active Job Requisitions</li>
                    <li>• Structured Interview Scorecards</li>
                    <li>• Basic Webhooks Integration</li>
                    <li>• Priority Email support</li>
                  </ul>
                </div>
                <button 
                  [disabled]="currentPlan() === 'Growth'"
                  (click)="upgradePlan('Growth')"
                  class="btn-primary mt-6 py-2 text-xs font-bold w-full justify-center cursor-pointer"
                >
                  {{ currentPlan() === 'Growth' ? 'Active Plan' : 'Select Plan' }}
                </button>
              </div>

              <!-- Tier 3: Enterprise -->
              <div 
                [ngClass]="currentPlan() === 'Enterprise' ? 'border-primary bg-primary/5' : 'border-border bg-secondary/5'"
                class="p-6 border rounded-[16px] flex flex-col justify-between"
              >
                <div>
                  <h4 class="text-sm font-bold text-foreground">Enterprise Plan</h4>
                  <p class="text-xs text-muted-foreground mt-1">For corporate scales requiring advanced AI.</p>
                  <div class="text-2xl font-bold text-foreground mt-4">$199 <span class="text-xs text-muted-foreground">/ month</span></div>
                  <ul class="text-[11px] text-muted-foreground space-y-2 mt-4">
                    <li>• Unlimited Requisitions</li>
                    <li>• Full SAML/OIDC SSO Gateways</li>
                    <li>• AI Resume Matcher & Scorecards</li>
                    <li>• Custom Webhooks & Public APIs</li>
                    <li>• 24/7 Phone & SLA support</li>
                  </ul>
                </div>
                <button 
                  [disabled]="currentPlan() === 'Enterprise'"
                  (click)="upgradePlan('Enterprise')"
                  class="btn-primary mt-6 py-2 text-xs font-bold w-full justify-center cursor-pointer"
                >
                  {{ currentPlan() === 'Enterprise' ? 'Active Plan' : 'Upgrade Plan' }}
                </button>
              </div>
            </div>

            <!-- Billing Limits Usage Gauge -->
            <div class="space-y-4 border-t border-border pt-8">
              <h4 class="text-xs font-bold uppercase tracking-wider text-muted-foreground">Active Usage Limits</h4>
              <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div class="space-y-1">
                  <div class="flex justify-between text-xs font-semibold">
                    <span>Job Requisitions</span>
                    <span>{{ activeJobsCount() }} / {{ getPlanJobsLimit() }}</span>
                  </div>
                  <div class="w-full bg-secondary/20 h-2 rounded">
                    <div class="bg-primary h-2 rounded" [style.width.%]="(activeJobsCount() / getPlanJobsLimit()) * 100"></div>
                  </div>
                </div>
                <div class="space-y-1">
                  <div class="flex justify-between text-xs font-semibold">
                    <span>Recruiter Seats</span>
                    <span>{{ activeUsersCount() }} / {{ getPlanRecruitersLimit() }}</span>
                  </div>
                  <div class="w-full bg-secondary/20 h-2 rounded">
                    <div class="bg-secondary h-2 rounded" [style.width.%]="(activeUsersCount() / getPlanRecruitersLimit()) * 100"></div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        }

        <!-- AUDIT LOGS TAB -->
        @if (activeTab() === 'audit') {
          <div class="space-y-6 animate-fade-in">
            <h3 class="text-sm font-black uppercase tracking-wider text-primary border-b border-border pb-2">Workspace Activity Audit Trail</h3>
            
            <div class="overflow-x-auto border border-border rounded-[12px] bg-secondary/5">
              <table class="w-full text-left text-xs border-collapse">
                <thead>
                  <tr class="bg-secondary/15 border-b border-border text-muted-foreground font-semibold">
                    <th class="p-3">User</th>
                    <th class="p-3">Event Type</th>
                    <th class="p-3">Entity Table</th>
                    <th class="p-3">Time</th>
                    <th class="p-3">Old State</th>
                    <th class="p-3">New State</th>
                  </tr>
                </thead>
                <tbody>
                  @if (auditLogs().length === 0) {
                    <tr>
                      <td colspan="6" class="p-8 text-center text-muted-foreground">No audit entries found in database.</td>
                    </tr>
                  }
                  @for (log of auditLogs(); track log.id) {
                    <tr class="border-b border-border/40 hover:bg-secondary/10 transition-colors">
                      <td class="p-3 font-semibold">{{ log.userEmail }}</td>
                      <td class="p-3">
                        <span 
                          [ngClass]="log.action === 'Added' ? 'text-emerald-500 bg-emerald-500/10' : (log.action === 'Deleted' ? 'text-rose-500 bg-rose-500/10' : 'text-amber-500 bg-amber-500/10')"
                          class="px-2.5 py-0.5 rounded text-[10px] font-bold uppercase tracking-wide"
                        >
                          {{ log.action }}
                        </span>
                      </td>
                      <td class="p-3 text-muted-foreground">{{ log.tableName }}</td>
                      <td class="p-3">{{ formatTimestamp(log.timestamp) }}</td>
                      <td class="p-3 max-w-[150px] truncate font-mono text-[10px]" [title]="log.oldValues">{{ log.oldValues || 'None' }}</td>
                      <td class="p-3 max-w-[150px] truncate font-mono text-[10px]" [title]="log.newValues">{{ log.newValues || 'None' }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>

            <!-- Pagination Toolbar -->
            @if (auditLogs().length > 0) {
              <div class="flex items-center justify-between text-xs pt-4 border-t border-border/40">
                <span class="text-muted-foreground">Showing page {{ auditPageIndex() }} of {{ auditTotalPages() }} ({{ auditTotalCount() }} total entries)</span>
                <div class="flex gap-2">
                  <button 
                    [disabled]="auditPageIndex() <= 1"
                    (click)="changeAuditPage(auditPageIndex() - 1)"
                    class="btn-secondary px-3 py-1.5 cursor-pointer text-xs"
                  >
                    Prev
                  </button>
                  <button 
                    [disabled]="auditPageIndex() >= auditTotalPages()"
                    (click)="changeAuditPage(auditPageIndex() + 1)"
                    class="btn-secondary px-3 py-1.5 cursor-pointer text-xs"
                  >
                    Next
                  </button>
                </div>
              </div>
            }
          </div>
        }

        <!-- WEBHOOKS TAB -->
        @if (activeTab() === 'webhooks') {
          <div class="space-y-6 animate-fade-in">
            <h3 class="text-sm font-black uppercase tracking-wider text-primary border-b border-border pb-2">Developer Event Webhooks</h3>
            
            <div class="glass-panel p-6 border border-border/50 bg-secondary/5 rounded-[12px] space-y-4">
              <h4 class="text-xs font-bold text-foreground">Register New Endpoint</h4>
              <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div class="space-y-1">
                  <label class="text-[10px] font-bold text-muted-foreground uppercase">Payload URL</label>
                  <input 
                    type="text" 
                    [(ngModel)]="newWebhookUrl" 
                    placeholder="https://your-api.com/webhooks" 
                    class="input-field text-xs w-full"
                  >
                </div>
                <div class="space-y-1">
                  <label class="text-[10px] font-bold text-muted-foreground uppercase">Event Trigger</label>
                  <select [(ngModel)]="newWebhookEvent" class="input-field text-xs w-full">
                    <option value="CandidateApplied">Candidate.Applied</option>
                    <option value="InterviewScheduled">Interview.Scheduled</option>
                    <option value="InterviewFeedbackSubmitted">Interview.FeedbackSubmitted</option>
                    <option value="OfferExtended">Offer.Extended</option>
                  </select>
                </div>
                <div class="flex items-end">
                  <button 
                    (click)="addWebhook()"
                    class="btn-primary py-2 px-6 text-xs font-bold w-full justify-center cursor-pointer"
                  >
                    Add Webhook Endpoint
                  </button>
                </div>
              </div>
            </div>

            <div class="space-y-4">
              <h4 class="text-xs font-bold text-foreground">Configured Endpoints</h4>
              @if (webhooks().length === 0) {
                <p class="text-xs text-muted-foreground bg-secondary/5 border border-border p-4 rounded-[12px]">No webhooks configured yet.</p>
              }
              @for (wh of webhooks(); track wh.url; let idx = $index) {
                <div class="p-4 border border-border/40 rounded-[12px] bg-secondary/10 flex items-center justify-between">
                  <div class="flex items-center gap-3">
                    <span class="text-lg">⚓</span>
                    <div class="flex flex-col">
                      <span class="text-xs font-bold text-foreground font-mono">{{ wh.url }}</span>
                      <span class="text-[9px] text-primary uppercase font-bold mt-1">Trigger: {{ wh.event }}</span>
                    </div>
                  </div>
                  <button 
                    (click)="removeWebhook(idx)"
                    class="text-xs font-bold text-rose-500 hover:underline cursor-pointer bg-transparent border-none"
                  >
                    Delete
                  </button>
                </div>
              }
            </div>
          </div>
        }

      </div>
    </div>
  `,
  styles: [`
    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(10px); }
      to { opacity: 1; transform: translateY(0); }
    }
    .animate-fade-in {
      animation: fadeIn 0.3s cubic-bezier(0.16, 1, 0.3, 1) forwards;
    }
  `]
})
export class SettingsComponent implements OnInit {
  private companyService = inject(CompanyService);

  public companyName = '';
  public companyDomain = '';
  public logoUrl = '';
  public primaryColor = '#7c3aed';
  public fontFamily = 'Inter';
  public customCss = '';
  public ssoEnabled = false;
  public ssoProvider = 'OIDC';
  public ssoIssuer = '';
  public ssoClientId = '';
  public ssoRedirectUrl = '';

  // Tab state
  public activeTab = signal<string>('general');

  // Billing states
  public currentPlan = signal<string>('Growth');
  public activeJobsCount = signal<number>(3);
  public activeUsersCount = signal<number>(2);

  // Webhook states
  public webhooks = signal<{ url: string; event: string }[]>([]);
  public newWebhookUrl = '';
  public newWebhookEvent = 'CandidateApplied';

  // Audit Logs states
  public auditLogs = signal<any[]>([]);
  public auditPageIndex = signal<number>(1);
  public auditTotalPages = signal<number>(1);
  public auditTotalCount = signal<number>(0);
  public auditPageSize = 10;

  // Premium Toast signal
  public toast = signal<{ message: string; type: 'success' | 'error' } | null>(null);

  public ngOnInit() {
    this.loadSettings();
    this.loadSourcingChannels();
    this.loadBillingInfo();
    this.loadWebhooks();
    this.loadAuditLogs();
  }

  public loadSettings() {
    this.companyService.getCompany().subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.companyName = res.data.name;
          this.companyDomain = res.data.domain;
          this.logoUrl = res.data.logoUrl || '';
          this.primaryColor = res.data.primaryColor || '#7c3aed';
          this.fontFamily = res.data.fontFamily || 'Inter';
          this.customCss = res.data.customCss || '';
          this.ssoEnabled = res.data.ssoEnabled || false;
          this.ssoProvider = res.data.ssoProvider || 'OIDC';
          this.ssoIssuer = res.data.ssoIssuer || '';
          this.ssoClientId = res.data.ssoClientId || '';
          this.ssoRedirectUrl = res.data.ssoRedirectUrl || '';
        }
      }
    });
  }

  public loadBillingInfo() {
    const plan = localStorage.getItem('billing_plan');
    if (plan) {
      this.currentPlan.set(plan);
    } else {
      localStorage.setItem('billing_plan', 'Growth');
    }
  }

  public getPlanJobsLimit(): number {
    const plan = this.currentPlan();
    if (plan === 'Free') return 1;
    if (plan === 'Growth') return 5;
    return 1000; // Unlimited
  }

  public getPlanRecruitersLimit(): number {
    const plan = this.currentPlan();
    if (plan === 'Free') return 1;
    if (plan === 'Growth') return 2;
    return 1000; // Unlimited
  }

  public upgradePlan(planName: string) {
    this.currentPlan.set(planName);
    localStorage.setItem('billing_plan', planName);
    this.showToast(`Successfully switched organization to ${planName} Plan!`);
  }

  public loadAuditLogs() {
    this.companyService.getAuditLogs(this.auditPageIndex(), this.auditPageSize).subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.auditLogs.set(res.data.items);
          this.auditTotalPages.set(res.data.totalPages);
          this.auditTotalCount.set(res.data.totalCount);
        }
      }
    });
  }

  public changeAuditPage(newPage: number) {
    if (newPage >= 1 && newPage <= this.auditTotalPages()) {
      this.auditPageIndex.set(newPage);
      this.loadAuditLogs();
    }
  }

  public formatTimestamp(ts: string): string {
    if (!ts) return '';
    const date = new Date(ts);
    return date.toLocaleString();
  }

  public loadWebhooks() {
    const saved = localStorage.getItem('webhooks_endpoints');
    if (saved) {
      try {
        this.webhooks.set(JSON.parse(saved));
      } catch {
        // Defaults
      }
    }
  }

  public addWebhook() {
    const url = this.newWebhookUrl.trim();
    if (!url) {
      this.showToast('Please enter a valid Payload URL.', 'error');
      return;
    }

    this.webhooks.update(list => {
      const copy = [...list, { url, event: this.newWebhookEvent }];
      localStorage.setItem('webhooks_endpoints', JSON.stringify(copy));
      return copy;
    });

    this.newWebhookUrl = '';
    this.showToast('Webhook registered successfully!');
  }

  public removeWebhook(idx: number) {
    this.webhooks.update(list => {
      const copy = [...list];
      copy.splice(idx, 1);
      localStorage.setItem('webhooks_endpoints', JSON.stringify(copy));
      return copy;
    });
    this.showToast('Webhook endpoint deleted.');
  }

  public onLogoSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.logoUrl = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  private loadSourcingChannels() {
    // Left for compliance
  }

  public showToast(message: string, type: 'success' | 'error' = 'success') {
    this.toast.set({ message, type });
    setTimeout(() => {
      this.toast.set(null);
    }, 4000);
  }

  public saveSettings() {
    if (!this.companyName.trim() || !this.companyDomain.trim()) {
      this.showToast('Company Name and Domain cannot be empty.', 'error');
      return;
    }

    const payload = {
      name: this.companyName,
      domain: this.companyDomain,
      logoUrl: this.logoUrl || null,
      primaryColor: this.primaryColor || null,
      fontFamily: this.fontFamily || null,
      customCss: this.customCss || null,
      ssoEnabled: this.ssoEnabled,
      ssoProvider: this.ssoEnabled ? this.ssoProvider : null,
      ssoIssuer: this.ssoEnabled ? this.ssoIssuer : null,
      ssoClientId: this.ssoEnabled ? this.ssoClientId : null,
      ssoRedirectUrl: this.ssoEnabled ? this.ssoRedirectUrl : null
    };

    this.companyService.updateCompany(payload).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.showToast('Workspace settings saved successfully.');
        } else {
          this.showToast('Failed to save settings: ' + res.message, 'error');
        }
      },
      error: () => {
        this.showToast('An error occurred while saving settings.', 'error');
      }
    });
  }
}
