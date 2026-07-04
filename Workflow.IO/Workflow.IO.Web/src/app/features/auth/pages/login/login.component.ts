import { Component, inject, signal, OnInit, effect } from '@angular/core';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import {
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';

import { AuthService } from '../../../../core/services/auth.service';
import { BackButtonService } from '../../../../core/services/back-button.service';

interface OauthAccount {
  name: string;
  email: string;
  avatar: string;
  role: string;
}

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styles: `
    :host {
      --primary-color: #6366f1;
      --primary-hover: #4f46e5;
      --bg-dark: #030712;
      --card-bg: #090d16;
      --card-border: rgba(99, 102, 241, 0.15);
      --input-bg: #0d121f;
      --input-border: rgba(255, 255, 255, 0.08);
      --text-muted: #64748b;
      --text-secondary: #94a3b8;
      --text-primary: #f8fafc;
    }

    .auth-page-container {
      min-height: 100vh;
      width: 100%;
      display: flex;
      flex-direction: column;
      position: relative;
      overflow: hidden;
      background-color: var(--bg-dark);
      font-family: 'Inter', system-ui, -apple-system, sans-serif;
    }

    .split-layout {
      display: flex;
      width: 100%;
      flex: 1;
      min-height: 0;
    }

    /* Left Branding Section */
    .branding-section {
      flex: 1.1;
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      padding: 5rem 6rem;
      position: relative;
      background-color: #030712;
      overflow: hidden;
    }

    .mesh-background {
      position: absolute;
      inset: 0;
      z-index: 0;
      pointer-events: none;
      background: radial-gradient(circle at 10% 10%, rgba(99, 102, 241, 0.15) 0%, transparent 60%),
                  radial-gradient(circle at 90% 90%, rgba(139, 92, 246, 0.1) 0%, transparent 60%);
    }

    .blob {
      position: absolute;
      border-radius: 50%;
      filter: blur(120px);
      opacity: 0.18;
      mix-blend-mode: screen;
    }
    .blob-1 {
      width: 400px;
      height: 400px;
      background: #4f46e5;
      top: -10%;
      left: -5%;
      animation: float-blob1 25s infinite alternate ease-in-out;
    }
    .blob-2 {
      width: 500px;
      height: 500px;
      background: #7c3aed;
      bottom: -10%;
      right: -10%;
      animation: float-blob2 30s infinite alternate ease-in-out;
    }
    .blob-3 {
      width: 300px;
      height: 300px;
      background: #c084fc;
      top: 35%;
      left: 35%;
      animation: float-blob3 22s infinite alternate ease-in-out;
    }

    @keyframes float-blob1 {
      0% { transform: translate(0, 0) scale(1); }
      100% { transform: translate(60px, 40px) scale(1.1); }
    }
    @keyframes float-blob2 {
      0% { transform: translate(0, 0) scale(1); }
      100% { transform: translate(-50px, -60px) scale(0.95); }
    }
    @keyframes float-blob3 {
      0% { transform: translate(0, 0) scale(1); }
      100% { transform: translate(40px, -30px) scale(1.1); }
    }

    .dot-grid {
      position: absolute;
      width: 280px;
      height: 180px;
      background-image: radial-gradient(rgba(255, 255, 255, 0.08) 1.5px, transparent 1.5px);
      background-size: 16px 16px;
      bottom: 8%;
      right: 5%;
      opacity: 0.65;
      pointer-events: none;
      z-index: 0;
    }

    .branding-content {
      position: relative;
      z-index: 1;
      width: 100%;
      max-width: 640px;
      display: flex;
      flex-direction: column;
      gap: 2.5rem;
      height: 100%;
      justify-content: center;
    }

    .logo-area {
      display: flex;
      align-items: center;
      gap: 1.25rem;
    }
    .logo-container {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 2.75rem;
      height: 2.75rem;
      background: transparent;
      border: none;
      box-shadow: none;
      backdrop-filter: none;
    }
    .logo-image {
      width: 100%;
      height: 100%;
      object-fit: contain;
    }
    .logo-text-group {
      display: flex;
      flex-direction: column;
    }
    .logo-text {
      font-size: 1.5rem;
      font-weight: 700;
      letter-spacing: -0.02em;
      color: #ffffff;
    }
    .logo-text .text-accent {
      color: #60a5fa;
    }
    .logo-subtitle {
      font-size: 0.8rem;
      color: var(--text-secondary);
      font-weight: 500;
      margin-top: 0.1rem;
    }

    .headline {
      font-size: 2.85rem;
      font-weight: 800;
      line-height: 1.2;
      letter-spacing: -0.03em;
      color: #ffffff;
    }
    .gradient-text {
      background: linear-gradient(135deg, #a78bfa 0%, #60a5fa 100%);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
    }

    .tagline {
      font-size: 1.05rem;
      line-height: 1.6;
      color: var(--text-secondary);
      margin-top: -0.5rem;
    }

    /* Features Grid */
    .features-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 2rem 2.5rem;
    }
    .feature-card {
      display: flex;
      gap: 1rem;
      align-items: flex-start;
    }
    .feature-icon-wrapper {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 2.5rem;
      height: 2.5rem;
      border-radius: 10px;
      border: 1px solid rgba(255, 255, 255, 0.08);
      background-color: rgba(255, 255, 255, 0.02);
      flex-shrink: 0;
    }
    .feature-icon {
      font-size: 1.25rem;
    }

    /* Colors and shadows matching the feature cards */
    .purple-box { color: #a78bfa; border-color: rgba(167, 139, 250, 0.2); background: rgba(167, 139, 250, 0.05); }
    .green-box { color: #34d399; border-color: rgba(52, 211, 153, 0.2); background: rgba(52, 211, 153, 0.05); }
    .amber-box { color: #fbbf24; border-color: rgba(251, 191, 36, 0.2); background: rgba(251, 191, 36, 0.05); }
    .blue-box { color: #60a5fa; border-color: rgba(96, 165, 250, 0.2); background: rgba(96, 165, 250, 0.05); }

    .feature-text h3 {
      font-size: 0.95rem;
      font-weight: 600;
      color: #ffffff;
      margin: 0 0 0.25rem 0;
    }
    .feature-text p {
      font-size: 0.825rem;
      color: var(--text-muted);
      line-height: 1.4;
      margin: 0;
    }

    /* Dashboard Mockup Tilt effect */
    .dashboard-preview-container {
      margin-top: 2.5rem;
      perspective: 1200px;
      width: 100%;
      max-width: 480px;
      align-self: flex-start;
    }
    .dashboard-preview-img {
      width: 100%;
      border-radius: 12px;
      border: 1px solid rgba(255, 255, 255, 0.1);
      box-shadow: 0 30px 60px rgba(0, 0, 0, 0.6), 
                  0 0 40px rgba(99, 102, 241, 0.15);
      transform: rotateX(15deg) rotateY(-15deg) rotateZ(5deg);
      transition: transform 0.6s cubic-bezier(0.16, 1, 0.3, 1);
    }
    .dashboard-preview-container:hover .dashboard-preview-img {
      transform: rotateX(10deg) rotateY(-10deg) rotateZ(3deg) scale(1.02);
      box-shadow: 0 40px 80px rgba(0, 0, 0, 0.8), 
                  0 0 60px rgba(99, 102, 241, 0.25);
    }

    /* Right Login Section */
    .form-section {
      flex: 0.9;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 4rem 5rem;
      position: relative;
    }

    .form-wrapper {
      width: 100%;
      max-width: 460px;
      display: flex;
      flex-direction: column;
    }

    .auth-card {
      background: var(--card-bg);
      border: 1px solid var(--card-border);
      border-radius: 20px;
      padding: 3.5rem 3rem;
      box-shadow: 0 20px 50px rgba(0, 0, 0, 0.4),
                  inset 0 1px 1px rgba(255, 255, 255, 0.05);
      display: flex;
      flex-direction: column;
      gap: 1.75rem;
    }

    .auth-card-header h2 {
      font-size: 1.75rem;
      font-weight: 700;
      color: #ffffff;
      margin: 0;
    }
    .auth-card-header .subtitle {
      font-size: 0.875rem;
      color: var(--text-secondary);
      margin-top: 0.5rem;
    }

    .auth-form {
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }
    .form-group label {
      font-size: 0.825rem;
      font-weight: 600;
      color: var(--text-secondary);
    }

    .password-label-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .forgot-link {
      font-size: 0.825rem;
      color: #6366f1;
      cursor: pointer;
      font-weight: 500;
      text-decoration: none;
    }
    .forgot-link:hover {
      color: #818cf8;
      text-decoration: underline;
    }

    .input-with-icon {
      position: relative;
      display: flex;
      align-items: center;
      width: 100%;
    }
    .input-with-icon input {
      width: 100%;
      padding: 0.75rem 1rem 0.75rem 2.75rem;
      height: 2.85rem;
      background-color: var(--input-bg);
      border: 1.5px solid var(--input-border);
      border-radius: 8px;
      color: #ffffff;
      font-size: 0.9rem;
      transition: all 0.2s ease;
      box-sizing: border-box;
    }
    #password {
      padding-right: 2.75rem;
    }
    .input-with-icon input:focus {
      outline: none;
      border-color: #6366f1;
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.2);
    }
    .input-with-icon input.invalid {
      border-color: #ef4444 !important;
      box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.15) !important;
    }
    .input-icon {
      position: absolute;
      left: 0.85rem;
      color: var(--text-muted);
      font-size: 1.25rem;
      pointer-events: none;
    }

    .password-toggle-btn {
      position: absolute;
      right: 0.85rem;
      border: none;
      background: none;
      color: var(--text-muted);
      cursor: pointer;
      padding: 0.25rem;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .password-toggle-btn:hover {
      color: #ffffff;
    }

    .field-error {
      color: #ef4444;
      font-size: 0.75rem;
      margin-top: 0.25rem;
      font-weight: 500;
    }

    .alert-box {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.75rem 1rem;
      border-radius: 8px;
      font-size: 0.8rem;
      font-weight: 500;
      width: 100%;
      box-sizing: border-box;
    }
    .error-alert {
      background-color: rgba(239, 68, 68, 0.08);
      border: 1px solid rgba(239, 68, 68, 0.15);
      color: #ef4444;
    }

    /* Custom Checkbox Design matching the blue check in the screenshot */
    .form-options {
      display: flex;
      align-items: center;
    }
    .checkbox-container {
      display: flex;
      align-items: center;
      position: relative;
      padding-left: 1.75rem;
      cursor: pointer;
      font-size: 0.875rem;
      font-weight: 500;
      color: var(--text-secondary);
      user-select: none;
    }
    .checkbox-container input {
      position: absolute;
      opacity: 0;
      cursor: pointer;
      height: 0;
      width: 0;
    }
    .checkmark {
      position: absolute;
      left: 0;
      height: 1.125rem;
      width: 1.125rem;
      background-color: var(--input-bg);
      border: 1.5px solid var(--input-border);
      border-radius: 4px;
      transition: all 0.15s ease;
    }
    .checkbox-container:hover input ~ .checkmark {
      border-color: #6366f1;
    }
    .checkbox-container input:checked ~ .checkmark {
      background-color: #6366f1;
      border-color: #6366f1;
    }
    .checkmark:after {
      content: "";
      position: absolute;
      display: none;
    }
    .checkbox-container input:checked ~ .checkmark:after {
      display: block;
    }
    .checkbox-container .checkmark:after {
      left: 4px;
      top: 1px;
      width: 3px;
      height: 6px;
      border: solid white;
      border-width: 0 2px 2px 0;
      transform: rotate(45deg);
    }

    /* Submit Button styling with glowing gradients and arrow */
    .submit-btn {
      height: 3rem;
      font-weight: 600;
      font-size: 0.95rem;
      border-radius: 8px;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 0.65rem;
      background: linear-gradient(135deg, #4f46e5 0%, #6366f1 100%);
      color: #ffffff;
      border: none;
      cursor: pointer;
      transition: all 0.2s ease;
      box-shadow: 0 4px 20px rgba(99, 102, 241, 0.5), 0 0 30px rgba(99, 102, 241, 0.2);
      width: 100%;
    }
    .submit-btn:hover:not(:disabled) {
      transform: translateY(-1px);
      box-shadow: 0 6px 24px rgba(99, 102, 241, 0.6), 0 0 40px rgba(99, 102, 241, 0.35);
    }
    .submit-btn:disabled {
      opacity: 0.7;
      cursor: not-allowed;
    }

    /* SSO continue separator */
    .separator {
      display: flex;
      align-items: center;
      text-align: center;
      font-size: 0.775rem;
      color: var(--text-muted);
      font-weight: 600;
      margin: 0.5rem 0;
    }
    .separator::before,
    .separator::after {
      content: '';
      flex: 1;
      border-bottom: 1px solid rgba(255, 255, 255, 0.08);
    }
    .separator:not(:empty)::before { margin-right: 1rem; }
    .separator:not(:empty)::after { margin-left: 1rem; }

    /* Social buttons */
    .social-buttons {
      display: flex;
      gap: 1rem;
    }
    .btn-social {
      flex: 1;
      height: 3rem;
      border-radius: 8px;
      border: 1.5px solid rgba(255, 255, 255, 0.08);
      background-color: rgba(255, 255, 255, 0.02);
      display: flex;
      align-items: center;
      justify-content: center;
      cursor: pointer;
      transition: all 0.2s ease;
    }
    .btn-social:hover {
      background-color: rgba(255, 255, 255, 0.05);
      border-color: rgba(99, 102, 241, 0.4);
      transform: translateY(-1px);
    }

    /* SSO account footer */
    .auth-card-footer {
      display: flex;
      justify-content: center;
      gap: 0.4rem;
      font-size: 0.875rem;
      color: var(--text-secondary);
    }
    .auth-card-footer a {
      color: #6366f1;
      font-weight: 600;
      text-decoration: none;
    }
    .auth-card-footer a:hover {
      color: #818cf8;
      text-decoration: underline;
    }

    /* Bottom Footer */
    .auth-footer {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1.5rem 6rem;
      border-top: 1px solid rgba(255, 255, 255, 0.05);
      background-color: #030712;
      position: relative;
      z-index: 10;
    }
    .footer-left {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }
    .footer-item {
      display: flex;
      align-items: center;
      gap: 0.4rem;
      font-size: 0.8rem;
      color: var(--text-secondary);
    }
    .footer-icon {
      font-size: 1.1rem;
      color: var(--text-muted);
    }
    .footer-separator {
      color: var(--text-muted);
    }
    .footer-right {
      font-size: 0.8rem;
      color: var(--text-muted);
    }

    /* Floating Theme Toggler styling */
    .theme-toggle-btn {
      position: absolute;
      top: 1.5rem;
      right: 1.5rem;
      z-index: 100;
      width: 2.5rem;
      height: 2.5rem;
      border-radius: 50%;
      border: 1px solid rgba(255, 255, 255, 0.08);
      background-color: rgba(255, 255, 255, 0.02);
      color: #ffffff;
      display: flex;
      align-items: center;
      justify-content: center;
      cursor: pointer;
      transition: all 0.2s ease;
    }
    .theme-toggle-btn:hover {
      transform: scale(1.05);
      border-color: rgba(99, 102, 241, 0.4);
    }

    /* OAuth modal styles */
    .oauth-modal-backdrop {
      position: fixed;
      inset: 0;
      background-color: rgba(0, 0, 0, 0.75);
      backdrop-filter: blur(8px);
      z-index: 10000;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 1.5rem;
    }
    .oauth-modal-container {
      width: 100%;
      max-width: 440px;
      background: #090d16;
      border: 1px solid var(--card-border);
      border-radius: 16px;
      box-shadow: 0 25px 50px rgba(0, 0, 0, 0.5);
      overflow: hidden;
    }
    .oauth-modal-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1.25rem 1.5rem;
      border-bottom: 1px solid rgba(255, 255, 255, 0.08);
    }
    .provider-brand {
      display: flex;
      align-items: center;
      gap: 0.65rem;
      font-weight: 700;
      color: #ffffff;
    }
    .btn-close-oauth {
      background: none;
      border: none;
      color: var(--text-muted);
      cursor: pointer;
      font-size: 1.25rem;
      display: flex;
      align-items: center;
    }
    .btn-close-oauth:hover {
      color: #ffffff;
    }
    .oauth-modal-body {
      padding: 1.5rem;
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }
    .oauth-intro {
      font-size: 0.875rem;
      color: var(--text-secondary);
      line-height: 1.5;
    }
    .oauth-accounts-list {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }
    .oauth-account-item {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 0.75rem 1rem;
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 8px;
      cursor: pointer;
      background-color: rgba(255, 255, 255, 0.01);
      transition: all 0.2s ease;
    }
    .oauth-account-item:hover {
      border-color: #6366f1;
      background-color: rgba(99, 102, 241, 0.05);
      transform: translateY(-1px);
    }
    .account-avatar img {
      width: 32px;
      height: 32px;
      border-radius: 50%;
    }
    .account-info {
      display: flex;
      flex-direction: column;
      flex: 1;
    }
    .account-name {
      font-size: 0.9rem;
      font-weight: 600;
      color: #ffffff;
    }
    .account-email {
      font-size: 0.75rem;
      color: var(--text-secondary);
    }
    .account-badge {
      font-size: 0.65rem;
      font-weight: 600;
      padding: 0.15rem 0.5rem;
      border-radius: 99px;
      background-color: rgba(99, 102, 241, 0.15);
      color: #818cf8;
      text-transform: uppercase;
    }
    .oauth-notice {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.75rem;
      color: var(--text-muted);
      border-top: 1px solid rgba(255, 255, 255, 0.08);
      padding-top: 1rem;
    }

    .oauth-loading-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 2rem 1rem;
      text-align: center;
      gap: 1rem;
    }
    .oauth-loading-state h3 {
      color: #ffffff;
      font-size: 1.1rem;
      margin: 0;
    }
    .oauth-loading-state p {
      color: var(--text-secondary);
      font-size: 0.85rem;
      margin: 0;
    }
    .spinner-lg {
      width: 40px;
      height: 40px;
      border: 3.5px solid rgba(99, 102, 241, 0.2);
      border-top-color: #6366f1;
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    /* Light Theme overrides for auth container */
    .auth-page-container:not(.dark-theme) {
      background-color: #f8fafc;
    }
    .auth-page-container:not(.dark-theme) .split-layout {
      background-color: #f8fafc;
    }
    .auth-page-container:not(.dark-theme) .auth-card {
      background-color: #ffffff;
      border-color: #e2e8f0;
      box-shadow: 0 20px 40px rgba(0, 0, 0, 0.05);
    }
    .auth-page-container:not(.dark-theme) .auth-card-header h2 {
      color: #0f172a;
    }
    .auth-page-container:not(.dark-theme) .subtitle {
      color: #475569;
    }
    .auth-page-container:not(.dark-theme) .form-group label {
      color: #475569;
    }
    .auth-page-container:not(.dark-theme) .input-with-icon input {
      background-color: #ffffff;
      border-color: #cbd5e1;
      color: #0f172a;
    }
    .auth-page-container:not(.dark-theme) .input-with-icon input:focus {
      border-color: #6366f1;
    }
    .auth-page-container:not(.dark-theme) .checkbox-container {
      color: #475569;
    }
    .auth-page-container:not(.dark-theme) .checkmark {
      background-color: #ffffff;
      border-color: #cbd5e1;
    }
    .auth-page-container:not(.dark-theme) .btn-social {
      background-color: #ffffff;
      border-color: #e2e8f0;
    }
    .auth-page-container:not(.dark-theme) .btn-social:hover {
      background-color: #f8fafc;
    }
    .auth-page-container:not(.dark-theme) .auth-card-footer {
      color: #475569;
    }
    .auth-page-container:not(.dark-theme) .auth-footer {
      background-color: #ffffff;
      border-top-color: #e2e8f0;
    }
    .auth-page-container:not(.dark-theme) .footer-item {
      color: #475569;
    }

    @media (max-width: 1024px) {
      .branding-section { padding: 3rem 4rem; }
      .form-section { padding: 3rem; }
      .headline { font-size: 2.25rem; }
    }
    @media (max-width: 768px) {
      .split-layout { flex-direction: column; }
      .branding-section { flex: none; padding: 4rem 2rem; }
      .form-section { flex: 1; padding: 3rem 2rem; }
      .features-grid { grid-template-columns: 1fr; gap: 1.5rem; }
      .dashboard-preview-container { display: none; }
      .auth-footer { flex-direction: column; gap: 1rem; padding: 2rem; text-align: center; }
      .footer-left { flex-direction: column; gap: 0.5rem; }
      .footer-separator { display: none; }
    }
  `,
})
export class LoginComponent implements OnInit {
  protected readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly backButtonService = inject(BackButtonService);

