import { Component, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule
  ],
  templateUrl: './auth.component.html',
  styleUrls: ['./auth.component.scss']
})
export class AuthComponent implements OnInit, OnDestroy {
  public activeView = signal<'login' | 'register' | 'forgot' | 'register-otp' | 'register-success'>('login');
  public error = signal<string | null>(null);
  public success = signal<string | null>(null);
  public loading = signal<boolean>(false);
  public passwordVisible = signal<boolean>(false);
  public rememberMe = signal<boolean>(false);

  public ssoEnabled = signal<boolean>(false);
  public ssoRedirectUrl = signal<string | null>(null);

  public loginForm: FormGroup;
  public registerForm: FormGroup;
  public forgotForm: FormGroup;

  public companiesList = signal<any[]>([]);
  public showManualCompanyInput = signal<boolean>(true); // Default to true until a match or selection is made

  // OTP Verification state
  public otpCode = signal<string[]>(['', '', '', '', '', '']);
  public otpTimer = signal<number>(600); // 10 minutes in seconds
  public timerInterval: any = null;
  public resendCount = signal<number>(0);
  public otpSuccessMessage = signal<string | null>(null);

  public roles = [
    { value: 'Recruiter', label: 'Recruiter' },
    { value: 'HiringManager', label: 'Hiring Manager' },
    { value: 'Interviewer', label: 'Interviewer' }
  ];

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });

    this.loginForm.get('email')?.valueChanges.subscribe(email => {
      if (!email || this.loginForm.get('email')?.invalid) {
        this.ssoEnabled.set(false);
        this.ssoRedirectUrl.set(null);
        return;
      }

      this.authService.resolveSso(email).subscribe({
        next: (res) => {
          if (res && res.isSuccess && res.data && res.data.ssoEnabled) {
            this.ssoEnabled.set(true);
            this.ssoRedirectUrl.set(res.data.redirectUrl);
          } else {
            this.ssoEnabled.set(false);
            this.ssoRedirectUrl.set(null);
          }
        },
        error: () => {
          this.ssoEnabled.set(false);
          this.ssoRedirectUrl.set(null);
        }
      });
    });

    this.registerForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      companyId: [null],
      companyName: ['', [Validators.required]],
      role: ['Recruiter', [Validators.required]]
    });

    this.forgotForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });

    this.loadCompanies();
    this.setupEmailChangeListener();
  }

  private loadCompanies() {
    this.authService.getCompanies().subscribe({
      next: (res) => {
        if (res && res.isSuccess && res.data) {
          this.companiesList.set(res.data);
        }
      },
      error: () => {}
    });
  }

  private setupEmailChangeListener() {
    this.registerForm.get('email')?.valueChanges.subscribe(email => {
      if (!email) return;
      const atIndex = email.indexOf('@');
      if (atIndex !== -1 && atIndex < email.length - 1) {
        const domain = email.substring(atIndex + 1).toLowerCase().trim();
        const genericDomains = ['gmail.com', 'yahoo.com', 'outlook.com', 'hotmail.com', 'aol.com', 'icloud.com', 'mail.com'];
        if (!genericDomains.includes(domain)) {
          const matched = this.companiesList().find(c => c.domain.toLowerCase() === domain);
          if (matched) {
            this.registerForm.patchValue({
              companyId: matched.id,
              companyName: matched.name
            }, { emitEvent: false });
            this.showManualCompanyInput.set(false);
          }
        }
      }
    });
  }

  public onCompanySelectChange(event: any) {
    const value = event.target.value;
    if (value === 'new') {
      this.showManualCompanyInput.set(true);
      this.registerForm.patchValue({
        companyId: null,
        companyName: ''
      });
    } else {
      const selectedCompany = this.companiesList().find(c => c.id === value);
      this.showManualCompanyInput.set(false);
      this.registerForm.patchValue({
        companyId: value,
        companyName: selectedCompany ? selectedCompany.name : ''
      });
    }
  }

  public setView(view: 'login' | 'register' | 'forgot' | 'register-otp' | 'register-success') {
    this.activeView.set(view);
    this.error.set(null);
    this.success.set(null);
    if (view !== 'register-otp' && this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }

  public onLogin() {
    if (this.loginForm.invalid) return;
    this.loading.set(true);
    this.error.set(null);

    this.authService.login(this.loginForm.value).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.isSuccess) {
          if (res.data?.role === 'Candidate') {
            this.router.navigate(['/portal']);
          } else {
            this.router.navigate(['/dashboard']);
          }
        } else {
          this.error.set(res.message || 'Login failed');
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message || 'Invalid email or password');
      }
    });
  }

  public startTimer(): void {
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

  public getFormattedTime(): string {
    const minutes = Math.floor(this.otpTimer() / 60);
    const seconds = this.otpTimer() % 60;
    return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  }

  public onOtpInput(event: any, index: number): void {
    const val = event.target.value;
    if (!val) return;
    
    const codeArr = [...this.otpCode()];
    codeArr[index] = val.substring(val.length - 1);
    this.otpCode.set(codeArr);

    if (index < 5) {
      const nextInput = document.getElementById(`hn-otp-input-${index + 1}`) as HTMLInputElement;
      if (nextInput) {
        nextInput.focus();
      }
    }
  }

  public onOtpKeyDown(event: KeyboardEvent, index: number): void {
    if (event.key === 'Backspace') {
      const codeArr = [...this.otpCode()];
      if (!codeArr[index] && index > 0) {
        const prevInput = document.getElementById(`hn-otp-input-${index - 1}`) as HTMLInputElement;
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

  public onRegister() {
    if (this.registerForm.invalid) return;
    this.loading.set(true);
    this.error.set(null);

    this.authService.sendOtp(this.registerForm.value).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.isSuccess) {
          this.setView('register-otp');
          this.startTimer();
        } else {
          this.error.set(res.message || 'Registration request failed.');
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message || 'Registration failed. Check your input.');
      }
    });
  }

  public onVerifyOtp() {
    const code = this.otpCode().join('');
    if (code.length < 6) {
      this.error.set('Please enter the full 6-digit verification code.');
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    const email = this.registerForm.get('email')?.value;

    this.authService.verifyOtp(email, code).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.isSuccess) {
          this.setView('register-success');
          if (this.timerInterval) {
            clearInterval(this.timerInterval);
          }
          setTimeout(() => {
            if (res.data?.role === 'Candidate') {
              this.router.navigate(['/portal']);
            } else {
              this.router.navigate(['/dashboard']);
            }
          }, 3000);
        } else {
          this.error.set(res.message || 'Verification failed.');
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message || 'Verification failed. Please check your code.');
      }
    });
  }

  public onResendOtp() {
    if (this.resendCount() >= 3) {
      this.error.set('Maximum resend attempts reached (3/3). Please request again later.');
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    const email = this.registerForm.get('email')?.value;

    this.authService.resendOtp(email).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.isSuccess) {
          this.resendCount.update(c => c + 1);
          this.startTimer();
          this.otpCode.set(['', '', '', '', '', '']);
          
          setTimeout(() => {
            const firstInput = document.getElementById('hn-otp-input-0') as HTMLInputElement;
            if (firstInput) firstInput.focus();
          }, 100);

          this.otpSuccessMessage.set('A new verification code has been sent to your email.');
          setTimeout(() => this.otpSuccessMessage.set(null), 5000);
        } else {
          this.error.set(res.message || 'Failed to resend code.');
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message || 'Failed to resend verification code.');
      }
    });
  }

  public backToRegister() {
    this.setView('register');
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }

  public ngOnDestroy() {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }

  public onForgot() {
    if (this.forgotForm.invalid) return;
    this.loading.set(true);
    this.error.set(null);
    this.success.set(null);

    this.authService.forgotPassword(this.forgotForm.value.email).subscribe({
      next: (res) => {
        this.loading.set(false);
        this.success.set('Password reset instructions sent. Please check your mailbox.');
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message || 'Failed to process request.');
      }
    });
  }

  public ngOnInit() {
    this.route.queryParams.subscribe(params => {
      const token = params['token'];
      const refreshToken = params['refreshToken'];
      const errorMsg = params['error'];
      const verified = params['verified'];

      if (token && refreshToken) {
        this.authService.setSessionFromCallback(token, refreshToken);
        const user = this.authService.currentUser();
        if (user && user.role === 'Candidate') {
          this.router.navigate(['/portal']);
        } else {
          this.router.navigate(['/dashboard']);
        }
      } else if (errorMsg) {
        this.error.set(errorMsg);
      } else if (verified) {
        this.success.set('Email domain verified successfully! You can now log in.');
      }
    });
  }

  public onSsoSignIn() {
    const url = this.ssoRedirectUrl();
    if (url) {
      window.location.href = url;
    }
  }
}
