import { Injectable, signal, effect } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  public themeSignal = signal<'light' | 'dark'>('light');

  constructor() {
    this.initializeTheme();
    
    // Automatically apply theme changes whenever the signal updates
    effect(() => {
      const theme = this.themeSignal();
      const root = document.documentElement;
      
      if (theme === 'dark') {
        root.classList.add('dark');
        root.style.colorScheme = 'dark';
      } else {
        root.classList.remove('dark');
        root.style.colorScheme = 'light';
      }
      
      localStorage.setItem('theme', theme);
    });
  }

  private initializeTheme() {
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme === 'light' || savedTheme === 'dark') {
      this.themeSignal.set(savedTheme);
    } else {
      // Fallback to user system preference
      const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
      this.themeSignal.set(prefersDark ? 'dark' : 'light');
    }
  }

  public toggleTheme() {
    this.themeSignal.update(current => current === 'light' ? 'dark' : 'light');
  }

  public isDark(): boolean {
    return this.themeSignal() === 'dark';
  }
}
