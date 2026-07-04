import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { OfferService, OfferDto } from '../../core/services/offer.service';
import { ApplicationService, ApplicationDto } from '../../core/services/application.service';
import { CompanyService } from '../../core/services/company.service';
import { JobService, JobDto } from '../../core/services/job.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-offers',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule
  ],
  templateUrl: './offers.component.html',
  styleUrls: ['./offers.component.scss']
})
export class OffersComponent implements OnInit {
  private offerService = inject(OfferService);
  private applicationService = inject(ApplicationService);
  private companyService = inject(CompanyService);
  private jobService = inject(JobService);
  private authService = inject(AuthService);
  private fb = inject(FormBuilder);

  public offers = signal<OfferDto[]>([]);
  public applications = signal<ApplicationDto[]>([]);
  public jobs = signal<JobDto[]>([]);
  public companyName = signal<string>('Zensar Technologies');
  public loading = signal<boolean>(true);
  public showCreateForm = signal<boolean>(false);
  public selectedOffer = signal<OfferDto | null>(null);

  // Search, Filtering, Sorting and Pagination Signals
  public searchQuery = signal<string>('');
  public statusFilter = signal<string>('');
  public sortBy = signal<string>('startDate');
  public sortOrder = signal<'asc' | 'desc'>('desc');
  public pageIndex = signal<number>(1);
  public pageSize = signal<number>(30);
  
  public offerForm!: FormGroup;
  public signForm!: FormGroup;
  
  public submitting = signal<boolean>(false);
  public signing = signal<boolean>(false);
  public editedLetterContent = signal<string>('');
  public savingLetter = signal<boolean>(false);

  public offerStatuses = [
    { value: 0, label: 'Draft' },
    { value: 1, label: 'Approved' },
    { value: 2, label: 'Sent' },
    { value: 3, label: 'Accepted' },
    { value: 4, label: 'Rejected' },
    { value: 5, label: 'Negotiating' },
    { value: 6, label: 'Withdrawn' }
  ];

  constructor() {
    this.initForms();
  }