  // Mapped credentials for OAuth token requests
  readonly demoEmail = 'himanshutrip780@gmail.com';
  readonly demoPassword = 'Password123';

  // Signals for local UI state
  readonly isDarkMode = signal(false);
  readonly showPassword = signal(false);
  
  // Dynamic Social Sign-In simulator properties
  readonly activeSocialProvider = signal<string | null>(null);
  readonly socialAuthenticating = signal(false);
  
  readonly oauthAccounts: OauthAccount[] = [
    {
      name: 'Himanshu Tripathi',
      email: 'himanshutrip780@gmail.com',
      avatar: 'https://avatar.vercel.sh/himanshu',
      role: 'Admin',
    },
    {
      name: 'Neha Sharma',
      email: 'neha.sharma@workflow.io.com',
      avatar: 'https://avatar.vercel.sh/neha',
      role: 'Admin',
    },
    {
      name: 'Rohit Verma',
      email: 'rohit.verma@workflow.io.com',
      avatar: 'https://avatar.vercel.sh/rohit',
      role: 'Developer',
    },
    {
      name: 'Sneha Rao',
      email: 'sneha.rao@workflow.io.com',
      avatar: 'https://avatar.vercel.sh/sneha',
      role: 'QA Engineer',
    },
    {
      name: 'John Doe',
      email: 'john.doe@indusbank.com',
      avatar: 'https://avatar.vercel.sh/john',
      role: 'Client',
    }
  ];

