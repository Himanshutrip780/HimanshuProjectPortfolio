import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { ProjectService } from '../../services/project.service';
import { ProjectResponse } from '../../../../core/models/project.models';
import { UserService } from '../../../../core/services/user.service';
import { UserDto } from '../../../../core/models/user.models';
import { ClientsService, ClientResponse } from '../../services/clients.service';

interface ClientItem {
  clientId: string;
  name: string;
  industry: string;
  contactPerson: string;
  email: string;
  projectsCount: number;
  status: 'Active' | 'Completed' | 'Pending';
  projects: ProjectResponse[];
}

@Component({
  selector: 'app-clients',
  standalone: true,
  templateUrl: './clients.component.html',
  styles: `
    .page-container {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .page-header h1 {
      font-size: 1.5rem;
      font-weight: 700;
      letter-spacing: -0.025em;
      margin: 0;
    }

    .subtitle {
      font-size: 0.85rem;
      color: var(--text-secondary);
      margin-top: 0.15rem;
    }

    .badge-total {
      background-color: var(--primary-glow);
      color: var(--primary-color);
      font-size: 0.8rem;
      font-weight: 700;
      padding: 0.35rem 0.75rem;
      border-radius: var(--radius-md);
      border: 1px solid var(--border-color);
    }

    .clients-list {
      padding: 0;
      overflow: visible;
      border-radius: var(--radius-xl);
      border: 1px solid var(--border-color);
      box-shadow: var(--shadow-sm);
    }

    .table-scroll {
      overflow: visible;
    }

    .clients-table {
      width: 100%;
      border-collapse: collapse;
      font-size: 0.85rem;
      text-align: left;
    }

    .clients-table th {
      background-color: var(--bg-hover);
      color: var(--text-secondary);
      font-weight: 600;
      padding: 0.75rem 1rem;
      border-bottom: 1px solid var(--border-color);
    }

    .clients-table td {
      padding: 0.75rem 1rem;
      border-bottom: 1px solid var(--border-color);
      color: var(--text-primary);
      vertical-align: middle;
    }

    .client-row {
      transition: background-color var(--transition-fast);
    }

    .client-row:hover {
      background-color: var(--bg-hover);
    }

    .client-name-cell {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }

    .client-logo-avatar {
      width: 28px;
      height: 28px;
      border-radius: var(--radius-md);
      color: #fff;
      font-weight: 700;
      display: flex;
      align-items: center;
      justify-content: center;
      text-transform: uppercase;
    }

    .client-name {
      font-weight: 700;
      color: var(--text-primary);
    }

    .industry-tag {
      background-color: var(--bg-hover);
      border: 1px solid var(--border-color);
      padding: 0.15rem 0.45rem;
      border-radius: var(--radius-sm);
      font-size: 0.75rem;
      font-weight: 500;
      color: var(--text-secondary);
    }

    .email-text {
      color: var(--text-secondary);
    }

    .projects-badge-container {
      position: relative;
      display: inline-block;
    }

    .projects-count-badge {
      font-weight: 600;
      font-size: 0.75rem;
      color: var(--primary-color);
      background-color: var(--primary-glow);
      padding: 0.15rem 0.45rem;
      border-radius: var(--radius-sm);
      cursor: pointer;
    }

    /* Premium Tooltip */
    .projects-tooltip {
      position: absolute;
      bottom: 125%;
      left: 50%;
      transform: translateX(-50%) translateY(4px);
      background-color: var(--bg-card, #ffffff);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      padding: 0.6rem 0.8rem;
      width: 260px;
      box-shadow: var(--shadow-lg);
      z-index: 100;
      opacity: 0;
      pointer-events: none;
      transition: opacity var(--transition-fast) ease, transform var(--transition-fast) ease;
    }

    .projects-badge-container:hover .projects-tooltip {
      opacity: 1;
      transform: translateX(-50%) translateY(0);
    }

    .tooltip-header {
      font-size: 0.7rem;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      color: var(--text-secondary);
      margin-bottom: 0.4rem;
      border-bottom: 1px solid var(--border-color);
      padding-bottom: 0.2rem;
    }

    .projects-tooltip ul {
      list-style: none;
      padding: 0;
      margin: 0;
      display: flex;
      flex-direction: column;
      gap: 0.3rem;
    }

    .tooltip-project-item {
      font-size: 0.75rem;
      color: var(--text-primary);
      display: flex;
      gap: 0.4rem;
      align-items: center;
    }

    .tooltip-project-item .project-key {
      font-weight: 700;
      color: var(--primary-color);
      flex-shrink: 0;
    }

    .tooltip-project-item .project-name {
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
      font-weight: 500;
    }

    /* Badges */
    .status-badge {
      font-size: 0.7rem;
      font-weight: 700;
      padding: 0.15rem 0.5rem;
      border-radius: 9999px;
    }

    .status-active { background-color: rgba(16, 185, 129, 0.15); color: var(--success-color); }
    .status-completed { background-color: rgba(99, 102, 241, 0.15); color: var(--primary-color); }
    .status-pending { background-color: rgba(245, 158, 11, 0.15); color: var(--warning-color); }
  `
})
export class ClientsComponent implements OnInit {
  private readonly projectService = inject(ProjectService);
  private readonly userService = inject(UserService);
  private readonly clientsService = inject(ClientsService);
  
