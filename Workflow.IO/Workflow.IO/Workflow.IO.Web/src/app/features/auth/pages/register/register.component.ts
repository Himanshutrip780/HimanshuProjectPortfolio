import { Component, inject, OnInit, OnDestroy, signal } from '@angular/core';
import {
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { AuthService } from '../../../../core/services/auth.service';
import { ApiErrorService } from '../../../../core/services/api-error.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styles: `
    .auth-page-container {
      min-height: 100vh;
      width: 100%;
      display: flex;
      position: relative;
      overflow-x: hidden;
      background-color: var(--bg-body, #f9fafb);
      transition: background-color var(--transition-normal);
    }

    .auth-page-container.dark-theme {
      --bg-body: #0b1120;
      --bg-panel: rgba(15, 23, 42, 0.7);
      --border-color: rgba(99, 102, 241, 0.15);
      --text-primary: #f8fafc;
      --text-secondary: #94a3b8;
      --bg-card: rgba(15, 23, 42, 0.85);
      --bg-input: #0f172a;
      --bg-hover: #1e293b;
    }

    /* Theme toggler floating button */
    .theme-toggle-btn {
      position: absolute;
      top: 1.5rem;
      right: 1.5rem;
      z-index: 100;
      width: 2.75rem;
      height: 2.75rem;
      border-radius: 50%;
      border: 1px solid var(--border-color, #e2e8f0);
      background-color: var(--bg-panel, #ffffff);
      color: var(--text-primary, #0f172a);
      display: flex;
      align-items: center;
      justify-content: center;
      cursor: pointer;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
      transition: all var(--transition-normal);
    }
    .theme-toggle-btn:hover {
      transform: scale(1.05) rotate(15deg);
      box-shadow: 0 6px 16px rgba(99, 102, 241, 0.15);
    }

    /* Split layout container */
    .split-layout {
      display: flex;
      width: 100%;
    }

    /* Left Branding Section styling */
    .branding-section {
      flex: 1.15;
      display: flex;
      flex-direction: column;
      justify-content: center;
      padding: 4rem 5rem;
      position: relative;
      background-color: #030712;
      overflow: hidden;
      color: #ffffff;
    }

    /* Glow mesh backgrounds and dynamic blobs */
    .mesh-background {
      position: absolute;
      inset: 0;
      z-index: 0;
      pointer-events: none;
      background: radial-gradient(circle at 0% 0%, rgba(99, 102, 241, 0.15) 0%, transparent 50%),
                  radial-gradient(circle at 100% 100%, rgba(139, 92, 246, 0.12) 0%, transparent 50%);
    }
    .blob {
      position: absolute;
      border-radius: 50%;
      filter: blur(100px);
      opacity: 0.25;
      mix-blend-mode: screen;
    }
    .blob-1 {
      width: 350px;
      height: 350px;
      background: #6366f1;
      top: -10%;
      left: 10%;
      animation: float-blob1 20s infinite alternate ease-in-out;
    }
    .blob-2 {
      width: 400px;
      height: 400px;
      background: #8b5cf6;
      bottom: -15%;
      right: 5%;
      animation: float-blob2 25s infinite alternate ease-in-out;
    }
    .blob-3 {
      width: 280px;
      height: 280px;
      background: #d946ef;
      top: 40%;
      left: 45%;
      animation: float-blob3 18s infinite alternate ease-in-out;
    }

    @keyframes float-blob1 {
      0% { transform: translate(0, 0) scale(1); }
      100% { transform: translate(50px, 30px) scale(1.1); }
    }
    @keyframes float-blob2 {
      0% { transform: translate(0, 0) scale(1); }
      100% { transform: translate(-40px, -50px) scale(0.9); }
    }
    @keyframes float-blob3 {
      0% { transform: translate(0, 0) scale(1); }
      100% { transform: translate(30px, -40px) scale(1.15); }
    }

    .branding-content {
      position: relative;
      z-index: 1;
      max-width: 620px;
      display: flex;
      flex-direction: column;
      gap: 2.25rem;
    }

    /* Logo area styling */
    .logo-area {
      display: flex;
      align-items: center;
      gap: 1rem;
    }
    .logo-container {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 3.5rem;
      height: 3.5rem;
      border-radius: 12px;
      background: rgba(255, 255, 255, 0.04);
      border: 1px solid rgba(255, 255, 255, 0.1);
      box-shadow: 0 8px 32px rgba(0, 0, 0, 0.24), inset 0 1px 1px rgba(255, 255, 255, 0.08);
      backdrop-filter: blur(10px);
      -webkit-backdrop-filter: blur(10px);
      transition: transform 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275), border-color 0.3s ease, box-shadow 0.3s ease;
    }
    .logo-container:hover {
      transform: scale(1.08) rotate(3deg);
      border-color: rgba(139, 92, 246, 0.5);
      box-shadow: 0 12px 40px rgba(139, 92, 246, 0.3), inset 0 1px 1px rgba(255, 255, 255, 0.15);
    }
    .logo-image {
      width: 2rem;
      height: 2rem;
      object-fit: contain;
      filter: drop-shadow(0 2px 8px rgba(0, 0, 0, 0.25));
    }
    .logo-text {
      font-size: 1.5rem;
      font-weight: 800;
      letter-spacing: -0.03em;
      background: linear-gradient(135deg, #ffffff 0%, #cbd5e1 100%);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      transition: color 0.3s ease;
    }

    /* Typography */
    .headline {
      font-size: 2.75rem;
      font-weight: 800;
      line-height: 1.15;
      letter-spacing: -0.04em;
      background: linear-gradient(135deg, #ffffff 0%, #cbd5e1 100%);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
    }
    .tagline {
      font-size: 1.05rem;
      line-height: 1.6;
      color: #94a3b8;
    }

    /* Feature Grid cards */
    .features-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 1.5rem;
    }
    .feature-card {
      display: flex;
      gap: 1rem;
      align-items: flex-start;
    }
    .feature-icon {
      font-size: 1.25rem;
      padding: 0.4rem;
      border-radius: 8px;
      border: 1px solid rgba(255, 255, 255, 0.1);
      background-color: rgba(255, 255, 255, 0.03);
    }
    .feature-icon.purple-glow { color: #a78bfa; border-color: rgba(167, 139, 250, 0.25); background: rgba(167, 139, 250, 0.05); }
    .feature-icon.green-glow { color: #34d399; border-color: rgba(52, 211, 153, 0.25); background: rgba(52, 211, 153, 0.05); }
    .feature-icon.amber-glow { color: #fbbf24; border-color: rgba(251, 191, 36, 0.25); background: rgba(251, 191, 36, 0.05); }
    .feature-icon.blue-glow { color: #60a5fa; border-color: rgba(96, 165, 250, 0.25); background: rgba(96, 165, 250, 0.05); }

    .feature-text h3 {
      font-size: 0.95rem;
      font-weight: 600;
      color: #f8fafc;
      margin-bottom: 0.2rem;
    }
    .feature-text p {
      font-size: 0.825rem;
      color: #64748b;
      line-height: 1.4;
    }

    /* Mockup visual chart cards */
    .dashboard-preview-card {
      background: rgba(15, 23, 42, 0.4);
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 16px;
      padding: 1.25rem;
      box-shadow: 0 20px 40px -15px rgba(0, 0, 0, 0.5);
      backdrop-filter: blur(10px);
      display: flex;
      flex-direction: column;
      gap: 1rem;
      width: 100%;
    }
    .preview-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .preview-title {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.85rem;
      font-weight: 600;
      color: #cbd5e1;
    }
    .status-indicator {
      width: 8px;
      height: 8px;
      border-radius: 50%;
    }
    .status-indicator.online {
      background-color: #10b981;
      box-shadow: 0 0 8px #10b981;
    }
    .preview-badge {
      font-size: 0.75rem;
      font-weight: 500;
      padding: 0.15rem 0.5rem;
      border-radius: 999px;
      background: rgba(99, 102, 241, 0.15);
      color: #818cf8;
      border: 1px solid rgba(99, 102, 241, 0.25);
    }
    .preview-body {
      display: flex;
      align-items: flex-end;
      justify-content: space-between;
      gap: 2rem;
    }
    .preview-chart {
      display: flex;
      align-items: flex-end;
      gap: 0.5rem;
      height: 60px;
      flex: 1;
    }
    .chart-bar {
      width: 14px;
      border-radius: 3px 3px 0 0;
      background-color: rgba(99, 102, 241, 0.25);
    }
    .chart-bar.highlight {
      background: linear-gradient(180deg, #8b5cf6 0%, #6366f1 100%);
      box-shadow: 0 0 10px rgba(99, 102, 241, 0.5);
    }
    .preview-stats {
      display: flex;
      gap: 1.25rem;
    }
    .stat {
      display: flex;
      flex-direction: column;
    }
    .stat-label {
      font-size: 0.725rem;
      color: #64748b;
      text-transform: uppercase;
    }
    .stat-value {
      font-size: 1rem;
      font-weight: 700;
      color: #f8fafc;
    }

    /* Trusted By avatars */
    .trusted-by {
      display: flex;
      align-items: center;
      justify-content: space-between;
      border-top: 1px solid rgba(255, 255, 255, 0.08);
      padding-top: 1.5rem;
      margin-top: 1rem;
    }
    .trusted-by p {
      font-size: 0.8rem;
      color: #475569;
      font-weight: 500;
    }
    .avatar-group {
      display: flex;
      align-items: center;
    }
    .avatar-group img {
      width: 28px;
      height: 28px;
      border-radius: 50%;
      border: 2px solid #030712;
      margin-left: -8px;
    }
    .avatar-group img:first-child {
      margin-left: 0;
    }
    .avatar-more {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 28px;
      height: 28px;
      border-radius: 50%;
      background: #1f2937;
      color: #9ca3af;
      font-size: 0.7rem;
      font-weight: 600;
      border: 2px solid #030712;
      margin-left: -8px;
    }

    /* Right section auth form card styling */
    .form-section {
      flex: 0.85;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 3rem 4rem;
      position: relative;
      z-index: 10;
    }

    .form-wrapper {
      width: 100%;
      max-width: 440px;
    }

    .auth-card {
      background: var(--bg-panel, #ffffff);
      border: 1px solid var(--border-color, #e2e8f0);
      border-radius: 24px;
      padding: 3rem 2.5rem;
      box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.02),
                  0 8px 10px -6px rgba(0, 0, 0, 0.02);
      display: flex;
      flex-direction: column;
      gap: 1.75rem;
      transition: all var(--transition-normal);
      backdrop-filter: blur(16px);
      width: 100%;
    }

    .auth-card-header h2 {
      font-size: 1.75rem;
      font-weight: 800;
      letter-spacing: -0.03em;
      color: var(--text-primary, #0f172a);
    }
    .subtitle {
      font-size: 0.875rem;
      color: var(--text-secondary, #475569);
      margin-top: 0.25rem;
    }

    /* Forms */
    .auth-form {
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }
    .form-group {
      display: flex;
      flex-direction: column;
      gap: 0.4rem;
    }
    .form-group label {
      font-size: 0.825rem;
      font-weight: 600;
      color: var(--text-secondary);
    }

    .form-row-2 {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
    }

    @media (max-width: 400px) {
      .form-row-2 {
        grid-template-columns: 1fr;
      }
    }

    .input-with-icon {
      position: relative;
      display: flex;
      align-items: center;
    }
    .input-with-icon input {
      padding-left: 2.75rem;
      padding-right: 2.75rem;
      height: 2.75rem;
      background-color: var(--bg-input, #f8fafc);
      border: 1.5px solid var(--border-color, #cbd5e1);
      color: var(--text-primary);
      transition: all var(--transition-fast);
    }
    .input-icon {
      position: absolute;
      left: 0.85rem;
      color: var(--text-muted, #94a3b8);
      font-size: 1.25rem;
    }

    /* Password visibility eye toggle styling */
    .password-toggle-btn {
      position: absolute;
      right: 0.75rem;
      border: none;
      background: none;
      color: var(--text-muted, #94a3b8);
      cursor: pointer;
      padding: 0.25rem;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: color 0.15s ease;
    }
    .password-toggle-btn:hover {
      color: var(--text-primary);
    }

    /* Primary submit button */
    .submit-btn {
      height: 2.85rem;
      font-weight: 600;
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 0.5rem;
      background: var(--primary-gradient);
      box-shadow: 0 4px 14px rgba(99, 102, 241, 0.2);
    }
    .submit-btn:hover:not(:disabled) {
      box-shadow: 0 6px 20px rgba(99, 102, 241, 0.35);
    }

    /* Footer link */
    .auth-card-footer {
      display: flex;
      justify-content: center;
      gap: 0.35rem;
      font-size: 0.85rem;
      color: var(--text-secondary);
      border-top: 1px solid var(--border-color, #e2e8f0);
      padding-top: 1.25rem;
      margin-top: 0.5rem;
    }
    .auth-card-footer a {
      color: #6366f1;
      font-weight: 600;
      text-decoration: none;
      transition: color 0.15s ease;
    }
    .auth-card-footer a:hover {
      color: #4f46e5;
      text-decoration: underline;
    }

    /* Focus & Validation error indicators */
    input.invalid {
      border-color: #f43f5e !important;
      box-shadow: 0 0 0 3px rgba(244, 63, 94, 0.1) !important;
    }
    .field-error {
      color: #f43f5e;
      font-size: 0.725rem;
      margin-top: 0.15rem;
      font-weight: 500;
    }
    .alert-box {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.75rem 1rem;
      border-radius: 12px;
      font-size: 0.8rem;
      font-weight: 500;
      width: 100%;
    }
    .error-alert {
      background-color: rgba(244, 63, 94, 0.08);
      border: 1px solid rgba(244, 63, 94, 0.15);
      color: #f43f5e;
    }

    /* ========================================================
       DARK THEME overrides to prevent element disappearance
       ======================================================== */
    .auth-page-container.dark-theme .auth-card {
      background: rgba(11, 17, 32, 0.75);
      border-color: rgba(99, 102, 241, 0.18);
      box-shadow: 0 25px 60px -15px rgba(0, 0, 0, 0.7),
                  0 0 50px rgba(99, 102, 241, 0.08);
    }

    /* Footer visibility correction */
    .auth-page-container.dark-theme .auth-card-footer {
      color: #94a3b8;
      border-top-color: rgba(255, 255, 255, 0.1);
    }
    .auth-page-container.dark-theme .auth-card-footer a {
      color: #a78bfa;
    }
    .auth-page-container.dark-theme .auth-card-footer a:hover {
      color: #c084fc;
    }

    /* Form Inputs contrast in dark theme */
    .auth-page-container.dark-theme input {
      background-color: #0f172a;
      border-color: rgba(255, 255, 255, 0.12);
      color: #f8fafc;
    }
    .auth-page-container.dark-theme input:focus {
      border-color: #6366f1;
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.2);
    }
    .auth-page-container.dark-theme input::placeholder {
      color: #475569;
    }

    /* Breakpoints and responsive utilities */
    @media (max-width: 1024px) {
      .branding-section { padding: 3rem; }
      .form-section { padding: 3rem; }
      .headline { font-size: 2.25rem; }
    }
    @media (max-width: 768px) {
      .split-layout { flex-direction: column; }
      .branding-section { flex: none; padding: 3rem 2rem; }
      .form-section { flex: 1; padding: 3rem 2rem; }
      .features-grid { grid-template-columns: 1fr; gap: 1.25rem; }
      .dashboard-preview-card { display: none; }
    }
    
    .hover-scale {
      transition: transform var(--transition-fast) var(--motion-easing);
    }
    .hover-scale:hover {
      transform: scale(1.02);
    }
    
    .spinner-sm {
      width: 16px;
      height: 16px;
      border: 2px solid rgba(255, 255, 255, 0.3);
      border-top-color: white;
      border-radius: 50%;
      animation: spin 1s linear infinite;
      display: inline-block;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    /* Verification UI styling */
    .otp-inputs-wrapper {
      display: flex;
      justify-content: center;
      gap: 0.75rem;
      margin: 1.5rem 0;
    }
    .otp-digit-input {
      width: 3.25rem;
      height: 3.5rem;
      font-size: 1.75rem;
      font-weight: 700;
      text-align: center;
      border-radius: 12px;
      border: 1.5px solid var(--border-color, #cbd5e1);
      background-color: var(--bg-input, #f8fafc);
      color: var(--text-primary);
      transition: all var(--transition-fast);
    }
    .otp-digit-input:focus {
      border-color: #6366f1;
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.15);
      outline: none;
    }
    .timer-badge {
      display: inline-flex;
      align-items: center;
      gap: 0.35rem;
      font-size: 0.85rem;
      font-weight: 600;
      padding: 0.25rem 0.75rem;
      border-radius: 999px;
      background-color: rgba(99, 102, 241, 0.08);
      color: #6366f1;
      border: 1px solid rgba(99, 102, 241, 0.15);
      margin: 0.5rem 0;
    }
    .timer-badge.expired {
      background-color: rgba(244, 63, 94, 0.08);
      color: #f43f5e;
      border-color: rgba(244, 63, 94, 0.15);
    }
    .resend-btn {
      border: none;
      background: none;
      color: #6366f1;
      font-weight: 600;
      cursor: pointer;
      text-decoration: underline;
      transition: color 0.15s ease;
      padding: 0;
    }
    .resend-btn:hover:not(:disabled) {
      color: #4f46e5;
    }
    .resend-btn:disabled {
      color: var(--text-muted, #94a3b8);
      cursor: not-allowed;
      text-decoration: none;
    }
    .success-icon-wrapper {
      width: 4.5rem;
      height: 4.5rem;
      border-radius: 50%;
      background-color: rgba(16, 185, 129, 0.1);
      border: 1px solid rgba(16, 185, 129, 0.2);
      color: #10b981;
      display: flex;
      align-items: center;
      justify-content: center;
      margin: 1rem auto;
      font-size: 2.25rem;
      animation: pulse-success 2s infinite;
    }
    @keyframes pulse-success {
      0% { transform: scale(1); box-shadow: 0 0 0 0 rgba(16, 185, 129, 0.2); }
      70% { transform: scale(1.05); box-shadow: 0 0 0 10px rgba(16, 185, 129, 0); }
      100% { transform: scale(1); box-shadow: 0 0 0 0 rgba(16, 185, 129, 0); }
    }
  `,
})
export class RegisterComponent implements OnInit, OnDestroy {
  protected readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly apiErrors = inject(ApiErrorService);

  isDarkMode = signal(false);
  readonly showPassword = signal(false);
  
  // Registration steps
  activeStep = signal<'form' | 'otp' | 'success'>('form');

  // OTP Verification state
  otpCode = signal<string[]>(['', '', '', '', '', '']);
  otpTimer = signal<number>(600); // 10 minutes in seconds
  timerInterval: any = null;
  resendCount = signal<number>(0);
  verificationError = signal<string | null>(null);
  otpSuccessMessage = signal<string | null>(null);

  readonly form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    password: ['', [Validators.required, Validators.minLength(8)]]
  });

  ngOnInit(): void {
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme) {
      this.isDarkMode.set(savedTheme === 'dark');
      this.applyTheme(savedTheme);
    } else {
      const systemPrefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
      this.isDarkMode.set(systemPrefersDark);
      this.applyTheme(systemPrefersDark ? 'dark' : 'light');
    }
  }

  ngOnDestroy(): void {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }

  toggleTheme(): void {
    const nextTheme = this.isDarkMode() ? 'light' : 'dark';
    this.isDarkMode.set(nextTheme === 'dark');
    localStorage.setItem('theme', nextTheme);
    this.applyTheme(nextTheme);
  }

  private applyTheme(theme: string): void {
    const root = document.documentElement;
    if (theme === 'dark') {
      root.classList.add('dark-theme');
      root.setAttribute('data-theme', 'dark');
    } else {
      root.classList.remove('dark-theme');
      root.setAttribute('data-theme', 'light');
    }
  }

  togglePassword(): void {
    this.showPassword.update((v) => !v);
  }

  emailInvalid(): boolean {
    const control = this.form.controls.email;
    return control.invalid && (control.dirty || control.touched);
  }

  firstNameInvalid(): boolean {
    const control = this.form.controls.firstName;
    return control.invalid && (control.dirty || control.touched);
  }

  lastNameInvalid(): boolean {
    const control = this.form.controls.lastName;
    return control.invalid && (control.dirty || control.touched);
  }

  passwordInvalid(): boolean {
    const control = this.form.controls.password;
    return control.invalid && (control.dirty || control.touched);
  }

  startTimer(): void {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
    this.otpTimer.set(600);
    this.timerInterval = setInterval(() => {
      if (this.otpTimer() > 0) {
        this.otpTimer.update((t) => t - 1);
      } else {
        clearInterval(this.timerInterval);
      }
    }, 1000);
  }

  getFormattedTime(): string {
    const minutes = Math.floor(this.otpTimer() / 60);
    const seconds = this.otpTimer() % 60;
    return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  }

  onOtpInput(event: any, index: number): void {
    const val = event.target.value;
    if (!val) return;
    
    // Set character
    const codeArr = [...this.otpCode()];
    codeArr[index] = val.substring(val.length - 1);
    this.otpCode.set(codeArr);

    // Auto-focus next input
    if (index < 5) {
      const nextInput = document.getElementById(`otp-input-${index + 1}`) as HTMLInputElement;
      if (nextInput) {
        nextInput.focus();
      }
    }
  }

  onOtpKeyDown(event: KeyboardEvent, index: number): void {
    if (event.key === 'Backspace') {
      const codeArr = [...this.otpCode()];
      if (!codeArr[index] && index > 0) {
        // focus previous
        const prevInput = document.getElementById(`otp-input-${index - 1}`) as HTMLInputElement;
        if (prevInput) {
          prevInput.focus();
          codeArr[index - 1] = '';
          this.otpCode.set(codeArr);
        }
      } else {
        codeArr[index] = '';
        this.otpCode.set(codeArr);
      }
    }
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = this.form.getRawValue();
    this.verificationError.set(null);

    this.auth.sendOtp(payload).subscribe({
      next: () => {
        this.activeStep.set('otp');
        this.startTimer();
      },
      error: (err) => {
        this.apiErrors.capture(err, 'Verification code request failed');
      }
    });
  }

  onVerifyOtp(): void {
    const code = this.otpCode().join('');
    if (code.length < 6) {
      this.verificationError.set('Please enter the full 6-digit verification code.');
      return;
    }

    const email = this.form.controls.email.value;
    this.verificationError.set(null);

    this.auth.verifyOtp(email, code).subscribe({
      next: () => {
        this.activeStep.set('success');
        if (this.timerInterval) {
          clearInterval(this.timerInterval);
        }
        // Auto login after 2 seconds
        setTimeout(() => {
          this.auth.login({
            email: this.form.controls.email.value,
            password: this.form.controls.password.value,
          }).subscribe({
            next: () => void this.router.navigate(['/projects']),
          });
        }, 2000);
      },
      error: (err) => {
        this.verificationError.set(err.error?.message || 'Verification failed. Please check your code.');
      }
    });
  }

  onResendOtp(): void {
    if (this.resendCount() >= 3) {
      this.verificationError.set('Maximum resend attempts reached (3/3). Please request again later.');
      return;
    }

    const email = this.form.controls.email.value;
    this.verificationError.set(null);

    this.auth.resendOtp(email).subscribe({
      next: () => {
        this.resendCount.update((c) => c + 1);
        this.startTimer();
        this.otpCode.set(['', '', '', '', '', '']);
        
        // focus first input
        setTimeout(() => {
          const firstInput = document.getElementById('otp-input-0') as HTMLInputElement;
          if (firstInput) firstInput.focus();
        }, 100);

        this.otpSuccessMessage.set('A new verification code has been sent to your email.');
        setTimeout(() => this.otpSuccessMessage.set(null), 5000);
      },
      error: (err) => {
        this.verificationError.set(err.error?.message || 'Failed to resend verification code.');
      }
    });
  }

  backToForm(): void {
    this.activeStep.set('form');
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }
}

