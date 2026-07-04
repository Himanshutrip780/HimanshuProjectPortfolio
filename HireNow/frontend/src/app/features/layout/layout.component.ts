import { Component, inject, signal, HostListener, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ThemeService } from '../../core/services/theme.service';
import { NotificationService } from '../../core/services/notification.service';
import { CompanyService } from '../../core/services/company.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    FormsModule
  ],
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss']
})
export class LayoutComponent {
  public authService = inject(AuthService);
  public themeService = inject(ThemeService);
  private router = inject(Router);
  private companyService = inject(CompanyService);

  // Layout UI states
  public isCollapsed = signal<boolean>(false);
  public isWorkspaceOpen = signal<boolean>(false);
  public isHeaderProfileOpen = signal<boolean>(false);
  public isSidebarProfileOpen = signal<boolean>(false);
  public isNotificationsOpen = signal<boolean>(false);
  public isCommandPaletteOpen = signal<boolean>(false);

  // Organization identity states
  public companyName = signal<string>('Acme Corp');
  public companyLogoUrl = signal<string | null>(null);

  // Command Palette State
  public searchQuery = signal<string>('');
  public selectedIndex = signal<number>(0);

  private notificationService = inject(NotificationService);

  // Notifications State
  public notifications = signal<any[]>([]);
  public unreadCount = computed(() => this.notifications().filter(n => !n.isRead).length);

  constructor() {
    this.loadNotifications();
    this.loadCompanyInfo();
  }