  readonly projects = signal<ProjectResponse[]>([]);
  readonly users = signal<UserDto[]>([]);
  readonly clients = signal<ClientResponse[]>([]);
  
  readonly clientsList = computed<ClientItem[]>(() => {
    const projectsList = this.projects();
    const usersList = this.users();
    const dbClients = this.clients();

    const clientItemsMap = new Map<string, ClientItem>();

    // Pre-populate with database client records
    dbClients.forEach(c => {
      clientItemsMap.set(c.clientId, {
        clientId: c.clientId,
        name: c.name,
        industry: c.industry,
        contactPerson: c.contactPerson,
        email: c.email,
        projectsCount: 0,
        status: 'Pending',
        projects: []
      });
    });

    // Group each project dynamically
    projectsList.forEach(proj => {
      const nameLower = proj.name.toLowerCase();
      const descLower = (proj.description || '').toLowerCase();
      const keyLower = proj.key.toLowerCase();

      let matchedClientId: string | null = null;
      
      // Match against database client keywords list
      for (const bc of dbClients) {
        const keywordList = bc.keywords.split(',').map(k => k.trim()).filter(Boolean);
        const matchesKeyword = keywordList.some(kw => 
          nameLower.includes(kw) || descLower.includes(kw) || keyLower.includes(kw)
        );
        if (matchesKeyword) {
          matchedClientId = bc.clientId;
          break;
        }
      }

      if (matchedClientId) {
        const item = clientItemsMap.get(matchedClientId)!;
        item.projects.push(proj);
        item.projectsCount++;
      } else {
        // Dynamically spawn a new Client based on the new project!
        const words = proj.name.split(/\s+/).filter(Boolean);
        const clientName = words.length > 1 ? `${words[0]} ${words[1]}` : proj.name;

        // Resolve owner contact information from User directory
        const owner = usersList.find(u => u.userId === proj.ownerId);
        const contactPerson = owner ? `${owner.firstName} ${owner.lastName}` : 'Project Lead';
        const email = owner ? owner.email : 'contact@workflow.io.com';

        const dynamicClientId = `dynamic-${clientName.toLowerCase().replace(/[^a-z0-9]/g, '-')}`;

        if (clientItemsMap.has(dynamicClientId)) {
          const item = clientItemsMap.get(dynamicClientId)!;
          item.projects.push(proj);
          item.projectsCount++;
        } else {
          clientItemsMap.set(dynamicClientId, {
            clientId: dynamicClientId,
            name: clientName,
            industry: 'Technology',
            contactPerson,
            email,
            projectsCount: 1,
            status: 'Active',
            projects: [proj]
          });
        }
      }
    });

    const result: ClientItem[] = [];
    clientItemsMap.forEach(item => {
      if (item.projects.length === 0) {
        item.status = 'Pending';
      } else {
        const allCompleted = item.projects.every(p => p.status === 2);
        item.status = allCompleted ? 'Completed' : 'Active';
      }
      result.push(item);
    });

    return result;
  });

  ngOnInit(): void {
    this.projectService.getMyProjects().subscribe({
      next: (projects) => {
        this.projects.set(projects);
      }
    });

    this.userService.getAllUsers().subscribe({
      next: (users) => {
        this.users.set(users);
      }
    });

    this.clientsService.getClients().subscribe({
      next: (clients) => {
        this.clients.set(clients);
      }
    });
  }

  getClientStatusClass(status: 'Active' | 'Completed' | 'Pending'): string {
    switch (status) {
      case 'Active': return 'status-active';
      case 'Completed': return 'status-completed';
      case 'Pending': return 'status-pending';
      default: return 'status-active';
    }
  }

  getClientBgColor(name: string): string {
    const colors = ['#8b5cf6', '#3b82f6', '#10b981', '#f59e0b', '#ec4899', '#f43f5e', '#06b6d4'];
    let hash = 0;
    for (let i = 0; i < name.length; i++) {
      hash = name.charCodeAt(i) + ((hash << 5) - hash);
    }
    const index = Math.abs(hash) % colors.length;
    return colors[index];
  }
}
