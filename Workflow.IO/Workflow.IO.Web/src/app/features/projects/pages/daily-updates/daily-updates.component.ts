import { Component, inject, OnInit, signal, effect, DestroyRef } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { DatePipe } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { TaskService } from '../../../../features/tasks/services/task.service';
import { BackButtonService } from '../../../../core/services/back-button.service';
import { getApiErrorMessage } from '../../../../core/utils/api-error.util';

@Component({
  selector: 'app-daily-updates',
  standalone: true,
  imports: [ReactiveFormsModule, DatePipe],
  template: `
    <div class="page-container">
      <header class="page-header">
        <div class="header-main">
          <span class="material-symbols-outlined header-icon">mail</span>
          <div>
            <h1>Daily Sprint Updates</h1>
            <p class="subtitle">Broadcast sprint progress, velocity, and updates to leadership and project members.</p>
          </div>
        </div>
      </header>

      <div class="daily-update-panel panel">
        <div class="card-header-area">
          <span class="material-symbols-outlined broadcast-icon">campaign</span>
          <div class="header-text-block">
            <h3>Sprint Broadcast Configuration</h3>
            <p class="desc-text">Manage manual sprint email triggers and configure mailing list recipients.</p>
          </div>
        </div>

        <div class="card-body-area">
          @if (sendingEmail()) {
            <div class="spinner-container">
              <span class="material-symbols-outlined loading-spinner">sync</span>
              <span>Generating and broadcasting update email...</span>
            </div>
          } @else {
            <div class="action-row">
              <button
                type="button"
                class="btn btn-primary btn-glow"
                [disabled]="!updateState()?.allowedManualTrigger || updateState()?.isTriggeredToday"
                (click)="sendDailyUpdate()"
              >
                <span class="material-symbols-outlined" style="font-size: 1.2rem;">send</span>
                Send Daily Update Now
              </button>
              <button
                type="button"
                class="btn btn-secondary"
                (click)="openRecipientsModal()"
              >
                <span class="material-symbols-outlined" style="font-size: 1.2rem;">group_add</span>
                Manage Recipients
              </button>
            </div>

            @if (updateState()?.isTriggeredToday) {
              <div class="status-banner success">
                <span class="material-symbols-outlined">check_circle</span>
                <span>Daily update has already been broadcast today. Next auto-send scheduled at 12 PM.</span>
              </div>
            } @else {
              <div class="status-banner info">
                <span class="material-symbols-outlined">schedule</span>
                <span>Automated broadcast scheduled daily at 12 PM (working days only).</span>
              </div>
            }

            @if (updateState()?.lastSentAt) {
              <div class="last-sent">
                <span class="material-symbols-outlined">done_all</span>
                <span>Last broadcasted: {{ updateState()?.lastSentAt | date:'medium' }}</span>
              </div>
            }

            @if (sendError()) {
              <div class="status-banner error">
                <span class="material-symbols-outlined">error</span>
                <span style="flex: 1;">{{ sendError() }}</span>
                <button type="button" class="btn-text danger" (click)="sendDailyUpdate()">Retry</button>
              </div>
            }
          }
        </div>
      </div>

      <!-- Recipient Management Modal Overlay -->
      @if (showRecipientsModal()) {
        <div class="modal-overlay" (click)="closeRecipientsModal()">
          <div class="modal-container" (click)="$event.stopPropagation()">
            <div class="modal-header">
              <div style="display: flex; align-items: center; gap: 0.5rem;">
                <span class="material-symbols-outlined" style="color: var(--primary-color);">alternate_email</span>
                <h3>Manage Extra Recipients</h3>
              </div>
              <button type="button" class="btn-close" (click)="closeRecipientsModal()">×</button>
            </div>
            <div class="modal-body">
              <p class="modal-desc">Add external email addresses to receive the automated daily sprint summary report.</p>

              <div class="recipient-tags">
                @for (email of extraRecipientsList(); track email; let idx = $index) {
                  <span class="tag">
                    <span class="tag-text">{{ email }}</span>
                    <button type="button" class="btn-remove-tag" (click)="removeRecipientTag(idx)">×</button>
                  </span>
                }
                @if (extraRecipientsList().length === 0) {
                  <p class="no-recipients">No additional recipients configured. Broadcasts will only go to active project members.</p>
                }
              </div>

              <form [formGroup]="recipientForm" (ngSubmit)="addRecipientTag()" class="tag-input-row">
                <input
                  type="email"
                  placeholder="Enter email address (e.g. client@zensar.com)..."
                  formControlName="email"
                />
                <button type="submit" [disabled]="recipientForm.invalid" class="btn btn-primary">
                  <span class="material-symbols-outlined" style="font-size: 1.1rem;">add</span> Add
                </button>
              </form>

              <div class="modal-footer">
                <button type="button" class="btn btn-secondary" (click)="closeRecipientsModal()">Cancel</button>
                <button type="button" class="btn btn-primary" (click)="saveRecipients()">Save Recipients</button>
              </div>
            </div>
          </div>
        </div>
      }
    </div>
  `,
  styles: `
    .page-container {
      padding: 2rem;
      max-width: 800px;
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

    .daily-update-panel {
      background: var(--bg-panel);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      padding: 2rem;
      box-shadow: var(--shadow-md);
    }

    .card-header-area {
      display: flex;
      align-items: center;
      gap: 1rem;
      margin-bottom: 2rem;
      border-bottom: 1px solid var(--border-color);
      padding-bottom: 1.25rem;
    }

    .broadcast-icon {
      font-size: 2.25rem;
      color: var(--primary-color);
      background: var(--primary-glow);
      padding: 0.5rem;
      border-radius: var(--radius-md);
    }

    .header-text-block h3 {
      margin: 0;
      font-size: 1.2rem;
      font-weight: 700;
      color: var(--text-primary);
    }

    .desc-text {
      margin: 0.15rem 0 0;
      font-size: 0.85rem;
      color: var(--text-secondary);
    }

    .action-row {
      display: flex;
      flex-wrap: wrap;
      gap: 1rem;
      margin-bottom: 1.5rem;
    }

    .btn-glow {
      box-shadow: 0 0 12px var(--primary-glow);
    }

    .spinner-container {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      color: var(--text-secondary);
      font-size: 0.95rem;
      padding: 1.5rem 0;
      justify-content: center;
    }

    .loading-spinner {
      animation: spin 1s linear infinite;
      color: var(--primary-color);
      font-size: 1.5rem;
    }

    @keyframes spin {
      from { transform: rotate(0deg); }
      to { transform: rotate(360deg); }
    }

    .status-banner {
      display: flex;
      align-items: center;
      gap: 0.65rem;
      padding: 0.85rem 1.15rem;
      border-radius: var(--radius-md);
      font-size: 0.875rem;
      margin-bottom: 1.25rem;
      line-height: 1.4;
    }

    .status-banner.success {
      background: rgba(16, 185, 129, 0.1);
      color: #10b981;
      border: 1px solid rgba(16, 185, 129, 0.2);
    }

    .status-banner.info {
      background: rgba(59, 130, 246, 0.1);
      color: #3b82f6;
      border: 1px solid rgba(59, 130, 246, 0.2);
    }

    .status-banner.error {
      background: rgba(239, 68, 68, 0.1);
      color: var(--danger-color);
      border: 1px solid rgba(239, 68, 68, 0.2);
    }

    .btn-text.danger {
      background: transparent;
      border: none;
      color: var(--danger-color);
      font-weight: 600;
      cursor: pointer;
      padding: 0.2rem 0.5rem;
      border-radius: var(--radius-sm);
    }

    .btn-text.danger:hover {
      background: rgba(239, 68, 68, 0.08);
      text-decoration: underline;
    }

    .last-sent {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.85rem;
      color: #10b981;
      margin-top: 0.75rem;
      padding: 0 0.25rem;
    }

    .last-sent span.material-symbols-outlined {
      font-size: 1.1rem;
      font-weight: 700;
    }

    /* Modal Glassmorphic Styling */
    .modal-overlay {
      position: fixed;
      top: 0;
      left: 0;
      width: 100vw;
      height: 100vh;
      background: rgba(11, 17, 32, 0.6);
      backdrop-filter: blur(8px);
      -webkit-backdrop-filter: blur(8px);
      z-index: 1000;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .modal-container {
      background: var(--bg-panel);
      border-radius: var(--radius-lg);
      border: 1px solid var(--border-color);
      width: 90%;
      max-width: 500px;
      box-shadow: var(--shadow-lg);
      overflow: hidden;
      animation: modalFadeIn 0.25s cubic-bezier(0.34, 1.56, 0.64, 1);
    }

    @keyframes modalFadeIn {
      from { transform: scale(0.95); opacity: 0; }
      to { transform: scale(1); opacity: 1; }
    }

    .modal-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 1.25rem 1.5rem;
      border-bottom: 1px solid var(--border-color);
    }

    .modal-header h3 {
      margin: 0;
      font-size: 1.1rem;
      font-weight: 700;
      color: var(--text-primary);
    }

    .btn-close {
      background: none;
      border: none;
      font-size: 1.5rem;
      color: var(--text-muted);
      cursor: pointer;
      line-height: 1;
      padding: 0.2rem;
      border-radius: var(--radius-sm);
    }

    .btn-close:hover {
      color: var(--text-primary);
      background: var(--bg-hover);
    }

    .modal-body {
      padding: 1.5rem;
    }

    .modal-desc {
      margin: 0 0 1.25rem;
      font-size: 0.875rem;
      color: var(--text-secondary);
      line-height: 1.5;
    }

    .recipient-tags {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
      padding: 1rem;
      border: 1.5px solid var(--border-color);
      border-radius: var(--radius-md);
      min-height: 100px;
      max-height: 180px;
      overflow-y: auto;
      background: var(--bg-hover);
      margin-bottom: 1.25rem;
    }

    .recipient-tags .tag {
      display: inline-flex;
      align-items: center;
      gap: 0.45rem;
      background: var(--primary-glow);
      color: var(--primary-color);
      border: 1px solid var(--border-color);
      border-radius: 9999px;
      padding: 0.3rem 0.75rem;
      font-size: 0.8rem;
      font-weight: 600;
      max-width: 100%;
    }

    .tag-text {
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .btn-remove-tag {
      background: none;
      border: none;
      color: var(--primary-color);
      cursor: pointer;
      font-size: 1.1rem;
      padding: 0;
      line-height: 1;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 700;
    }

    .btn-remove-tag:hover {
      opacity: 0.7;
    }

    .no-recipients {
      margin: auto;
      color: var(--text-muted);
      font-size: 0.85rem;
      text-align: center;
      line-height: 1.4;
    }

    .tag-input-row {
      display: flex;
      gap: 0.5rem;
      margin-bottom: 1.5rem;
    }

    .tag-input-row input {
      flex: 1;
    }

    .modal-footer {
      display: flex;
      justify-content: flex-end;
      gap: 0.75rem;
      border-top: 1px solid var(--border-color);
      padding-top: 1.25rem;
    }
  `
})
export class DailyUpdatesComponent implements OnInit {
  private readonly taskService = inject(TaskService);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly backButtonService = inject(BackButtonService);

