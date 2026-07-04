import { Injectable, inject } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { Location } from '@angular/common';
import { filter } from 'rxjs/operators';

export interface BackHandler {
  name: string;
  priority: number;
  handle(): boolean; // Returns true if the back action was consumed/handled
}

@Injectable({ providedIn: 'root' })
export class BackButtonService {
  private readonly router = inject(Router);
  private readonly location = inject(Location);
  
  private handlers: BackHandler[] = [];
  private history: string[] = [];
  private exitClicks = 0;
  private exitTimer: any = null;

  constructor() {
    this.initHistoryTracking();
    this.initBrowserBackHandling();
    this.initCapacitorHandling();
  }

  /**
   * Registers a back button handler.
   * Higher priority handlers are executed first.
   * Returns a cleanup function to unregister the handler.
   */
  registerHandler(name: string, priority: number, handle: () => boolean): () => void {
    const handler: BackHandler = { name, priority, handle };
    this.handlers.push(handler);
    // Sort descending by priority (higher priority first)
    this.handlers.sort((a, b) => b.priority - a.priority);

    return () => {
      this.handlers = this.handlers.filter(h => h !== handler);
    };
  }

  private initHistoryTracking(): void {
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: any) => {
        const url = event.urlAfterRedirects;
        // Keep track of navigation history in the app session
        if (this.history.length === 0 || this.history[this.history.length - 1] !== url) {
          this.history.push(url);
        }
      });
  }

  private initBrowserBackHandling(): void {
    // Intercept browser back popstate events
    window.addEventListener('popstate', () => {
      if (this.handlers.length > 0) {
        // Prevent URL change by pushing state back to current location
        history.pushState(null, '', window.location.href);
        this.executeHandlers();
      } else {
        // No handlers registered, use system default router back or exit check
        this.handleSystemBack();
      }
    });

    // Seed the history stack to capture popstate even on first page load
    history.pushState(null, '', window.location.href);
  }

  private initCapacitorHandling(): void {
    const win = window as any;
    if (win.Capacitor && win.Capacitor.isPluginAvailable('App')) {
      try {
        const App = win.Capacitor.Plugins.App;
        App.addListener('backButton', () => {
          if (this.handlers.length > 0) {
            this.executeHandlers();
          } else {
            this.handleSystemBack();
          }
        });
      } catch (err) {
        console.warn('Capacitor App plugin listener failed to load', err);
      }
    }
  }

  private executeHandlers(): boolean {
    for (const handler of this.handlers) {
      const consumed = handler.handle();
      if (consumed) {
        return true;
      }
    }
    return false;
  }

  private handleSystemBack(): void {
    const currentUrl = this.router.url.split('?')[0];
    
    // We treat both '/' and '/projects' (overview/dashboard) as our home dashboard pages
    const isHome = currentUrl === '/projects' || currentUrl === '/dashboard' || currentUrl === '/';

    if (!isHome) {
      // RULE 1: If on any inner page, back button navigates to Dashboard/Home page
      if (this.history.length > 1) {
        this.history.pop(); // Remove current route
        const prevRoute = this.history.pop();
        if (prevRoute && prevRoute !== currentUrl) {
          void this.router.navigateByUrl(prevRoute);
          return;
        }
      }
      // Default fallback is the main projects page
      void this.router.navigate(['/projects']);
    } else {
      // RULE 2: If user is on dashboard/home, handle app exit / exit toast check
      this.handleAppExit();
    }
  }

  private handleAppExit(): void {
    const win = window as any;
    this.exitClicks++;
    
    if (this.exitClicks >= 2) {
      if (win.Capacitor && win.Capacitor.isPluginAvailable('App')) {
        const App = win.Capacitor.Plugins.App;
        void App.exitApp();
      } else {
        this.showToast('Exiting application...');
      }
      this.exitClicks = 0;
    } else {
      this.showToast('Press back again to exit');
      if (this.exitTimer) {
        clearTimeout(this.exitTimer);
      }
      this.exitTimer = setTimeout(() => {
        this.exitClicks = 0;
      }, 2000);
    }
  }

  private showToast(message: string): void {
    // Create modern premium toast popup overlay dynamically
    const toast = document.createElement('div');
    toast.className = 'zt-exit-toast';
    toast.innerText = message;
    
    Object.assign(toast.style, {
      position: 'fixed',
      bottom: '3.5rem',
      left: '50%',
      transform: 'translateX(-50%)',
      backgroundColor: 'rgba(15, 23, 42, 0.95)',
      color: '#f8fafc',
      border: '1px solid rgba(99, 102, 241, 0.2)',
      borderRadius: '24px',
      padding: '0.6rem 1.4rem',
      fontSize: '0.825rem',
      fontWeight: '600',
      boxShadow: '0 10px 30px -10px rgba(99, 102, 241, 0.3)',
      backdropFilter: 'blur(8px)',
      zIndex: '999999',
      transition: 'opacity 0.2s cubic-bezier(0.4, 0, 0.2, 1), transform 0.2s cubic-bezier(0.4, 0, 0.2, 1)',
      pointerEvents: 'none',
      opacity: '0',
    });
    
    document.body.appendChild(toast);
    
    requestAnimationFrame(() => {
      toast.style.opacity = '1';
      toast.style.transform = 'translateX(-50%) translateY(-5px)';
    });
    
    setTimeout(() => {
      toast.style.opacity = '0';
      toast.style.transform = 'translateX(-50%) translateY(0)';
      setTimeout(() => {
        toast.remove();
      }, 200);
    }, 1800);
  }
}
