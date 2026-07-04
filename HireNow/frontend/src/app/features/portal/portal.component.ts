import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CandidatePortalService } from '../../core/services/candidate-portal.service';
import { AuthService } from '../../core/services/auth.service';
import { ThemeService } from '../../core/services/theme.service';

@Component({
  selector: 'app-portal',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  template: `
    <!-- 1. LOGIN SCREEN -->
    @if (!isCandidateAuthenticated()) {
      <div class="min-h-screen bg-background text-foreground transition-colors duration-300 font-sans flex items-center justify-center p-6 relative">
        <!-- Theme Toggle in upper right corner of Login screen -->
        <div class="absolute top-6 right-6">
          <button 
            (click)="themeService.toggleTheme()"
            class="p-2.5 rounded-[14px] hover:bg-secondary text-muted-foreground hover:text-foreground transition-all duration-200 cursor-pointer bg-card border border-border shadow-level1"
            [title]="themeService.isDark() ? 'Switch to Light Mode' : 'Switch to Dark Mode'"
          >
            <svg *ngIf="themeService.isDark()" class="w-5 h-5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M12 3v2.25m0 13.5V21M5.25 5.25l1.626 1.626m10.248 10.248 1.626 1.626M3 12h2.25m13.5 0H21M5.25 18.75l1.626-1.626m10.248-10.248 1.626-1.626M12 7.5a4.5 4.5 0 1 0 0 9 4.5 4.5 0 0 0 0-9Z"/></svg>
            <svg *ngIf="!themeService.isDark()" class="w-5 h-5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M21.752 15.002A9.718 9.718 0 0 1 18 15.75c-5.385 0-9.75-4.365-9.75-9.75 0-1.33.266-2.597.748-3.752A9.753 9.753 0 0 0 3 11.25C3 16.635 7.365 21 12.75 21a9.753 9.753 0 0 0 9.002-5.998Z"/></svg>
          </button>
        </div>

        <div class="relative group max-w-md w-full animate-fade-in-up">
          <!-- Glowing background gradient -->
          <div class="absolute -inset-1.5 rounded-[22px] bg-gradient-to-r from-primary/20 via-violet-500/15 to-primary/20 opacity-70 blur-2xl transition-premium group-hover:opacity-100 animate-pulse"></div>
          <div class="glass-card relative p-8 space-y-6 transition-premium hover:shadow-2xl text-xs">
            <div class="text-center space-y-3 select-none">
              <div class="w-12 h-12 rounded-[14px] bg-primary flex items-center justify-center text-white shrink-0 shadow-md mx-auto text-xl">
                ⚡
              </div>
              <h2 class="text-2xl font-black tracking-tight text-foreground mt-2">Candidate Workspace</h2>
              <p class="text-xs text-muted-foreground">Sign in to track applications, accept offers, and coordinate interviews.</p>
            </div>

            @if (loginStep() === 'request') {
              <form [formGroup]="loginForm" (ngSubmit)="onRequestCode()" class="space-y-4">
                <div class="space-y-1">
                  <label class="block font-bold text-muted-foreground uppercase tracking-wider text-[10px]">Email Address</label>
                  <input type="email" formControlName="email" placeholder="you@example.com" class="input-field transition-premium py-2.5 px-3 text-xs w-full focus:border-violet-500 focus:ring-4 focus:ring-violet-500/10 outline-none hover:border-violet-500/50">
                </div>

                @if (errorMsg()) {
                  <div class="p-2.5 bg-destructive/10 border border-destructive/20 text-destructive font-semibold rounded-[14px] animate-fade-in-up">
                    {{ errorMsg() }}
                  </div>
                }

                <button 
                  type="submit" 
                  [disabled]="loginForm.invalid || loading()" 
                  class="btn-primary transition-premium hover-scale w-full py-2.5 text-xs font-bold"
                >
                  @if (loading()) {
                    <span class="animate-pulse">Sending login code...</span>
                  } @else {
                    <span>Send Login Code</span>
                  }
                </button>
              </form>
            } @else {
              <form [formGroup]="verifyForm" (ngSubmit)="onVerifyCode()" class="space-y-4">
                <div class="p-3 bg-secondary/60 border border-border text-muted-foreground rounded-[14px]">
                  Secure code sent to <strong>{{ loginForm.value.email }}</strong>. Please check your email (and server logs).
                </div>

                <div class="space-y-1">
                  <label class="block font-bold text-muted-foreground uppercase tracking-wider text-[10px]">6-Digit Code</label>
                  <input formControlName="code" maxlength="6" class="input-field transition-premium py-2.5 px-3 text-center tracking-widest text-xs w-full font-bold focus:border-violet-500 focus:ring-4 focus:ring-violet-500/10 outline-none hover:border-violet-500/50">
                </div>

                @if (errorMsg()) {
                  <div class="p-2.5 bg-destructive/10 border border-destructive/20 text-destructive font-semibold rounded-[14px] animate-fade-in-up">
                    {{ errorMsg() }}
                  </div>
                }

                <div class="flex gap-2">
                  <button 
                    type="button" 
                    (click)="loginStep.set('request'); verifyForm.reset(); errorMsg.set(null)"
                    class="btn-secondary transition-premium hover-scale w-1/3 py-2.5 text-xs font-bold"
                  >
                    Back
                  </button>
                  <button 
                    type="submit" 
                    [disabled]="verifyForm.invalid || loading()" 
                    class="btn-primary transition-premium hover-scale w-2/3 py-2.5 text-xs font-bold"
                  >
                    @if (loading()) {
                      <span class="animate-pulse">Verifying...</span>
                    } @else {
                      <span>Verify Code</span>
                    }
                  </button>
                </div>
              </form>
            }
          </div>
        </div>
      </div>
    }

    <!-- 2. AUTHENTICATED WORKSPACE SHELL -->
    @else {
      <div class="flex h-screen w-screen overflow-hidden bg-background text-foreground transition-colors duration-300 font-sans">
        
        <!-- Left Side: Navigation Sidebar -->
        <aside 
          [class.w-[280px]]="!isSidebarCollapsed()" 
          [class.w-20]="isSidebarCollapsed()" 
          class="flex flex-col bg-card border-r border-border transition-all duration-300 h-full select-none z-30 shrink-0 overflow-hidden"
        >
          <!-- Logo Header -->
          <div 
            class="h-[72px] flex items-center relative shrink-0 border-b border-border transition-all duration-300"
            [class.px-6]="!isSidebarCollapsed()"
            [class.px-0]="isSidebarCollapsed()"
            [class.justify-between]="!isSidebarCollapsed()"
            [class.justify-center]="isSidebarCollapsed()"
          >
            <div class="flex items-center gap-3 select-none">
              <div class="w-8 h-8 rounded-[10px] bg-primary flex items-center justify-center text-white shrink-0 shadow-md">
                ⚡
              </div>
              <span *ngIf="!isSidebarCollapsed()" class="text-base font-extrabold tracking-tight text-foreground">HireNow</span>
            </div>
            <button 
              *ngIf="!isSidebarCollapsed()"
              (click)="isSidebarCollapsed.set(true)" 
              class="p-1.5 rounded-[8px] hover:bg-secondary text-muted-foreground hover:text-foreground cursor-pointer transition-colors"
            >
              <svg class="w-4 h-4" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 19.5 8.25 12l7.5-7.5" />
              </svg>
            </button>
          </div>

          <!-- Identity Card -->
          <div class="px-4 py-4 border-b border-border shrink-0 relative">
            <div class="flex items-center gap-3 p-2 rounded-[14px] flex-1 min-w-0 border border-border bg-secondary/20 transition-premium">
              <div class="w-7 h-7 rounded-[8px] bg-primary flex items-center justify-center text-white font-bold text-xs shrink-0 shadow-sm">
                {{ candidateName() ? candidateName().charAt(0).toUpperCase() : 'C' }}
              </div>
              <div class="flex-1 min-w-0 flex flex-col text-left" *ngIf="!isSidebarCollapsed()">
                <span class="text-xs font-bold tracking-tight truncate leading-none text-foreground">{{ candidateName() }}</span>
                <span class="text-[9px] font-semibold text-muted-foreground mt-1 truncate leading-none">Active Candidate</span>
              </div>
            </div>
          </div>

          <!-- Sidebar Navigation -->
          <nav class="flex-1 px-3 py-4 space-y-2 overflow-y-auto">
            <button 
              (click)="activeTab.set('applications')" 
              [class.active-nav-item]="activeTab() === 'applications'"
              class="flex items-center gap-3 px-3.5 py-2.5 rounded-[14px] text-muted-foreground hover:text-foreground transition-premium cursor-pointer group relative text-xs w-full text-left"
              [title]="isSidebarCollapsed() ? 'Applications' : ''"
            >
              <span class="active-indicator absolute left-0 top-2.5 bottom-2.5 w-1 rounded-r bg-white opacity-0 transition-opacity duration-300 shadow-[0_0_8px_#ffffff]"></span>
              <div class="w-5 h-5 flex items-center justify-center shrink-0">
                <svg class="w-5 h-5 text-muted-foreground group-hover:text-foreground" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M20.25 14.15v4.25c0 .9-.73 1.63-1.63 1.63H5.38c-.9 0-1.63-.72-1.63-1.62v-4.25m16.5 0a2.18 2.18 0 0 0 .07-.54V8c0-.9-.73-1.63-1.63-1.63h-2.25m-12.75 9.38c-.06-.17-.07-.36-.07-.53V8c0-.9.73-1.63 1.63-1.63h2.25m3.75 3c0-.9.73-1.63 1.63-1.63h3.75c.9 0 1.63.73 1.63 1.63v4.5m-7 0H21m-18 0h3.75M9 6.38V4.5c0-.9.73-1.63 1.63-1.63h2.75c.9 0 1.62.73 1.62 1.63v1.88"/></svg>
              </div>
              <span *ngIf="!isSidebarCollapsed()" class="font-bold tracking-tight">Applications</span>
            </button>

            <button 
              (click)="activeTab.set('interviews')" 
              [class.active-nav-item]="activeTab() === 'interviews'"
              class="flex items-center gap-3 px-3.5 py-2.5 rounded-[14px] text-muted-foreground hover:text-foreground transition-premium cursor-pointer group relative text-xs w-full text-left"
              [title]="isSidebarCollapsed() ? 'Interviews' : ''"
            >
              <span class="active-indicator absolute left-0 top-2.5 bottom-2.5 w-1 rounded-r bg-white opacity-0 transition-opacity duration-300 shadow-[0_0_8px_#ffffff]"></span>
              <div class="w-5 h-5 flex items-center justify-center shrink-0">
                <svg class="w-5 h-5 text-muted-foreground group-hover:text-foreground" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M6.75 3v2.25M17.25 3v2.25M3 18.75V7.5a2.25 2.25 0 0 1 2.25-2.25h13.5A2.25 2.25 0 0 1 21 7.5v11.25m-18 0A2.25 2.25 0 0 0 5.25 21h13.5A2.25 2.25 0 0 0 21 18.75m-18 0v-7.5A2.25 2.25 0 0 1 5.25 9h13.5A2.25 2.25 0 0 1 21 11.25v7.5m-9-6h.008v.008H12v-.008ZM12 15h.008v.008H12V15Zm0 2.25h.008v.008H12v-.008ZM9.75 15h.008v.008H9.75V15Zm0 2.25h.008v.008H9.75v-.008ZM7.5 15h.008v.008H7.5V15Zm0 2.25h.008v.008H7.5v-.008Zm6.75-4.5h.008v.008h-.008v-.008Zm0 2.25h.008v.008h-.008V15Zm0 2.25h.008v.008h-.008v-.008Zm2.25-4.5h.008v.008H16.5v-.008Zm0 2.25h.008v.008H16.5V15Z"/></svg>
              </div>
              <span *ngIf="!isSidebarCollapsed()" class="font-bold tracking-tight">Interviews</span>
            </button>

            <button 
              (click)="activeTab.set('offers')" 
              [class.active-nav-item]="activeTab() === 'offers'"
              class="flex items-center gap-3 px-3.5 py-2.5 rounded-[14px] text-muted-foreground hover:text-foreground transition-premium cursor-pointer group relative text-xs w-full text-left"
              [title]="isSidebarCollapsed() ? 'Offers' : ''"
            >
              <span class="active-indicator absolute left-0 top-2.5 bottom-2.5 w-1 rounded-r bg-white opacity-0 transition-opacity duration-300 shadow-[0_0_8px_#ffffff]"></span>
              <div class="w-5 h-5 flex items-center justify-center shrink-0">
                <svg class="w-5 h-5 text-muted-foreground group-hover:text-foreground" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M19.5 14.25v-2.625a3.375 3.375 0 0 0-3.375-3.375h-1.5A1.125 1.125 0 0 1 13.5 7.125v-1.5a3.375 3.375 0 0 0-3.375-3.375H8.25m0 12.75h7.5m-7.5 3H12M10.5 2.25H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 0 0-9-9Z"/></svg>
              </div>
              <span *ngIf="!isSidebarCollapsed()" class="font-bold tracking-tight">Offers</span>
            </button>

            <button 
              (click)="activeTab.set('documents')" 
              [class.active-nav-item]="activeTab() === 'documents'"
              class="flex items-center gap-3 px-3.5 py-2.5 rounded-[14px] text-muted-foreground hover:text-foreground transition-premium cursor-pointer group relative text-xs w-full text-left"
              [title]="isSidebarCollapsed() ? 'Documents' : ''"
            >
              <span class="active-indicator absolute left-0 top-2.5 bottom-2.5 w-1 rounded-r bg-white opacity-0 transition-opacity duration-300 shadow-[0_0_8px_#ffffff]"></span>
              <div class="w-5 h-5 flex items-center justify-center shrink-0">
                <svg class="w-5 h-5 text-muted-foreground group-hover:text-foreground" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M18.375 12.739l-7.693 7.693a4.5 4.5 0 01-6.364-6.364l10.94-10.94A3 3 0 1119.5 7.372L8.55 18.32a1.5 1.5 0 11-2.122-2.121L16.272 7.27"/></svg>
              </div>
              <span *ngIf="!isSidebarCollapsed()" class="font-bold tracking-tight">Documents</span>
            </button>
          </nav>

          <!-- Sidebar Footer -->
          <div class="p-3.5 border-t border-border flex flex-col gap-1.5 relative shrink-0">
            <!-- Theme Toggle -->
            <button 
              (click)="themeService.toggleTheme()"
              class="flex items-center gap-3 px-3.5 py-2.5 rounded-[14px] text-muted-foreground hover:text-foreground transition-all duration-200 cursor-pointer text-xs w-full text-left group"
              [title]="themeService.isDark() ? 'Switch to Light Mode' : 'Switch to Dark Mode'"
            >
              <div class="w-5 h-5 flex items-center justify-center shrink-0">
                <svg *ngIf="themeService.isDark()" class="w-5 h-5 animate-fade-in" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M12 3v2.25m0 13.5V21M5.25 5.25l1.626 1.626m10.248 10.248 1.626 1.626M3 12h2.25m13.5 0H21M5.25 18.75l1.626-1.626m10.248-10.248 1.626-1.626M12 7.5a4.5 4.5 0 1 0 0 9 4.5 4.5 0 0 0 0-9Z"/></svg>
                <svg *ngIf="!themeService.isDark()" class="w-5 h-5 animate-fade-in" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M21.752 15.002A9.718 9.718 0 0 1 18 15.75c-5.385 0-9.75-4.365-9.75-9.75 0-1.33.266-2.597.748-3.752A9.753 9.753 0 0 0 3 11.25C3 16.635 7.365 21 12.75 21a9.753 9.753 0 0 0 9.002-5.998Z"/></svg>
              </div>
              <span *ngIf="!isSidebarCollapsed()" class="font-bold tracking-tight">Theme Mode</span>
            </button>

            <!-- User Profile Section -->
            <div class="mt-2 pt-2 border-t border-border flex flex-col gap-1.5">
              <div 
                (click)="isProfileMenuOpen.set(!isProfileMenuOpen())" 
                class="flex items-center justify-between gap-2.5 cursor-pointer hover:bg-secondary/60 p-1.5 rounded-[14px] transition-premium hover-scale"
                [title]="isSidebarCollapsed() ? candidateName() + ' (Candidate)' : ''"
              >
                <div class="flex items-center gap-2.5 min-w-0">
                  <div class="w-8 h-8 rounded-full bg-primary/10 text-primary border border-primary/20 flex items-center justify-center font-bold text-xs shrink-0">
                    {{ candidateName() ? candidateName().charAt(0).toUpperCase() : 'C' }}
                  </div>
                  <div class="flex-1 min-w-0 flex flex-col leading-none" *ngIf="!isSidebarCollapsed()">
                    <span class="text-xs font-bold text-foreground truncate">{{ candidateName() }}</span>
                    <span class="text-[9px] font-semibold text-muted-foreground mt-1 truncate">Candidate</span>
                  </div>
                </div>
                <svg *ngIf="!isSidebarCollapsed()" class="w-3.5 h-3.5 text-muted-foreground shrink-0" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="m8.25 4.5 7.5 7.5-7.5 7.5"/></svg>
              </div>
            </div>

            <!-- Profile Menu Dropdown -->
            <div *ngIf="isProfileMenuOpen()" [class.left-3]="!isSidebarCollapsed()" [class.left-20]="isSidebarCollapsed()" class="absolute bottom-16 w-54 bg-card border border-border rounded-[20px] shadow-level2 py-1.5 z-40 animate-slide-down text-[10px]">
              <div class="px-3 py-2 border-b border-border">
                <span class="block font-bold text-foreground truncate">{{ candidateName() }}</span>
                <span class="block text-[9px] text-muted-foreground truncate mt-0.5">Candidate</span>
              </div>
              <a (click)="onLogout()" class="flex items-center gap-2 px-3 py-2 text-destructive hover:bg-destructive/5 cursor-pointer font-bold mt-1 transition-colors">
                <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M15.75 9V5.25A2.25 2.25 0 0 0 13.5 3h-6a2.25 2.25 0 0 0-2.25 2.25v13.5A2.25 2.25 0 0 0 7.5 21h6a2.25 2.25 0 0 0 2.25-2.25V15M12 9l-3 3m0 0 3 3m-3-3h12.75"/></svg>
                <span>Logout</span>
              </a>
            </div>
          </div>
        </aside>

        <!-- Right Side: Content Shell -->
        <div class="flex-1 flex flex-col h-full overflow-hidden bg-background">
          
          <!-- Topbar Header -->
          <header class="h-[72px] bg-card border-b border-border px-6 flex items-center justify-between z-20 shrink-0">
            <div class="flex items-center gap-3">
              <button 
                *ngIf="isSidebarCollapsed()"
                (click)="isSidebarCollapsed.set(false)"
                class="p-2 rounded-[14px] hover:bg-secondary/60 text-muted-foreground hover:text-foreground cursor-pointer transition-colors"
              >
                <svg class="w-5 h-5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25h16.5" />
                </svg>
              </button>
              <span class="text-base font-extrabold tracking-tight text-foreground capitalize">
                {{ activeTab() }}
              </span>
            </div>

            <div class="flex items-center gap-3">
              <span class="text-xs text-muted-foreground font-semibold">Workspace Mode: Candidate</span>
            </div>
          </header>

          <!-- Main Content Viewport -->
          <main class="flex-1 overflow-y-auto p-8 scrollbar-thin">
            <div class="max-w-[1440px] mx-auto w-full">
              
              <!-- Tab 1: Applications -->
              @if (activeTab() === 'applications') {
                <div class="space-y-6 animate-fade-in-up">
                  <div class="flex flex-col gap-1">
                    <span class="text-xs font-bold uppercase tracking-wider text-muted-foreground">Candidate Progress</span>
                    <h1 class="text-3xl font-extrabold tracking-tight text-foreground">Your Applications</h1>
                  </div>
                  
                  @if (loadingData()) {
                    <div class="flex justify-center py-12">
                      <span class="animate-spin border-2 border-primary/30 border-t-primary rounded-full w-6 h-6"></span>
                    </div>
                  } @else {
                    <div class="grid grid-cols-1 gap-4">
                      @for (app of applications(); track app.id; let idx = $index) {
                        <div 
                          class="bg-card border border-border p-6 rounded-[20px] shadow-level1 flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 transition-premium hover-scale animate-fade-in-up"
                          [style.animation-delay]="(idx * 75) + 'ms'"
                        >
                          <div class="space-y-1.5">
                            <h4 class="text-base font-bold text-foreground">{{ app.jobTitle }}</h4>
                            <div class="flex items-center gap-2 text-xs text-muted-foreground">
                              <span>Applied on: {{ app.createdDate | date:'mediumDate' }}</span>
                              <span>•</span>
                              <span>Ref ID: {{ app.id.substring(0, 8) }}</span>
                            </div>
                          </div>
                          <div class="flex items-center gap-2.5">
                            <span class="px-3 py-1 bg-primary/10 border border-primary/20 text-primary rounded-full text-xs font-bold">
                              Stage: {{ app.currentStage }}
                            </span>
                            <span 
                              [class]="app.status === 'Active' ? 'bg-amber-500/10 border-amber-500/20 text-amber-600 dark:text-amber-400' : (app.status === 'Hired' ? 'bg-emerald-500/10 border-emerald-500/20 text-emerald-600 dark:text-emerald-400' : 'bg-red-500/10 border-red-500/20 text-red-600 dark:text-red-400')"
                              class="px-3 py-1 border rounded-full text-xs font-bold"
                            >
                              {{ app.status }}
                            </span>
                          </div>
                        </div>
                      }

                      @if (applications().length === 0) {
                        <div class="bg-card border border-border rounded-[20px] p-12 text-center text-muted-foreground text-sm shadow-level1">
                          No active applications found. Explore open roles on our Careers page.
                        </div>
                      }
                    </div>
                  }
                </div>
              }

              <!-- Tab 2: Interviews -->
              @if (activeTab() === 'interviews') {
                <div class="space-y-6 animate-fade-in-up">
                  <div class="flex flex-col gap-1">
                    <span class="text-xs font-bold uppercase tracking-wider text-muted-foreground">Upcoming Agenda</span>
                    <h1 class="text-3xl font-extrabold tracking-tight text-foreground">Scheduled Interviews</h1>
                  </div>
                  
                  @if (loadingData()) {
                    <div class="flex justify-center py-12">
                      <span class="animate-spin border-2 border-primary/30 border-t-primary rounded-full w-6 h-6"></span>
                    </div>
                  } @else {
                    <div class="grid grid-cols-1 gap-4">
                      @for (intv of interviews(); track intv.id; let idx = $index) {
                        <div 
                          class="bg-card border border-border p-6 rounded-[20px] shadow-level1 space-y-4 transition-premium hover-scale animate-fade-in-up"
                          [style.animation-delay]="(idx * 75) + 'ms'"
                        >
                          <div class="flex justify-between items-start">
                            <div class="space-y-1">
                              <h4 class="text-base font-bold text-foreground">{{ intv.title }}</h4>
                              <p class="text-xs text-muted-foreground">{{ intv.jobTitle }} • {{ intv.type }}</p>
                            </div>
                            <span class="px-3 py-1 bg-emerald-500/10 border border-emerald-500/20 text-emerald-600 dark:text-emerald-400 rounded-full text-xs font-bold">
                              {{ intv.status }}
                            </span>
                          </div>

                          <div class="grid grid-cols-1 md:grid-cols-2 gap-4 text-xs pt-4 border-t border-border">
                            <div class="space-y-1">
                              <span class="block text-[10px] font-bold text-muted-foreground uppercase tracking-wider">Scheduled Time</span>
                              <strong class="text-foreground font-semibold text-sm">{{ intv.scheduledTime | date:'medium' }} ({{ intv.durationMinutes }} mins)</strong>
                            </div>
                            <div class="space-y-1">
                              <span class="block text-[10px] font-bold text-muted-foreground uppercase tracking-wider">Meeting Link</span>
                              @if (intv.videoLink) {
                                <a [href]="intv.videoLink" target="_blank" class="btn-secondary transition-premium py-1.5 px-4 font-bold text-xs inline-flex items-center gap-1.5">
                                  <span>Join Video Meeting</span>
                                  <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                                    <path stroke-linecap="round" stroke-linejoin="round" d="M13.5 6H5.25A2.25 2.25 0 0 0 3 8.25v10.5A2.25 2.25 0 0 0 5.25 21h10.5A2.25 2.25 0 0 0 18 18.75V10.5m-10.5 6L21 3m0 0h-5.25M21 3v5.25" />
                                  </svg>
                                </a>
                              } @else {
                                <span class="text-muted-foreground italic text-xs">Link will be generated</span>
                              }
                            </div>
                          </div>
                        </div>
                      }

                      @if (interviews().length === 0) {
                        <div class="bg-card border border-border rounded-[20px] p-12 text-center text-muted-foreground text-sm shadow-level1">
                          No interviews scheduled currently. Keep an eye on your email for updates.
                        </div>
                      }
                    </div>
                  }
                </div>
              }

              <!-- Tab 3: Offers -->
              @if (activeTab() === 'offers') {
                <div class="space-y-6 animate-fade-in-up">
                  <div class="flex flex-col gap-1">
                    <span class="text-xs font-bold uppercase tracking-wider text-muted-foreground">Action Required</span>
                    <h1 class="text-3xl font-extrabold tracking-tight text-foreground">Job Offers</h1>
                  </div>
                  
                  @if (loadingData()) {
                    <div class="flex justify-center py-12">
                      <span class="animate-spin border-2 border-primary/30 border-t-primary rounded-full w-6 h-6"></span>
                    </div>
                  } @else {
                    <div class="grid grid-cols-1 gap-6">
                      @for (off of offers(); track off.id; let idx = $index) {
                        <div 
                          class="bg-card border border-border p-6 md:p-8 rounded-[20px] shadow-level1 space-y-6 transition-premium hover-scale animate-fade-in-up"
                          [style.animation-delay]="(idx * 75) + 'ms'"
                        >
                          <div class="flex justify-between items-start border-b border-border pb-4">
                            <div class="space-y-1">
                              <h4 class="text-lg font-bold text-foreground">Offer for {{ off.jobTitle }}</h4>
                              <p class="text-xs text-muted-foreground">Proposed Start Date: {{ off.startDate | date:'mediumDate' }}</p>
                            </div>
                            <span 
                              [class]="off.status === 'Accepted' ? 'bg-emerald-500/10 border-emerald-500/20 text-emerald-600 dark:text-emerald-400' : 'bg-primary/10 border-primary/20 text-primary'"
                              class="px-3 py-1 border rounded-full text-xs font-bold"
                            >
                              {{ off.status }}
                            </span>
                          </div>

                          <div class="space-y-4">
                            <div class="space-y-1">
                              <span class="block text-[10px] font-bold text-muted-foreground uppercase tracking-wider">Offered Salary</span>
                              <strong class="text-foreground text-2xl font-black">{{ off.salary | currency }} <span class="text-xs text-muted-foreground font-normal">/ year</span></strong>
                            </div>
                            <div class="space-y-2">
                              <span class="block text-[10px] font-bold text-muted-foreground uppercase tracking-wider">Offer Letter Document</span>
                              <div class="bg-slate-50 dark:bg-slate-900 border border-border text-foreground shadow-inner rounded-[14px] p-6 whitespace-pre-line text-left leading-relaxed text-xs font-sans max-h-96 overflow-y-auto">
                                {{ off.offerLetterContent }}
                              </div>
                            </div>
                          </div>

                          @if (off.status === 'Sent') {
                            <!-- Signature inputs -->
                            <div class="relative group pt-4">
                              <div class="absolute -inset-1 rounded-[16px] bg-gradient-to-r from-violet-600/10 to-indigo-600/10 blur-md opacity-70"></div>
                              <div class="bg-card border border-border relative p-5 rounded-[14px] space-y-4 shadow-level1">
                                <span class="block text-[10px] font-bold uppercase tracking-wider text-muted-foreground">Offer E-Signature Validation</span>
                                <div class="flex flex-col sm:flex-row gap-4 items-end">
                                  <div class="flex-1 w-full space-y-1.5">
                                    <label class="block text-xs font-semibold text-muted-foreground">Type Full Name to Sign</label>
                                    <input 
                                      [(ngModel)]="signatureInput" 
                                      placeholder="Jane Doe" 
                                      class="input-field transition-premium py-2 px-3.5 text-xs w-full bg-secondary/30 focus:border-violet-500 focus:ring-4 focus:ring-violet-500/10 outline-none hover:border-violet-500/50"
                                    >
                                  </div>
                                  <button 
                                    (click)="onAcceptOffer(off.id)"
                                    [disabled]="!signatureInput.trim() || acceptingOffer()"
                                    class="btn-primary transition-premium hover-scale py-2.5 px-6 text-xs font-bold w-full sm:w-auto shrink-0"
                                  >
                                    @if (acceptingOffer()) {
                                      <span>Accepting...</span>
                                    } @else {
                                      <span>Accept & Sign Offer</span>
                                    }
                                  </button>
                                </div>
                              </div>
                            </div>
                          } @else if (off.status === 'Accepted') {
                            <div class="p-4 bg-emerald-500/10 border border-emerald-500/20 text-emerald-600 dark:text-emerald-400 font-semibold rounded-[14px] text-xs flex items-center gap-2">
                              <span>✓</span>
                              <span>Offer Accepted. E-Signature: {{ off.eSignatureDetails }}</span>
                            </div>
                          }
                        </div>
                      }

                      @if (offers().length === 0) {
                        <div class="bg-card border border-border rounded-[20px] p-12 text-center text-muted-foreground text-sm shadow-level1">
                          No active job offers received yet.
                        </div>
                      }
                    </div>
                  }
                </div>
              }

              <!-- Tab 4: Documents -->
              @if (activeTab() === 'documents') {
                <div class="space-y-6 animate-fade-in-up">
                  <div class="flex flex-col gap-1">
                    <span class="text-xs font-bold uppercase tracking-wider text-muted-foreground">Attachments & Records</span>
                    <h1 class="text-3xl font-extrabold tracking-tight text-foreground">Your Documents</h1>
                  </div>
                  
                  @if (loadingData()) {
                    <div class="flex justify-center py-12">
                      <span class="animate-spin border-2 border-primary/30 border-t-primary rounded-full w-6 h-6"></span>
                    </div>
                  } @else {
                    <div class="grid grid-cols-1 gap-3">
                      @for (doc of documents(); track doc.path; let idx = $index) {
                        <div 
                          class="bg-card border border-border p-5 rounded-[20px] shadow-level1 flex justify-between items-center text-xs transition-premium hover-scale animate-fade-in-up"
                          [style.animation-delay]="(idx * 75) + 'ms'"
                        >
                          <div class="flex items-center gap-4">
                            <div class="w-10 h-10 rounded-[10px] bg-primary/10 text-primary flex items-center justify-center text-xl shrink-0">
                              📄
                            </div>
                            <div class="flex flex-col gap-0.5">
                              <strong class="text-foreground text-sm">{{ doc.name }}</strong>
                              <span class="text-[10px] text-muted-foreground uppercase tracking-wider font-bold">{{ doc.type }}</span>
                            </div>
                          </div>
                          <span class="px-2.5 py-0.5 bg-emerald-500/10 border border-emerald-500/20 text-emerald-600 dark:text-emerald-400 rounded-md text-[10px] font-bold">Active</span>
                        </div>
                      }

                      @if (documents().length === 0) {
                        <div class="bg-card border border-border rounded-[20px] p-12 text-center text-muted-foreground text-sm shadow-level1">
                          No documents associated with your profile.
                        </div>
                      }
                    </div>
                  }
                </div>
              }

            </div>
          </main>
        </div>
      </div>
    }
  `,
  styles: [`
    @keyframes slideDown {
      from { opacity: 0; transform: translateY(-8px); }
      to { opacity: 1; transform: translateY(0); }
    }
    .animate-slide-down {
      animation: slideDown 0.25s cubic-bezier(0.16, 1, 0.3, 1) forwards;
    }
    nav button {
      position: relative;
      transition: all 0.25s cubic-bezier(0.16, 1, 0.3, 1);
      
      &:hover:not(.active-nav-item) {
        background-color: var(--secondary);
        color: var(--foreground) !important;
        transform: translateX(2px);
      }
    }
    .active-nav-item {
      background-color: var(--primary) !important;
      color: var(--primary-foreground) !important;
      font-weight: 600;
      box-shadow: 0 4px 12px rgba(124, 58, 237, 0.25);
      
      svg {
        color: var(--primary-foreground) !important;
      }
      
      .active-indicator {
        opacity: 1 !important;
      }
    }
  `]
})
export class PortalComponent implements OnInit {
  private portalService = inject(CandidatePortalService);
  private authService = inject(AuthService);
  public themeService = inject(ThemeService);
  private fb = inject(FormBuilder);
  private router = inject(Router);