  // Initialize with empty placeholders instead of hardcoded values
  readonly form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    rememberMe: [true],
  });

  // Handle popstate back events for OAuth modal
  private oauthModalCleanup: (() => void) | null = null;
  private readonly oauthModalEffect = effect(() => {
    const provider = this.activeSocialProvider();
    if (provider) {
      this.oauthModalCleanup = this.backButtonService.registerHandler(
        'OAuthModal',
        15, // High priority to run before routes
        () => {
          this.closeSocialLogin();
          return true; // Consumed back action
        }
      );
    } else {
      if (this.oauthModalCleanup) {
        this.oauthModalCleanup();
        this.oauthModalCleanup = null;
      }
    }
  });

  ngOnInit(): void {
    // Sync local state to theme
    const savedTheme = localStorage.getItem('theme') || 'light';
    this.isDarkMode.set(savedTheme === 'dark');
    this.applyTheme(savedTheme);
  }

  toggleTheme(): void {
    const targetTheme = this.isDarkMode() ? 'light' : 'dark';
    this.isDarkMode.set(targetTheme === 'dark');
    localStorage.setItem('theme', targetTheme);
    this.applyTheme(targetTheme);
  }

  private applyTheme(theme: string): void {
    if (theme === 'dark') {
      document.documentElement.classList.add('dark-theme');
      document.documentElement.setAttribute('data-theme', 'dark');
    } else {
      document.documentElement.classList.remove('dark-theme');
      document.documentElement.setAttribute('data-theme', 'light');
    }
  }

  togglePassword(): void {
    this.showPassword.update((val) => !val);
  }

  emailInvalid(): boolean {
    const control = this.form.controls.email;
    return control.invalid && (control.dirty || control.touched);
  }

  passwordInvalid(): boolean {
    const control = this.form.controls.password;
    return control.invalid && (control.dirty || control.touched);
  }

  serverErrorFor(field: string): string | null {
    const errors = this.auth.fieldErrors();
    const key = Object.keys(errors).find(
      (k) => k.toLowerCase() === field.toLowerCase(),
    );
    return key ? (errors[key]?.[0] ?? null) : null;
  }

  onForgotPassword(): void {
    alert('A password reset link has been requested. If this email exists in the system, you will receive reset instructions shortly.');
  }

  socialLogin(platform: string): void {
    this.activeSocialProvider.set(platform);
    this.socialAuthenticating.set(false);
  }

  closeSocialLogin(): void {
    this.activeSocialProvider.set(null);
    this.socialAuthenticating.set(false);
  }

  selectOauthAccount(email: string): void {
    this.socialAuthenticating.set(true);
    
    // Simulate secure handshakes
    setTimeout(() => {
      const returnUrl =
        this.route.snapshot.queryParamMap.get('returnUrl') ?? '/projects';

      // Call authenticating service under the hood with chosen identity
      this.auth.login({ email, password: 'Password123' }).subscribe({
        next: () => {
          this.closeSocialLogin();
          void this.router.navigateByUrl(returnUrl);
        },
        error: () => {
          this.closeSocialLogin();
        }
      });
    }, 1200);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const returnUrl =
      this.route.snapshot.queryParamMap.get('returnUrl') ?? '/projects';

    const loginData = {
      email: this.form.controls.email.value,
      password: this.form.controls.password.value,
    };

    this.auth.login(loginData).subscribe({
      next: () => {
        void this.router.navigateByUrl(returnUrl);
      },
    });
  }
}