  readonly updateState = signal<any | null>(null);
  readonly sendingEmail = signal(false);
  readonly sendError = signal<string | null>(null);
  readonly showRecipientsModal = signal(false);
  readonly extraRecipientsList = signal<string[]>([]);
  readonly error = signal<string | null>(null);

  readonly recipientForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
  });

  private recipientsModalCleanup: (() => void) | null = null;
  private readonly recipientsModalEffect = effect(() => {
    const isOpen = this.showRecipientsModal();
    if (isOpen) {
      this.recipientsModalCleanup = this.backButtonService.registerHandler(
        'DailyUpdatesRecipientsModal',
        15,
        () => {
          this.closeRecipientsModal();
          return true; // Consumed
        }
      );
    } else {
      if (this.recipientsModalCleanup) {
        this.recipientsModalCleanup();
        this.recipientsModalCleanup = null;
      }
    }
  });

  private projectId(): string | null {
    return this.route.snapshot.paramMap.get('projectId') || 
           this.route.parent?.snapshot.paramMap.get('projectId') || 
           null;
  }

  ngOnInit(): void {
    this.loadDailyUpdateState();
  }

  loadDailyUpdateState(): void {
    const projectId = this.projectId();
    if (!projectId) return;

    this.taskService.getDailyUpdateState(projectId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.updateState.set(res);
          this.extraRecipientsList.set(res.extraRecipients || []);
        },
        error: (err) => {
          console.warn("Could not fetch daily update state", err);
        }
      });
  }

  sendDailyUpdate(): void {
    const projectId = this.projectId();
    if (!projectId) return;

    this.sendingEmail.set(true);
    this.sendError.set(null);

    this.taskService.sendDailyUpdate(projectId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.sendingEmail.set(false);
          this.loadDailyUpdateState();
        },
        error: (err) => {
          this.sendingEmail.set(false);
          this.sendError.set(getApiErrorMessage(err));
        }
      });
  }

  openRecipientsModal(): void {
    this.showRecipientsModal.set(true);
    if (this.updateState()) {
      this.extraRecipientsList.set([...(this.updateState().extraRecipients || [])]);
    }
  }

  closeRecipientsModal(): void {
    this.showRecipientsModal.set(false);
    this.recipientForm.reset();
  }

  addRecipientTag(): void {
    if (this.recipientForm.invalid) return;
    const email = this.recipientForm.controls.email.value.trim().toLowerCase();
    if (email && !this.extraRecipientsList().includes(email)) {
      this.extraRecipientsList.update(list => [...list, email]);
    }
    this.recipientForm.reset();
  }

  removeRecipientTag(index: number): void {
    this.extraRecipientsList.update(list => list.filter((_, idx) => idx !== index));
  }

  saveRecipients(): void {
    const projectId = this.projectId();
    if (!projectId) return;

    this.taskService.saveDailyUpdateRecipients(projectId, this.extraRecipientsList())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.extraRecipientsList.set(res);
          if (this.updateState()) {
            this.updateState().extraRecipients = res;
          }
          this.closeRecipientsModal();
        },
        error: (err) => this.error.set(getApiErrorMessage(err))
      });
  }
}