  public isCandidateAuthenticated = signal<boolean>(false);
  public candidateName = signal<string>('');
  public loginStep = signal<'request' | 'verify'>('request');
  public loading = signal<boolean>(false);
  public errorMsg = signal<string | null>(null);

  // Shell Layout State Signals
  public isSidebarCollapsed = signal<boolean>(false);
  public isProfileMenuOpen = signal<boolean>(false);

  // Forms
  public loginForm: FormGroup;
  public verifyForm: FormGroup;

  // Dashboard signals
  public activeTab = signal<'applications' | 'interviews' | 'offers' | 'documents'>('applications');
  public loadingData = signal<boolean>(false);
  public applications = signal<any[]>([]);
  public interviews = signal<any[]>([]);
  public offers = signal<any[]>([]);
  public documents = signal<any[]>([]);

  // Action states
  public signatureInput = '';
  public acceptingOffer = signal<boolean>(false);

  constructor() {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });

    this.verifyForm = this.fb.group({
      code: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]]
    });
  }

  public ngOnInit() {
    this.checkSession();
  }

  private checkSession() {
    const user = this.authService.currentUser();
    if (user && user.role === 'Candidate') {
      this.isCandidateAuthenticated.set(true);
      this.candidateName.set(`${user.firstName} ${user.lastName}`);
      this.loadDashboardData();
    } else {
      this.isCandidateAuthenticated.set(false);
    }
  }

  public onRequestCode() {
    if (this.loginForm.invalid) return;
    this.loading.set(true);
    this.errorMsg.set(null);

    this.portalService.requestLoginCode(this.loginForm.value.email).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.isSuccess) {
          this.loginStep.set('verify');
        } else {
          this.errorMsg.set(res.message || 'Email not found.');
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMsg.set(err.error?.message || 'Error processing email.');
      }
    });
  }

  public onVerifyCode() {
    if (this.verifyForm.invalid) return;
    this.loading.set(true);
    this.errorMsg.set(null);

    const email = this.loginForm.value.email;
    const code = this.verifyForm.value.code;

    this.portalService.verifyLoginCode(email, code).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.isSuccess) {
          // Re-load auth state
          window.location.reload();
        } else {
          this.errorMsg.set(res.message || 'Incorrect verification code.');
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMsg.set(err.error?.message || 'Verification error.');
      }
    });
  }

  public loadDashboardData() {
    this.loadingData.set(true);
    this.portalService.getApplications().subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.applications.set(res.data);
        }
        this.loadingData.set(false);
      },
      error: () => this.loadingData.set(false)
    });

    this.portalService.getInterviews().subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.interviews.set(res.data);
        }
      }
    });

    this.portalService.getOffers().subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.offers.set(res.data);
        }
      }
    });

    this.portalService.getDocuments().subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.documents.set(res.data);
        }
      }
    });
  }

  public onAcceptOffer(offerId: string) {
    if (!this.signatureInput.trim()) return;
    this.acceptingOffer.set(true);

    this.portalService.acceptOffer(offerId, this.signatureInput).subscribe({
      next: (res) => {
        this.acceptingOffer.set(false);
        if (res.isSuccess) {
          this.signatureInput = '';
          this.loadDashboardData();
        }
      },
      error: () => this.acceptingOffer.set(false)
    });
  }

  public onLogout() {
    this.authService.logout();
    this.isCandidateAuthenticated.set(false);
    this.loginStep.set('request');
    this.loginForm.reset();
    this.verifyForm.reset();
  }
}