  public loadCompanyInfo() {
    this.companyService.getCompany().subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.companyName.set(res.data.name);
          this.companyLogoUrl.set(res.data.logoUrl);
        }
      }
    });
  }

  public loadNotifications() {
    this.notificationService.getNotifications().subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.notifications.set(res.data);
        }
      }
    });
  }

  public markAsRead(id: string) {
    this.notificationService.markAsRead(id).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.loadNotifications();
        }
      }
    });
  }

  public markAllAsRead() {
    this.notificationService.markAllAsRead().subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.loadNotifications();
        }
      }
    });
  }

  public navItems = [
    { path: '/dashboard', label: 'Dashboard', iconName: 'dashboard' },
    { path: '/jobs', label: 'Jobs', iconName: 'jobs' },
    { path: '/candidates', label: 'Candidates', iconName: 'candidates' },
    { path: '/pipeline', label: 'Pipeline', iconName: 'pipeline' },
    { path: '/interviews', label: 'Interviews', iconName: 'interviews' },
    { path: '/offers', label: 'Offers', iconName: 'offers' },
    { path: '/analytics', label: 'Analytics', iconName: 'analytics' },
    { path: '/settings', label: 'Settings', iconName: 'settings' }
  ];

  private roleRouteMappings: { [key: string]: string[] } = {
    '/dashboard': ['SuperAdmin', 'Recruiter', 'HiringManager', 'Interviewer'],
    '/jobs': ['SuperAdmin', 'Recruiter', 'HiringManager'],
    '/candidates': ['SuperAdmin', 'Recruiter', 'HiringManager'],
    '/pipeline': ['SuperAdmin', 'Recruiter', 'HiringManager'],
    '/interviews': ['SuperAdmin', 'Recruiter', 'HiringManager', 'Interviewer'],
    '/offers': ['SuperAdmin', 'Recruiter'],
    '/analytics': ['SuperAdmin', 'Recruiter'],
    '/settings': ['SuperAdmin']
  };

  public filteredNavItems = computed(() => {
    const role = this.authService.currentUser()?.role || '';
    return this.navItems.filter(item => {
      const allowedRoles = this.roleRouteMappings[item.path];
      return !allowedRoles || allowedRoles.includes(role);
    });
  });

  // List of links inside Ctrl+K command palette
  public commandPaletteItems = [
    { label: 'Go to Dashboard', category: 'Navigation', route: '/dashboard', roles: ['SuperAdmin', 'Recruiter', 'HiringManager', 'Interviewer'] },
    { label: 'Go to Job Openings', category: 'Navigation', route: '/jobs', roles: ['SuperAdmin', 'Recruiter', 'HiringManager'] },
    { label: 'Go to Candidates Directory', category: 'Navigation', route: '/candidates', roles: ['SuperAdmin', 'Recruiter', 'HiringManager'] },
    { label: 'Go to Kanban Board', category: 'Navigation', route: '/pipeline', roles: ['SuperAdmin', 'Recruiter', 'HiringManager'] },
    { label: 'Go to Interview Scheduler', category: 'Navigation', route: '/interviews', roles: ['SuperAdmin', 'Recruiter', 'HiringManager', 'Interviewer'] },
    { label: 'Go to Offers Portal', category: 'Navigation', route: '/offers', roles: ['SuperAdmin', 'Recruiter'] },
    { label: 'Go to Analytics & Settings', category: 'Navigation', route: '/analytics', roles: ['SuperAdmin', 'Recruiter'] },
    { label: 'Go to Workspace Settings', category: 'Navigation', route: '/settings', roles: ['SuperAdmin'] },
    { label: 'Create New Job Requisition', category: 'Actions', action: 'create-job', roles: ['SuperAdmin', 'Recruiter'] },
    { label: 'Upload Candidate Resume', category: 'Actions', action: 'upload-resume', roles: ['SuperAdmin', 'Recruiter'] }
  ];

  // Filter items in command palette based on search query
  public filteredPaletteItems = computed(() => {
    const role = this.authService.currentUser()?.role || '';
    const query = this.searchQuery().toLowerCase().trim();
    
    const roleFiltered = this.commandPaletteItems.filter(item => 
      !item.roles || item.roles.includes(role)
    );

    if (!query) return roleFiltered;
    return roleFiltered.filter(item => 
      item.label.toLowerCase().includes(query) || 
      item.category.toLowerCase().includes(query)
    );
  });

  // Breadcrumb derived from current route
  public getActiveRouteLabel(): string {
    const url = this.router.url;
    if (url.startsWith('/dashboard')) return 'Dashboard';
    if (url.startsWith('/jobs')) return 'Jobs';
    if (url.startsWith('/candidates')) return 'Candidates';
    if (url.startsWith('/pipeline')) return 'Pipeline';
    if (url.startsWith('/interviews')) return 'Interviews';
    if (url.startsWith('/offers')) return 'Offers';
    if (url.startsWith('/analytics')) return 'Analytics';
    if (url.startsWith('/settings')) return 'Settings';
    return 'Recruiter Workspace';
  }

  // Keyboard shortcut listener for Ctrl+K
  @HostListener('window:keydown', ['$event'])
  public handleKeyboardEvents(event: KeyboardEvent) {
    // Check for Ctrl+K or Cmd+K
    if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') {
      event.preventDefault();
      this.toggleCommandPalette();
    }

    // Escape to close
    if (event.key === 'Escape') {
      if (this.isCommandPaletteOpen()) {
        this.closeCommandPalette();
      }
      this.isWorkspaceOpen.set(false);
      this.isHeaderProfileOpen.set(false);
      this.isSidebarProfileOpen.set(false);
      this.isNotificationsOpen.set(false);
    }

    // Keyboard navigation inside command palette
    if (this.isCommandPaletteOpen()) {
      const listLength = this.filteredPaletteItems().length;
      if (event.key === 'ArrowDown') {
        event.preventDefault();
        this.selectedIndex.update(idx => (idx + 1) % listLength);
      } else if (event.key === 'ArrowUp') {
        event.preventDefault();
        this.selectedIndex.update(idx => (idx - 1 + listLength) % listLength);
      } else if (event.key === 'Enter') {
        event.preventDefault();
        this.triggerCommand(this.filteredPaletteItems()[this.selectedIndex()]);
      }
    }
  }

  public toggleSidebar() {
    this.isCollapsed.update(val => !val);
  }

  public toggleWorkspace() {
    this.isWorkspaceOpen.update(val => !val);
    this.isHeaderProfileOpen.set(false);
    this.isSidebarProfileOpen.set(false);
    this.isNotificationsOpen.set(false);
  }

  public toggleHeaderProfile() {
    this.isHeaderProfileOpen.update(val => !val);
    this.isSidebarProfileOpen.set(false);
    this.isWorkspaceOpen.set(false);
    this.isNotificationsOpen.set(false);
  }

  public toggleSidebarProfile() {
    this.isSidebarProfileOpen.update(val => !val);
    this.isHeaderProfileOpen.set(false);
    this.isWorkspaceOpen.set(false);
    this.isNotificationsOpen.set(false);
  }

  public toggleNotifications() {
    this.isNotificationsOpen.update(val => !val);
    this.isWorkspaceOpen.set(false);
    this.isHeaderProfileOpen.set(false);
    this.isSidebarProfileOpen.set(false);
  }

  public toggleCommandPalette() {
    this.isCommandPaletteOpen.update(val => !val);
    this.searchQuery.set('');
    this.selectedIndex.set(0);
  }

  public closeCommandPalette() {
    this.isCommandPaletteOpen.set(false);
  }

  public triggerCommand(item: any) {
    if (!item) return;
    this.closeCommandPalette();
    
    if (item.route) {
      this.router.navigate([item.route]);
    } else if (item.action) {
      if (item.action === 'create-job') {
        this.router.navigate(['/jobs']); // Navigate and open jobs
      } else if (item.action === 'upload-resume') {
        this.router.navigate(['/candidates']); // Navigate and open uploader
      }
    }
  }

  public getFormattedRole(): string {
    const role = this.authService.currentUser()?.role || '';
    if (role === 'SuperAdmin') return 'Super Admin';
    if (role === 'HiringManager') return 'Hiring Manager';
    return role;
  }

  public getUserFullName(): string {
    const user = this.authService.currentUser();
    if (!user) return '';
    return `${user.firstName} ${user.lastName}`;
  }

  public onLogout() {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }
}
