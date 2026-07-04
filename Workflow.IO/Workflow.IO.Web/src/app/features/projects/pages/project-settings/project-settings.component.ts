import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { getApiErrorMessage } from '../../../../core/utils/api-error.util';
import { ProjectService } from '../../services/project.service';
import { TaskService } from '../../../tasks/services/task.service';
import { UserService } from '../../../../core/services/user.service';

@Component({
  selector: 'app-project-settings',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <div class="settings-container">
      <header class="page-header">
        <div class="header-main">
          <span class="material-symbols-outlined header-icon">settings</span>
          <div>
            <h1>Project Settings</h1>
            <p class="subtitle">Update project metadata, configure defaults, or archive/delete the project workspace.</p>
          </div>
        </div>
      </header>

      @if (message()) {
        <div class="status-banner success">
          <span class="material-symbols-outlined">check_circle</span>
          <span>{{ message() }}</span>
        </div>
      }
      @if (error()) {
        <div class="status-banner error">
          <span class="material-symbols-outlined">error</span>
          <span>{{ error() }}</span>
        </div>
      }

      <div class="settings-content">
        <section class="panel form-panel">
          <h3>General Information</h3>
          <p class="subtitle" style="font-size: 0.8rem; margin-bottom: 1.25rem;">Update basic details such as the project display name and long-form description.</p>
          
          <form [formGroup]="form" (ngSubmit)="save()" class="settings-form">
            <div class="form-group">
              <label for="proj-name">Project Name</label>
              <input id="proj-name" type="text" formControlName="name" placeholder="e.g. Zensar Platform" />
            </div>

            <div class="form-group">
              <label for="proj-desc">Description</label>
              <textarea id="proj-desc" formControlName="description" rows="4" placeholder="Brief summary of the project goals, tools, or team details..."></textarea>
            </div>

            <div class="form-actions">
              <button type="submit" [disabled]="saving() || form.invalid" class="btn btn-primary">
                @if (saving()) {
                  <span class="spinner-sm"></span> Saving...
                } @else {
                  <span class="material-symbols-outlined" style="font-size: 1.15rem;">save</span> Save Changes
                }
              </button>
            </div>
          </form>
        </section>

        <section class="panel form-panel">
          <h3>Workspace Automation Rules</h3>
          <p class="subtitle" style="font-size: 0.8rem; margin-bottom: 1.25rem;">Define triggers and automatic actions to optimize team workflows.</p>

          <!-- List Rules -->
          <div class="rules-list">
            @for (rule of rules(); track rule.automationRuleId) {
              <div class="rule-item">
                <div class="rule-info">
                  <span class="rule-name">{{ rule.name }}</span>
                  <span class="rule-desc">
                    When status becomes <strong>{{ getStatusLabel(rule.triggerValue) }}</strong>, 
                    do <strong>{{ rule.actionType }}</strong> ({{ getActionValueLabel(rule.actionType, rule.actionValue) }})
                  </span>
                </div>
                <div class="rule-actions">
                  <label class="toggle-switch">
                    <input type="checkbox" [checked]="rule.isEnabled" (change)="toggleRule(rule.automationRuleId, $any($event.target).checked)" />
                    <span class="toggle-slider"></span>
                  </label>
                  <button type="button" class="btn-delete-rule" (click)="deleteRule(rule.automationRuleId)" title="Delete Rule">
                    <span class="material-symbols-outlined">delete</span>
                  </button>
                </div>
              </div>
            } @empty {
              <p style="font-size: 0.8rem; color: var(--text-muted); text-align: center; padding: 1rem 0;">No active automation rules defined.</p>
            }
          </div>

          <!-- Add Rule Trigger -->
          @if (!showAddRuleForm()) {
            <button type="button" class="btn btn-secondary" style="margin-top: 1rem; display: inline-flex; align-items: center; gap: 0.5rem;" (click)="showAddRuleForm.set(true)">
              <span class="material-symbols-outlined" style="font-size: 1.15rem;">add</span> Add Automation Rule
            </button>
          } @else {
            <form [formGroup]="ruleForm" (ngSubmit)="createRule()" class="create-rule-form" style="margin-top: 1.5rem; display: flex; flex-direction: column; gap: 1rem; padding: 1.25rem; border: 1px solid var(--border-color); border-radius: var(--radius-lg); background-color: var(--bg-hover);">
              <div class="form-group">
                <label for="rule-name">Rule Name</label>
                <input id="rule-name" type="text" formControlName="name" placeholder="e.g. Reassign on Completed" style="padding: 0.55rem; border: 1px solid var(--border-color); border-radius: var(--radius-md); background-color: var(--bg-card); color: var(--text-primary);" />
              </div>

              <div class="form-group">
                <label for="rule-trigger-type">Trigger Type</label>
                <select id="rule-trigger-type" formControlName="triggerType" style="padding: 0.55rem; border: 1px solid var(--border-color); border-radius: var(--radius-md); background-color: var(--bg-card); color: var(--text-primary);">
                  <option value="StatusChanged">When Task Status Changes</option>
                </select>
              </div>

              <div class="form-group">
                <label for="rule-trigger-val">Target Status</label>
                <select id="rule-trigger-val" formControlName="triggerValue" style="padding: 0.55rem; border: 1px solid var(--border-color); border-radius: var(--radius-md); background-color: var(--bg-card); color: var(--text-primary);">
                  <option value="0">Backlog</option>
                  <option value="1">Selected For Development</option>
                  <option value="2">In Progress</option>
                  <option value="3">QA</option>
                  <option value="4">Completed</option>
                </select>
              </div>

              <div class="form-group">
                <label for="rule-action-type">Action Type</label>
                <select id="rule-action-type" formControlName="actionType" style="padding: 0.55rem; border: 1px solid var(--border-color); border-radius: var(--radius-md); background-color: var(--bg-card); color: var(--text-primary);">
                  <option value="ReassignTask">Assign Task to Member</option>
                  <option value="SendWebhook">Send Webhook Event</option>
                  <option value="LogEvent">Log System Audit Trace</option>
                </select>
              </div>

              <div class="form-group">
                <label for="rule-action-val">Action Value</label>
                @if (ruleForm.get('actionType')?.value === 'ReassignTask') {
                  <select id="rule-action-val" formControlName="actionValue" style="padding: 0.55rem; border: 1px solid var(--border-color); border-radius: var(--radius-md); background-color: var(--bg-card); color: var(--text-primary);">
                    <option value="">-- Select Member --</option>
                    @for (m of members(); track m.userId) {
                      <option [value]="m.userId">{{ m.label }}</option>
                    }
                  </select>
                } @else {
                  <input id="rule-action-val" type="text" formControlName="actionValue" placeholder="e.g. webhook URL or log tag" style="padding: 0.55rem; border: 1px solid var(--border-color); border-radius: var(--radius-md); background-color: var(--bg-card); color: var(--text-primary);" />
                }
              </div>

              <div class="form-actions" style="display: flex; gap: 0.75rem; margin-top: 0.5rem;">
                <button type="submit" [disabled]="creatingRule() || ruleForm.invalid" class="btn btn-primary" style="padding: 0.45rem 0.9rem; font-size: 0.8rem; font-weight: 600; border-radius: var(--radius-md); background-color: var(--primary-color); color: #fff; border: none; cursor: pointer;">
                  @if (creatingRule()) {
                    <span class="spinner-sm"></span> Creating...
                  } @else {
                    Create Rule
                  }
                </button>
                <button type="button" class="btn btn-secondary" style="padding: 0.45rem 0.9rem; font-size: 0.8rem; font-weight: 600; border-radius: var(--radius-md); background-color: var(--bg-panel); color: var(--text-primary); border: 1px solid var(--border-color); cursor: pointer;" (click)="cancelAddRule()">
                  Cancel
                </button>
              </div>
            </form>
          }
        </section>

        <section class="panel danger-panel">
          <div class="danger-header">
            <span class="material-symbols-outlined danger-icon">warning</span>
            <div>
              <h3>Danger Zone</h3>
              <p class="subtitle" style="font-size: 0.8rem; color: rgba(255, 255, 255, 0.65);">Irreversible workspace administrative actions.</p>
            </div>
          </div>
          
          <p class="danger-warning">Archiving makes the project read-only. Deleting permanently removes all columns, issues, velocity logs, and metadata. Proceed with extreme caution.</p>

          <div class="danger-actions">
            <button type="button" class="btn-warn" (click)="archive()">
              <span class="material-symbols-outlined" style="font-size: 1.1rem;">archive</span> Archive Project
            </button>
            <button type="button" class="btn-danger" (click)="remove()">
              <span class="material-symbols-outlined" style="font-size: 1.1rem;">delete_forever</span> Delete Project
            </button>
          </div>
        </section>
      </div>
    </div>
  `,
  styles: `
    .settings-container {
      padding: 2rem;
      max-width: 650px;
      margin: 0 auto;
      display: flex;
      flex-direction: column;
      gap: 2rem;
    }

    .page-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
    }

    .header-main {
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .header-icon {
      font-size: 2.5rem;
      color: var(--primary-color);
      background: var(--primary-glow);
      padding: 0.5rem;
      border-radius: var(--radius-lg);
    }

    .subtitle {
      color: var(--text-secondary);
      font-size: 0.95rem;
      margin-top: 0.25rem;
    }

    .settings-content {
      display: flex;
      flex-direction: column;
      gap: 2rem;
    }

    .form-panel h3 {
      font-size: 1.15rem;
      font-weight: 600;
      color: var(--text-primary);
      margin: 0;
    }

    .settings-form {
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
    }

    .form-group label {
      font-size: 0.85rem;
      font-weight: 600;
      color: var(--text-secondary);
    }

    .form-actions {
      display: flex;
      justify-content: flex-start;
      margin-top: 0.5rem;
    }

    /* Danger Zone card styling */
    .danger-panel {
      border: 1px solid rgba(239, 68, 68, 0.25) !important;
      background: linear-gradient(135deg, var(--bg-panel) 0%, rgba(239, 68, 68, 0.03) 100%) !important;
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
      box-shadow: 0 4px 20px rgba(239, 68, 68, 0.05);
    }

    .danger-header {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }

    .danger-icon {
      font-size: 2rem;
      color: var(--danger-color);
      background: rgba(239, 68, 68, 0.1);
      padding: 0.4rem;
      border-radius: var(--radius-md);
    }

    .danger-panel h3 {
      font-size: 1.15rem;
      font-weight: 700;
      color: var(--danger-color);
      margin: 0;
    }

    .danger-warning {
      font-size: 0.85rem;
      line-height: 1.5;
      color: var(--text-secondary);
      margin: 0;
    }

    .danger-actions {
      display: flex;
      flex-wrap: wrap;
      gap: 0.75rem;
      margin-top: 0.5rem;
    }

    .btn-warn {
      background-color: #d97706;
      color: white;
      border: none;
      padding: 0.55rem 1.15rem;
      border-radius: var(--radius-md);
      font-size: 0.9rem;
      font-weight: 600;
      cursor: pointer;
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      transition: background-color var(--transition-fast), transform var(--transition-fast);
    }

    .btn-warn:hover {
      background-color: #b45309;
      transform: translateY(-1px);
    }

    .btn-danger {
      background-color: var(--danger-color);
      color: white;
      border: none;
      padding: 0.55rem 1.15rem;
      border-radius: var(--radius-md);
      font-size: 0.9rem;
      font-weight: 600;
      cursor: pointer;
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      transition: background-color var(--transition-fast), transform var(--transition-fast);
    }

    .btn-danger:hover {
      background-color: #b91c1c;
      transform: translateY(-1px);
    }

    .status-banner {
      display: flex;
      align-items: center;
      gap: 0.65rem;
      padding: 0.75rem 1rem;
      border-radius: var(--radius-md);
      font-size: 0.875rem;
    }

    .status-banner.success {
      background: rgba(34, 197, 94, 0.1);
      color: #10b981;
      border: 1px solid rgba(34, 197, 94, 0.2);
    }

    .status-banner.error {
      background: rgba(239, 68, 68, 0.1);
      color: var(--danger-color);
      border: 1px solid rgba(239, 68, 68, 0.2);
    }

    .spinner-sm {
      width: 14px;
      height: 14px;
      border: 2px solid rgba(255, 255, 255, 0.3);
      border-top-color: white;
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
      display: inline-block;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    /* Automation rules styles */
    .rules-list {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
      margin-top: 1rem;
    }
    .rule-item {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0.75rem 1rem;
      background-color: var(--bg-hover);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
    }
    .rule-info {
      display: flex;
      flex-direction: column;
      gap: 0.15rem;
    }
    .rule-name {
      font-weight: 600;
      font-size: 0.875rem;
      color: var(--text-primary);
    }
    .rule-desc {
      font-size: 0.75rem;
      color: var(--text-secondary);
    }
    .rule-actions {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }
    .toggle-switch {
      position: relative;
      display: inline-block;
      width: 36px;
      height: 20px;
    }
    .toggle-switch input {
      opacity: 0;
      width: 0;
      height: 0;
    }
    .toggle-slider {
      position: absolute;
      cursor: pointer;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background-color: var(--border-color);
      transition: .3s;
      border-radius: 20px;
    }
    .toggle-slider:before {
      position: absolute;
      content: "";
      height: 14px;
      width: 14px;
      left: 3px;
      bottom: 3px;
      background-color: white;
      transition: .3s;
      border-radius: 50%;
    }
    input:checked + .toggle-slider {
      background-color: var(--primary-color);
    }
    input:checked + .toggle-slider:before {
      transform: translateX(16px);
    }
    .btn-delete-rule {
      background: transparent;
      border: none;
      color: var(--text-muted);
      cursor: pointer;
      display: flex;
      align-items: center;
      padding: 0.25rem;
      border-radius: var(--radius-sm);
      transition: color 0.2s;
    }
    .btn-delete-rule:hover {
      color: var(--danger-color);
    }
  `,
})
export class ProjectSettingsComponent implements OnInit {
  private readonly projectService = inject(ProjectService);
  private readonly taskService = inject(TaskService);
  private readonly userService = inject(UserService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly saving = signal(false);
  readonly message = signal<string | null>(null);
  readonly error = signal<string | null>(null);

  readonly rules = signal<any[]>([]);
  readonly members = signal<{ userId: string; label: string }[]>([]);
  readonly showAddRuleForm = signal(false);
  readonly creatingRule = signal(false);

  readonly form = this.fb.group({
    name: ['', Validators.required],
    description: [''],
  });

  readonly ruleForm = this.fb.group({
    name: ['', Validators.required],
    triggerType: ['StatusChanged', Validators.required],
    triggerValue: ['4', Validators.required],
    actionType: ['ReassignTask', Validators.required],
    actionValue: ['', Validators.required],
  });

  ngOnInit(): void {
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    if (!projectId) {
      return;
    }

    this.projectService
      .getProjectById(projectId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (p) =>
          this.form.patchValue({
            name: p.name,
            description: p.description ?? '',
          }),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });

    this.loadRulesAndMembers(projectId);

    this.ruleForm.get('actionType')?.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.ruleForm.patchValue({ actionValue: '' });
      });
  }

  private loadRulesAndMembers(projectId: string): void {
    this.taskService.getAutomationRules(projectId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (rules) => {
          // Handle response mapping
          const mappedRules = Array.isArray(rules) ? rules : ((rules as any).data || []);
          this.rules.set(mappedRules);
        },
        error: (err) => console.error('Failed to load rules', err)
      });

    this.projectService.getMembers(projectId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (members) => {
          if (!members || members.length === 0) {
            this.members.set([]);
            return;
          }
          const obs = members.map(m => 
            this.userService.getUserById(m.userId).pipe(
              catchError(() => of({ userId: m.userId, firstName: '', lastName: m.userId } as any))
            )
          );
          forkJoin(obs).subscribe((users: any[]) => {
            this.members.set(users.map(u => {
              const name = (u.firstName || u.lastName)
                ? `${u.firstName || ''} ${u.lastName || ''}`.trim()
                : u.userId;
              return {
                userId: u.userId,
                label: name
              };
            }));
          });
        },
        error: (err) => console.error('Failed to load members', err)
      });
  }

  save(): void {
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    if (!projectId || this.form.invalid) {
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    this.message.set(null);
    const raw = this.form.getRawValue();
    this.projectService
      .updateProject(projectId, {
        name: raw.name,
        description: raw.description || null,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.message.set('Project details saved successfully.');
          this.saving.set(false);
        },
        error: (err) => {
          this.error.set(getApiErrorMessage(err));
          this.saving.set(false);
        },
      });
  }

  createRule(): void {
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    if (!projectId || this.ruleForm.invalid) {
      return;
    }

    this.creatingRule.set(true);
    const raw = this.ruleForm.getRawValue();
    this.taskService.createAutomationRule(projectId, raw)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          const newRule = res.data || res;
          this.rules.update(list => [...list, newRule]);
          this.creatingRule.set(false);
          this.showAddRuleForm.set(false);
          this.ruleForm.reset({
            name: '',
            triggerType: 'StatusChanged',
            triggerValue: '4',
            actionType: 'ReassignTask',
            actionValue: ''
          });
        },
        error: (err) => {
          console.error('Failed to create rule', err);
          this.creatingRule.set(false);
        }
      });
  }

  deleteRule(ruleId: string): void {
    if (!confirm('Are you sure you want to delete this automation rule?')) {
      return;
    }

    this.taskService.deleteAutomationRule(ruleId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.rules.update(list => list.filter(r => r.automationRuleId !== ruleId));
        },
        error: (err) => console.error('Failed to delete rule', err)
      });
  }

  toggleRule(ruleId: string, isChecked: boolean): void {
    this.taskService.toggleAutomationRule(ruleId, isChecked)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updatedRule) => {
          const ruleData = updatedRule.data || updatedRule;
          this.rules.update(list => list.map(r => r.automationRuleId === ruleId ? { ...r, isEnabled: ruleData.isEnabled } : r));
        },
        error: (err) => {
          console.error('Failed to toggle rule', err);
          const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
          if (projectId) this.loadRulesAndMembers(projectId);
        }
      });
  }

  cancelAddRule(): void {
    this.showAddRuleForm.set(false);
    this.ruleForm.reset({
      name: '',
      triggerType: 'StatusChanged',
      triggerValue: '4',
      actionType: 'ReassignTask',
      actionValue: ''
    });
  }

  getStatusLabel(val: string): string {
    switch (val) {
      case '0': return 'Backlog';
      case '1': return 'Selected For Development';
      case '2': return 'In Progress';
      case '3': return 'QA';
      case '4': return 'Completed';
      default: return `Status ${val}`;
    }
  }

  getActionValueLabel(actionType: string, val: string): string {
    if (actionType === 'ReassignTask') {
      const member = this.members().find(m => m.userId === val);
      return member ? member.label : val;
    }
    return val;
  }

  archive(): void {
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    if (!projectId) {
      return;
    }

    this.error.set(null);
    this.message.set(null);
    this.projectService
      .archiveProject(projectId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.message.set('Project archived successfully.'),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }

  remove(): void {
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    if (!projectId || !confirm('Are you absolutely sure you want to permanently delete this project? All associated issues and settings will be lost.')) {
      return;
    }

    this.error.set(null);
    this.message.set(null);
    this.projectService
      .deleteProject(projectId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => void this.router.navigate(['/projects']),
        error: (err) => this.error.set(getApiErrorMessage(err)),
      });
  }
}