  public ngOnInit() {
    this.loadOffers();
    this.loadApplications();
    this.loadJobs();
    this.companyService.getCompany().subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.companyName.set(res.data.name);
        }
      }
    });
  }

  private initForms() {
    this.offerForm = this.fb.group({
      applicationId: ['', [Validators.required]],
      salary: [100000, [Validators.required, Validators.min(1000)]],
      startDate: ['', [Validators.required]],
      offerLetterContent: ['', [Validators.required]]
    });

    this.signForm = this.fb.group({
      signatureName: ['', [Validators.required]]
    });

    // Setup listener to auto-compile template
    this.offerForm.get('applicationId')?.valueChanges.subscribe(() => this.updateDraftTemplate());
    this.offerForm.get('salary')?.valueChanges.subscribe(() => this.updateDraftTemplate());
    this.offerForm.get('startDate')?.valueChanges.subscribe(() => this.updateDraftTemplate());
  }

  public loadOffers() {
    this.loading.set(true);
    this.offerService.getOffers().subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.offers.set(res.data);
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  private loadApplications() {
    // Filter to candidates in Offer stage or screening
    this.applicationService.getApplications().subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.applications.set(res.data);
        }
      }
    });
  }

  private loadJobs() {
    this.jobService.getJobs().subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.jobs.set(res.data);
        }
      }
    });
  }

  public toggleCreateForm() {
    this.showCreateForm.update(val => !val);
    if (this.showCreateForm()) {
      this.selectedOffer.set(null);
      this.offerForm.reset({
        applicationId: '',
        salary: 100000,
        startDate: '',
        offerLetterContent: ''
      });
    }
  }

  public viewOfferDetails(offer: OfferDto) {
    this.selectedOffer.set(offer);
    this.editedLetterContent.set(offer.offerLetterContent || '');
    this.showCreateForm.set(false);
    this.signForm.reset({
      signatureName: ''
    });
  }

  public closeDetails() {
    this.selectedOffer.set(null);
  }

  public saveOfferLetterContent() {
    const offer = this.selectedOffer();
    if (!offer) return;

    this.savingLetter.set(true);
    const updatedText = this.editedLetterContent();

    this.offerService.updateOfferLetter(offer.id, updatedText).subscribe({
      next: (res) => {
        this.savingLetter.set(false);
        if (res.isSuccess) {
          const updatedOffer = { ...offer, offerLetterContent: updatedText };
          this.selectedOffer.set(updatedOffer);
          this.offers.update(list => 
            list.map(o => o.id === offer.id ? { ...o, offerLetterContent: updatedText } : o)
          );
        }
      },
      error: () => this.savingLetter.set(false)
    });
  }

  public onCreateOffer() {
    if (this.offerForm.invalid) return;
    this.submitting.set(true);

    const formVal = this.offerForm.value;
    const payload = {
      ...formVal,
      startDate: new Date(formVal.startDate).toISOString()
    };

    this.offerService.createOffer(payload).subscribe({
      next: (res) => {
        this.submitting.set(false);
        if (res.isSuccess) {
          this.loadOffers();
          this.showCreateForm.set(false);
        }
      },
      error: () => this.submitting.set(false)
    });
  }

  public onSignOffer() {
    if (this.signForm.invalid) return;
    this.signing.set(true);

    const offer = this.selectedOffer();
    if (!offer) return;

    const signature = this.signForm.value.signatureName;
    const signDetails = `Digitally Signed by ${signature} at ${new Date().toLocaleString()}`;

    // Update status to Accepted (3)
    this.offerService.updateOfferStatus(offer.id, 3, signDetails).subscribe({
      next: (res) => {
        this.signing.set(false);
        if (res.isSuccess) {
          this.loadOffers();
          this.selectedOffer.set(null);
        }
      },
      error: () => this.signing.set(false)
    });
  }

  public onTransitionStatus(newStatus: any) {
    const offer = this.selectedOffer();
    if (!offer) return;

    const statusVal = Number(newStatus);
    this.offerService.updateOfferStatus(offer.id, statusVal, '').subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.loadOffers();
          const updatedOffer = { ...offer, status: statusVal };
          this.selectedOffer.set(updatedOffer);
        }
      }
    });
  }

  public getStatusLabel(status: number): string {
    return this.offerStatuses.find(s => s.value === status)?.label || 'Draft';
  }

  public getStatusClass(status: number): string {
    switch (status) {
      case 0: return 'text-slate-600 dark:text-slate-400 bg-secondary/80 border-border';
      case 1: return 'text-amber-700 dark:text-amber-400 bg-amber-500/10 border-amber-500/20';
      case 2: return 'text-sky-700 dark:text-sky-400 bg-sky-500/10 border-sky-500/20';
      case 3: return 'text-emerald-700 dark:text-emerald-400 bg-emerald-500/10 border-emerald-500/20';
      case 4: return 'text-red-700 dark:text-red-400 bg-red-500/10 border-red-500/20';
      case 5: return 'text-indigo-700 dark:text-indigo-400 bg-indigo-500/10 border-indigo-500/20';
      case 6: return 'text-slate-500 dark:text-slate-500 bg-slate-500/10 border-slate-500/20';
      default: return '';
    }
  }

  // Advanced Table Filter & Sorting Methods
  public getFilteredOffers(): OfferDto[] {
    let list = this.offers() || [];
    
    // 1. Search Query filter (by Candidate Name or Job Title)
    const search = this.searchQuery().trim().toLowerCase();
    if (search) {
      list = list.filter(o => {
        const candidateName = o.candidateName || '';
        const jobTitle = o.jobTitle || '';
        return candidateName.toLowerCase().includes(search) || jobTitle.toLowerCase().includes(search);
      });
    }

    // 2. Status Filter
    const status = this.statusFilter();
    if (status !== '') {
      const statusNum = Number(status);
      list = list.filter(o => o.status === statusNum);
    }

    // 3. Sorting
    const field = this.sortBy();
    const order = this.sortOrder() === 'asc' ? 1 : -1;
    
    list = [...list].sort((a: any, b: any) => {
      let valA = a[field];
      let valB = b[field];

      if (field === 'salary') {
        valA = a.salary || 0;
        valB = b.salary || 0;
      } else if (field === 'startDate') {
        valA = a.startDate ? new Date(a.startDate).getTime() : 0;
        valB = b.startDate ? new Date(b.startDate).getTime() : 0;
      }

      if (valA < valB) return -1 * order;
      if (valA > valB) return 1 * order;
      return 0;
    });

    return list;
  }

  public getPaginatedOffers(): OfferDto[] {
    const list = this.getFilteredOffers();
    const start = (this.pageIndex() - 1) * this.pageSize();
    const end = start + this.pageSize();
    return list.slice(start, end);
  }

  public onSearchChanged(val: string) {
    this.searchQuery.set(val);
    this.pageIndex.set(1);
  }

  public onStatusFilterChange(val: string) {
    this.statusFilter.set(val);
    this.pageIndex.set(1);
  }

  public toggleSort(field: string) {
    if (this.sortBy() === field) {
      this.sortOrder.update(order => order === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortBy.set(field);
      this.sortOrder.set('asc');
    }
    this.pageIndex.set(1);
  }

  public prevPage() {
    if (this.pageIndex() > 1) {
      this.pageIndex.update(idx => idx - 1);
    }
  }

  public nextPage() {
    const totalCount = this.getFilteredOffers().length;
    if (this.pageIndex() * this.pageSize() < totalCount) {
      this.pageIndex.update(idx => idx + 1);
    }
  }

  public updateDraftTemplate() {
    const formVal = this.offerForm.value;
    const appId = formVal.applicationId;
    const salary = formVal.salary;
    const sDateRaw = formVal.startDate;

    if (!appId) return;

    const app = this.applications().find(a => a.id === appId);
    if (!app) return;

    const candidateName = app.candidateName || 'Candidate';
    const jobTitle = app.jobTitle || 'Software Professional';
    
    // Lookup job details to extract department, location, recruiter, and currency
    const job = this.jobs().find(j => j.id === app.jobId);
    
    const departmentName = job?.departmentName || 'Engineering';
    const location = job?.location || 'Remote';
    const currency = job?.currency || 'USD';
    const recruiterName = job?.recruiterName || (this.authService.currentUser()?.firstName + ' ' + (this.authService.currentUser()?.lastName || '')) || 'Lead Recruiter';
    const recruiterEmail = this.authService.currentUser()?.email || 'recruitment@zensar.com';

    const dateFormatted = sDateRaw ? new Date(sDateRaw).toLocaleDateString(undefined, { year: 'numeric', month: 'long', day: 'numeric' }) : '[Start Date]';
    const salaryFormatted = salary ? Number(salary).toLocaleString('en-US', { style: 'currency', currency: currency, maximumFractionDigits: 0 }) : `[Salary] ${currency}`;
    const compName = this.companyName();

    const letterText = `Dear ${candidateName},

On behalf of ${compName}, we are pleased to offer you the position of ${jobTitle} within our ${departmentName} department, located at our ${location} office. We are extremely excited about the prospect of you joining our team!

Please find the detailed terms of your employment offer below:

- **Position**: ${jobTitle}
- **Department**: ${departmentName}
- **Location**: ${location}
- **Annual Base Salary**: ${salaryFormatted}
- **Start Date**: ${dateFormatted}
- **Employment Status**: Full-Time

This offer is contingent upon the successful completion of standard background checks and reference validations.

To accept this offer, please review the document and type your full name in the signature field in the e-sign portal by ${dateFormatted}.

Best regards,

${recruiterName}
Lead Recruiter / Hiring Coordinator
${compName} Talent Acquisition Team
Email: ${recruiterEmail}`;

    this.offerForm.patchValue({
      offerLetterContent: letterText
    }, { emitEvent: false });
  }
}
