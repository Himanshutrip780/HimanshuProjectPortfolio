import { Component, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

interface SkillGroup {
  category: string;
  icon: string;
  items: string[];
}

interface Experience {
  role: string;
  company: string;
  client?: string;
  period: string;
  responsibilities: string[];
}

interface DeployedProject {
  name: string;
  tagline: string;
  description: string;
  technologies: string[];
  url: string;
  icon: string;
  businessProblem: string;
  technicalChallenge: string;
  results: string;
  lessons: string;
  techStackDetailed: string[];
}

interface ResumeProject {
  name: string;
  technologies: string;
  description: string;
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  // Personal Info
  protected readonly name = 'Himanshu Tripathi';
  protected readonly title = '.NET Full Stack Developer';
  protected readonly subtitle = 'ASP.NET Core | Angular | Azure | SQL Server';
  protected readonly experienceYears = '3.10+ Years Experience';
  protected readonly location = 'Pune, India';
  protected readonly contactPhone = '+91-8858802873';
  protected readonly linkedIn = 'https://linkedin.com/in/himanshu-tripathi780';
  protected readonly gitHub = 'https://github.com/Himanshutrip780';
  
  // Customization & Themes
  protected isLightTheme = signal<boolean>(false);
  protected isRecruiterMode = signal<boolean>(false);

  // Lead capture analytics
  protected totalPageVisits = signal<number>(1);
  protected resumeDownloads = signal<number>(0);
  protected contactSubmissions = signal<number>(0);
  protected avgMessageLength = signal<number>(0);

  // Active Case Study tabs per project
  protected activeTabs = signal<{ [key: string]: string }>({
    'HireNow': 'overview',
    'Workflow.io': 'overview'
  });

  // Contact Form Model
  protected contactModel = {
    name: '',
    email: '',
    subject: '',
    message: ''
  };
  
  protected formStatus = signal<'idle' | 'sending' | 'success' | 'error'>('idle');

  // Inbox Modal State
  protected showInbox = signal<boolean>(false);
  protected inboxMessages = signal<any[]>([]);

  public ngOnInit() {
    this.initTheme();
    this.trackPageVisit();
    this.loadAnalytics();
  }

  protected initTheme() {
    const saved = localStorage.getItem('portfolio_theme');
    const prefersLight = window.matchMedia('(prefers-color-scheme: light)').matches;
    if (saved === 'light' || (!saved && prefersLight)) {
      this.isLightTheme.set(true);
      document.body.classList.add('light-theme');
      document.body.classList.remove('dark-theme');
    } else {
      this.isLightTheme.set(false);
      document.body.classList.remove('light-theme');
      document.body.classList.add('dark-theme');
    }
  }

  protected toggleTheme() {
    this.isLightTheme.update(v => !v);
    if (this.isLightTheme()) {
      document.body.classList.add('light-theme');
      document.body.classList.remove('dark-theme');
      localStorage.setItem('portfolio_theme', 'light');
    } else {
      document.body.classList.remove('light-theme');
      document.body.classList.add('dark-theme');
      localStorage.setItem('portfolio_theme', 'dark');
    }
  }

  protected toggleRecruiterMode() {
    this.isRecruiterMode.update(v => !v);
  }

  protected trackPageVisit() {
    let visits = localStorage.getItem('portfolio_page_visits');
    let visitsNum = visits ? parseInt(visits, 10) + 1 : 1;
    localStorage.setItem('portfolio_page_visits', visitsNum.toString());
    this.totalPageVisits.set(visitsNum);
  }

  protected loadAnalytics() {
    const downloads = localStorage.getItem('portfolio_downloads');
    this.resumeDownloads.set(downloads ? parseInt(downloads, 10) : 0);

    const msgs = localStorage.getItem('portfolio_messages');
    const list = msgs ? JSON.parse(msgs) : [];
    this.inboxMessages.set(list);
    this.contactSubmissions.set(list.length);

    if (list.length > 0) {
      const sum = list.reduce((acc: number, m: any) => acc + (m.message?.length || 0), 0);
      this.avgMessageLength.set(Math.round(sum / list.length));
    } else {
      this.avgMessageLength.set(0);
    }
  }

  protected setProjectTab(projectName: string, tabName: string) {
    this.activeTabs.update(tabs => ({
      ...tabs,
      [projectName]: tabName
    }));
  }

  protected openInboxModal() {
    this.loadLocalMessages();
    this.showInbox.set(true);
  }

  protected closeInboxModal() {
    this.showInbox.set(false);
  }

  protected loadLocalMessages() {
    const msgs = localStorage.getItem('portfolio_messages');
    this.inboxMessages.set(msgs ? JSON.parse(msgs) : []);
  }

  protected saveMessageToLocalHistory(msg: any) {
    const msgs = localStorage.getItem('portfolio_messages');
    const list = msgs ? JSON.parse(msgs) : [];
    list.unshift({ ...msg, date: new Date().toLocaleString() });
    localStorage.setItem('portfolio_messages', JSON.stringify(list));
    this.loadAnalytics();
  }

  protected clearInbox() {
    localStorage.removeItem('portfolio_messages');
    this.inboxMessages.set([]);
    this.loadAnalytics();
  }

  protected scrollToContact() {
    const element = document.getElementById('contact');
    if (element) {
      element.scrollIntoView({ behavior: 'smooth' });
      const nameInput = document.getElementById('name');
      if (nameInput) {
        setTimeout(() => nameInput.focus(), 800);
      }
    }
  }

  // Interactive Skill bar metrics
  protected readonly skillBars = [
    { name: 'ASP.NET Core / C# API', percentage: 95, icon: 'fa-server' },
    { name: 'Angular / TypeScript', percentage: 90, icon: 'fa-code' },
    { name: 'SQL Server / Performance Tuning', percentage: 88, icon: 'fa-database' },
    { name: 'Microservices & RabbitMQ', percentage: 85, icon: 'fa-diagram-project' },
    { name: 'Docker / Azure / DevOps CI-CD', percentage: 80, icon: 'fa-cloud' }
  ];

  // Technical Skills List
  protected readonly skillGroups: SkillGroup[] = [
    {
      category: 'AI & Generative AI',
      icon: 'fa-solid fa-brain',
      items: [
        'Generative AI Workflows', 'LLM Integration', 'NLP Fundamentals', 'Prompt Engineering'
      ]
    },
    {
      category: 'Backend Technologies',
      icon: 'fa-solid fa-server',
      items: [
        'ASP.NET Core', 'C#', 'ASP.NET Web API', 
        'REST APIs', 'Entity Framework Core', 'LINQ', 'JWT Auth & Security'
      ]
    },
    {
      category: 'Frontend Technologies',
      icon: 'fa-solid fa-code',
      items: [
        'Angular', 'TypeScript', 'JavaScript', 
        'HTML5', 'CSS3 / SCSS', 'Bootstrap'
      ]
    },
    {
      category: 'Architecture',
      icon: 'fa-solid fa-sitemap',
      items: [
        'Microservices', 'Clean Architecture', 'SOLID Principles', 'YARP Gateway Routing'
      ]
    },
    {
      category: 'Databases & Message Queues',
      icon: 'fa-solid fa-database',
      items: [
        'SQL Server', 'PostgreSQL', 'RabbitMQ Event Bus', 'Stored Procedures & Tuning'
      ]
    },
    {
      category: 'Cloud and DevOps',
      icon: 'fa-solid fa-cloud',
      items: [
        'Azure Cloud', 'Docker Containers', 'Git Version Control', 
        'Azure DevOps', 'CI/CD Pipelines'
      ]
    }
  ];

  // Work Experience
  protected readonly experiences: Experience[] = [
    {
      role: 'Software Engineer',
      company: 'Zensar Technologies',
      client: 'FIS',
      period: 'Aug 2022 – Present',
      responsibilities: [
        'Developed enterprise web applications using ASP.NET Core, C#, Angular, SQL Server, and Web APIs.',
        'Developed RESTful APIs with JWT authentication, RBAC, and business workflow automation.',
        'Optimized SQL performance and contributed to microservice-based cloud deployments.',
        'Participated in Agile ceremonies, code reviews, debugging, troubleshooting, and production support.',
        'Developed backend APIs for GenAI solutions involving LLM integrations.'
      ]
    },
    {
      role: 'Software Engineering Intern',
      company: 'Ericsson Global',
      period: 'May 2022 – Aug 2022',
      responsibilities: [
        'Worked on cloud computing and Kubernetes-based workflows within Ericsson’s OSS & Analytics domain.'
      ]
    }
  ];

  // Interactive timeline milestones
  protected readonly timeline = [
    { year: '2022 (May - Aug)', title: 'Ericsson Global', desc: 'Cloud Computing & Kubernetes Intern. Focused on OSS Analytics deployments.' },
    { year: '2022 (Aug) - Present', title: 'Software Engineer at Zensar', desc: 'Backend & full-stack development for BFSI clients. Lead developer for GenAI, microservices and REST APIs.' },
    { year: '2023 (Nov)', title: 'Runner-Up at Zensar Hackathon', desc: 'Awarded 2nd place in Company-Wide Hackathon for proposing an AI-driven workflow engine prototype.' },
    { year: '2024 (May)', title: 'FIS Microservices Migration', desc: 'Spearheaded migration of legacy services to modern REST API architecture with YARP.' },
    { year: '2026 (Q4)', title: 'Team of the Quarter Award', desc: 'Awarded BFSI Team of the Quarter for performance tuning, database optimization, and high SLA releases.' }
  ];

  // Social Proof Testimonials
  protected readonly testimonials = [
    { name: 'BFSI Delivery Manager', company: 'Zensar Technologies', quote: 'Himanshu\'s capacity to design, tune, and deploy critical API features with zero-downtime compliance was key to our BFSI releases.' },
    { name: 'Senior Technical Lead', company: 'FIS Client Account', quote: 'A highly skilled .NET & Angular engineer. His contributions to our microservice scaling and query optimization saved 40% in server overhead.' }
  ];

  // Case Studies Deployed Projects
  protected readonly deployedProjects: DeployedProject[] = [
    {
      name: 'HireNow',
      tagline: 'Applicant Tracking System (ATS)',
      description: 'A modern recruitment management platform supporting candidate management, resume parsing pipelines, application tracking, and interview scheduling.',
      technologies: ['ASP.NET Core', 'Angular', 'SQL Server', 'JWT Authentication', 'Docker'],
      url: 'https://hirenow.himanshuprojectportfolio.xyz',
      icon: 'fa-solid fa-user-tie',
      businessProblem: 'Recruiters and hiring managers spend hours manually reviewing PDFs, tracking interview stages in spreadsheets, and coordinating feedback, leading to candidates slipping through the pipeline.',
      technicalChallenge: 'Parsing multi-format resumes synchronously (PDF/Docx), enforcing strict multi-tenant tenant isolation across shared SQL Server instances, and matching candidates to job descriptions using AI scoring weights.',
      results: 'Reduces manual screening time by 75%. Provides automated duplicate applicant detection and a 92% accurate automated candidate matching model.',
      lessons: 'Implementing robust Global Query Filters in EF Core DB context ensures seamless multi-tenancy protection without manual developer filter overrides.',
      techStackDetailed: ['.NET Core Web API', 'Angular 18 Workspace', 'EF Core / LINQ', 'SQL Server Database', 'Docker Containerization', 'UglyToad PdfPig & DocX Parser']
    },
    {
      name: 'Workflow.io',
      tagline: 'Workflow Management Platform (Workflow.IO)',
      description: 'A distributed workflow orchestration platform supporting visual process automation, approval workflows, task assignment boards, and operational tracking.',
      technologies: ['ASP.NET Core', 'Angular', 'PostgreSQL', 'RabbitMQ', 'Docker', 'YARP Gateway'],
      url: 'https://workflow.himanshuprojectportfolio.xyz',
      icon: 'fa-solid fa-diagram-project',
      businessProblem: 'Teams struggle with slow, disjointed task management systems that lack real-time synchronization, modular project boundaries, and automatic assignment rules.',
      technicalChallenge: 'Orchestrating microservice communication under load, syncing Gantt timeline views with Kanban lists, and executing real-time task notifications via SignalR and RabbitMQ event buses.',
      results: 'Achieved sub-50ms user state synchronizations. Serves thousands of websocket sessions concurrently. Handled 100% of automation triggers asynchronously.',
      lessons: 'Linked scrolling events using reference scroll offset handlers eliminates layout jitter and keeps split-view timelines perfectly aligned.',
      techStackDetailed: ['.NET Web API Services', 'Angular Frontend Host', 'PostgreSQL per microservice', 'RabbitMQ Event Broker', 'YARP Gateway Router', 'SignalR Websockets']
    }
  ];

  // Other Projects listed in the Resume
  protected readonly resumeProjects: ResumeProject[] = [
    {
      name: 'Mini Transformer-based Language Model Implementation',
      technologies: 'Python, Transformers, NLP',
      description: 'Implemented a transformer-based NLP pipeline covering tokenization, embeddings, attention mechanisms, and inference workflows using Python.'
    },
    {
      name: 'Workflow.IO – Project Management System',
      technologies: 'ASP.NET Web API, Angular, SQL Server',
      description: 'Built a full-stack project management platform. Developed RESTful APIs with JWT authentication, RBAC, and workflow automation, optimizing SQL performance.'
    },
    {
      name: 'Laundromart',
      technologies: 'ASP.NET Core, Angular, SQL Server',
      description: 'Developed a full-stack laundry management application featuring workflow automation and customer management modules. Awarded Runner-Up at the Zensar Cladethon.'
    },
    {
      name: 'Exam Portal',
      technologies: 'ASP.NET MVC, Web API, SQL Server',
      description: 'Developed an online examination platform with RBAC implementation. Designed and implemented RESTful APIs supporting business-critical workflows, improving data access performance.'
    }
  ];

  // Achievements
  protected readonly achievements = [
    {
      title: 'Team of the Quarter – BFSI | Q4FY26 at Zensar',
      description: 'Recognized for outstanding delivery, performance tuning, and critical features release for FIS client.',
      icon: 'fa-solid fa-trophy'
    },
    {
      title: 'Runner-Up at Zensar Cladethon (ZENBYTES)',
      description: 'Awarded second place in the company-wide hackathon for proposing and implementing an AI-driven workflow engine prototype.',
      icon: 'fa-solid fa-award'
    },
    {
      title: 'Zensar Ace Alliance Team Award',
      description: 'Awarded for collaborative synergy, project excellence, and maintaining zero-downtime SLA compliance.',
      icon: 'fa-solid fa-people-group'
    },
    {
      title: 'Smart India Hackathon (SIH)',
      description: 'Project idea selected for government research and development funding.',
      icon: 'fa-solid fa-lightbulb'
    }
  ];

  // Certifications
  protected readonly certifications = [
    { name: 'Microsoft Certified: Azure Fundamentals (AZ-900)', authority: 'Microsoft', icon: 'fa-brands fa-microsoft' },
    { name: 'NuvePro MVC Assessment certification', authority: 'NuvePro', icon: 'fa-solid fa-certificate' },
    { name: 'TechAcademy Full Stack Developer Certification', authority: 'TechAcademy', icon: 'fa-solid fa-graduation-cap' }
  ];

  // Education
  protected readonly education = {
    institution: 'Madan Mohan Malaviya University of Technology (MMMUT), Gorakhpur',
    degree: 'Bachelor of Technology (B.Tech)',
    branch: 'Electrical Engineering',
    gpa: 'CGPA: 8.46',
    year: 'Graduated: 2022'
  };

  // Contact Form Submit Handler
  protected onSubmitContact() {
    this.formStatus.set('sending');
    
    const body = {
      name: this.contactModel.name,
      email: this.contactModel.email,
      subject: this.contactModel.subject,
      message: this.contactModel.message
    };
    
    const contactUrl = window.location.hostname.includes('localhost') 
      ? "https://formsubmit.co/ajax/himanshutrip780@gmail.com" 
      : "/api/contact";

    // Send message via Nginx proxy in production to bypass adblockers
    fetch(contactUrl, {
      method: "POST",
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      },
      body: JSON.stringify(body)
    })
    .then(response => {
      if (response.ok) {
        this.formStatus.set('success');
        this.saveMessageToLocalHistory(body);
        this.contactModel = {
          name: '',
          email: '',
          subject: '',
          message: ''
        };
        setTimeout(() => this.formStatus.set('idle'), 5000);
      } else {
        throw new Error('FormSubmit returned error status');
      }
    })
    .catch(error => {
      console.error('Error sending message:', error);
      this.formStatus.set('error');
      setTimeout(() => this.formStatus.set('idle'), 5000);
    });
  }

  // Helper to trigger resume download and track it
  protected downloadResume() {
    let count = localStorage.getItem('portfolio_downloads');
    let countNum = count ? parseInt(count, 10) + 1 : 1;
    localStorage.setItem('portfolio_downloads', countNum.toString());
    this.resumeDownloads.set(countNum);

    const link = document.createElement('a');
    link.href = `/assets/resume.pdf?v=${new Date().getTime()}`;
    link.download = 'Himanshu_Tripathi_Resume.pdf';
    link.click();
  }
}
