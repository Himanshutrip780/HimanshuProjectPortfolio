using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ATS.Application.Common.Interfaces;
using ATS.Domain.Common;
using ATS.Domain.Entities;

namespace ATS.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ITenantProvider _tenantProvider;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ICurrentUserService currentUserService,
            ITenantProvider tenantProvider)
            : base(options)
        {
            _currentUserService = currentUserService;
            _tenantProvider = tenantProvider;
        }

        public Guid CurrentCompanyId => _tenantProvider.GetCompanyId() ?? Guid.Empty;

        public DbSet<Company> Companies => Set<Company>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Job> Jobs => Set<Job>();
        public DbSet<JobSkill> JobSkills => Set<JobSkill>();
        public DbSet<Candidate> Candidates => Set<Candidate>();
        public DbSet<CandidateSkill> CandidateSkills => Set<CandidateSkill>();
        public DbSet<ATS.Domain.Entities.Application> Applications => Set<ATS.Domain.Entities.Application>();
        public DbSet<ApplicationStage> ApplicationStages => Set<ApplicationStage>();
        public DbSet<Interview> Interviews => Set<Interview>();
        public DbSet<InterviewFeedback> InterviewFeedbacks => Set<InterviewFeedback>();
        public DbSet<Offer> Offers => Set<Offer>();
        public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<ResumeParsingResult> ResumeParsingResults => Set<ResumeParsingResult>();
        public DbSet<AIScore> AIScores => Set<AIScore>();
        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<CandidateNote> CandidateNotes => Set<CandidateNote>();
        public DbSet<InterviewQuestionTemplate> InterviewQuestionTemplates => Set<InterviewQuestionTemplate>();
        public DbSet<TenantVerificationRequest> TenantVerificationRequests => Set<TenantVerificationRequest>();
        public DbSet<TenantSmtpSettings> TenantSmtpSettings => Set<TenantSmtpSettings>();
        public DbSet<CandidateSearchIndex> CandidateSearchIndices => Set<CandidateSearchIndex>();
        public DbSet<EmailOutbox> EmailOutboxes => Set<EmailOutbox>();
        public DbSet<EmailVerificationRequest> EmailVerificationRequests => Set<EmailVerificationRequest>();
        public DbSet<OTPVerificationAttempt> OTPVerificationAttempts => Set<OTPVerificationAttempt>();
        public DbSet<EmailVerificationAuditLog> EmailVerificationAuditLogs => Set<EmailVerificationAuditLog>();

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService?.UserId ?? "System";

            // Process Auditing and Soft Deletes
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is BaseEntity baseEntity)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            baseEntity.CreatedBy = userId;
                            baseEntity.CreatedDate = DateTime.UtcNow;
                            baseEntity.UpdatedBy = userId;
                            baseEntity.IsDeleted = false;
                            break;
                        case EntityState.Modified:
                            baseEntity.UpdatedBy = userId;
                            baseEntity.UpdatedDate = DateTime.UtcNow;
                            break;
                        case EntityState.Deleted:
                            // Soft delete logic
                            entry.State = EntityState.Modified;
                            baseEntity.IsDeleted = true;
                            baseEntity.UpdatedBy = userId;
                            baseEntity.UpdatedDate = DateTime.UtcNow;
                            break;
                    }
                }
                else if (entry.Entity is ApplicationUser appUser)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            appUser.CreatedBy = userId;
                            appUser.CreatedDate = DateTime.UtcNow;
                            appUser.UpdatedBy = userId;
                            appUser.IsDeleted = false;
                            break;
                        case EntityState.Modified:
                            appUser.UpdatedBy = userId;
                            appUser.UpdatedDate = DateTime.UtcNow;
                            break;
                        case EntityState.Deleted:
                            entry.State = EntityState.Modified;
                            appUser.IsDeleted = true;
                            appUser.UpdatedBy = userId;
                            appUser.UpdatedDate = DateTime.UtcNow;
                            break;
                    }
                }
            }

            var auditEntries = OnBeforeSaveChanges(userId);

            var result = await base.SaveChangesAsync(cancellationToken);

            await OnAfterSaveChangesAsync(auditEntries);

            return result;
        }

        private List<AuditEntry> OnBeforeSaveChanges(string userId)
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditEntry(entry)
                {
                    TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                    UserId = userId,
                    Action = entry.State.ToString()
                };
                auditEntries.Add(auditEntry);

                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        if (property.IsTemporary)
                        {
                            auditEntry.TemporaryProperties.Add(property);
                            continue;
                        }
                        auditEntry.OldValues[propertyName] = property.CurrentValue;
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }
            }

            foreach (var auditEntry in auditEntries.Where(_ => !_.HasTemporaryProperties))
            {
                AuditLogs.Add(auditEntry.ToAuditLog());
            }

            return auditEntries.Where(_ => _.HasTemporaryProperties).ToList();
        }

        private async Task OnAfterSaveChangesAsync(List<AuditEntry> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count == 0)
                return;

            foreach (var auditEntry in auditEntries)
            {
                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.OldValues[prop.Metadata.Name] = prop.CurrentValue;
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    else
                    {
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }
                AuditLogs.Add(auditEntry.ToAuditLog());
            }

            await base.SaveChangesAsync();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure global query filter for Soft Delete on entities inheriting from BaseEntity (excluding IMultiTenant)
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType) && !typeof(IMultiTenant).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                    var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                    var falseConstant = System.Linq.Expressions.Expression.Constant(false);
                    var body = System.Linq.Expressions.Expression.Equal(property, falseConstant);
                    var lambda = System.Linq.Expressions.Expression.Lambda(body, parameter);

                    builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }

            // Configure explicit tenant query filter for IMultiTenant entities
            builder.Entity<Job>().HasQueryFilter(e => !e.IsDeleted && (CurrentCompanyId == Guid.Empty || e.CompanyId == CurrentCompanyId));
            builder.Entity<Candidate>().HasQueryFilter(e => !e.IsDeleted && (CurrentCompanyId == Guid.Empty || e.CompanyId == CurrentCompanyId));
            builder.Entity<Department>().HasQueryFilter(e => !e.IsDeleted && (CurrentCompanyId == Guid.Empty || e.CompanyId == CurrentCompanyId));
            builder.Entity<EmailTemplate>().HasQueryFilter(e => !e.IsDeleted && (CurrentCompanyId == Guid.Empty || e.CompanyId == CurrentCompanyId));
            builder.Entity<TenantSmtpSettings>().HasQueryFilter(e => !e.IsDeleted && (CurrentCompanyId == Guid.Empty || e.CompanyId == CurrentCompanyId));
            builder.Entity<ApplicationUser>().HasQueryFilter(u => !u.IsDeleted && (CurrentCompanyId == Guid.Empty || u.CompanyId == CurrentCompanyId));
            builder.Entity<CandidateSearchIndex>().HasQueryFilter(e => !e.IsDeleted && (CurrentCompanyId == Guid.Empty || e.CompanyId == CurrentCompanyId));

            // Configure TenantSmtpSettings relationship
            builder.Entity<TenantSmtpSettings>(entity =>
            {
                entity.HasIndex(s => s.CompanyId);

                entity.HasOne(s => s.Company)
                    .WithMany()
                    .HasForeignKey(s => s.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Department>(entity =>
            {
                entity.HasIndex(d => d.CompanyId);
            });

            builder.Entity<EmailTemplate>(entity =>
            {
                entity.HasIndex(e => e.CompanyId);
            });

            // Configure CandidateSearchIndex relationship
            builder.Entity<CandidateSearchIndex>(entity =>
            {
                entity.HasOne(s => s.Candidate)
                    .WithOne()
                    .HasForeignKey<CandidateSearchIndex>(s => s.CandidateId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Company)
                    .WithMany()
                    .HasForeignKey(s => s.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure explicit relationships to avoid cascade cycles in SQL Server
            builder.Entity<Job>(entity =>
            {
                entity.HasIndex(j => j.CompanyId);

                entity.HasOne(j => j.HiringManager)
                    .WithMany()
                    .HasForeignKey(j => j.HiringManagerId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(j => j.Recruiter)
                    .WithMany()
                    .HasForeignKey(j => j.RecruiterId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(j => j.Company)
                    .WithMany()
                    .HasForeignKey(j => j.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasOne(u => u.Company)
                    .WithMany(c => c.Users)
                    .HasForeignKey(u => u.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Candidate>(entity =>
            {
                entity.HasOne(c => c.Company)
                    .WithMany(comp => comp.Candidates)
                    .HasForeignKey(c => c.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(c => new { c.Email, c.CompanyId }).IsUnique();
                entity.HasIndex(c => c.CompanyId);

                var piiConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<string, string>(
                    v => ATS.Shared.Security.EncryptionHelper.Encrypt(v),
                    v => ATS.Shared.Security.EncryptionHelper.Decrypt(v));

                entity.Property(c => c.Email)
                    .HasConversion(piiConverter);

                entity.Property(c => c.Phone)
                    .HasConversion(piiConverter);
            });

            builder.Entity<Interview>(entity =>
            {
                entity.HasIndex(i => i.ApplicationId);

                entity.HasOne(i => i.Interviewer)
                    .WithMany()
                    .HasForeignKey(i => i.InterviewerId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(i => i.Application)
                    .WithMany(a => a.Interviews)
                    .HasForeignKey(i => i.ApplicationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<InterviewFeedback>(entity =>
            {
                entity.HasOne(f => f.Interview)
                    .WithMany(i => i.Feedbacks)
                    .HasForeignKey(f => f.InterviewId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(f => f.Interviewer)
                    .WithMany()
                    .HasForeignKey(f => f.InterviewerId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<Offer>(entity =>
            {
                entity.HasOne(o => o.Application)
                    .WithMany(a => a.Offers)
                    .HasForeignKey(o => o.ApplicationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(o => o.Salary)
                    .HasPrecision(18, 2);
            });

            builder.Entity<AIScore>(entity =>
            {
                entity.HasOne(s => s.Application)
                    .WithMany(a => a.AIScores)
                    .HasForeignKey(s => s.ApplicationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Job)
                    .WithMany()
                    .HasForeignKey(s => s.JobId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(s => s.Candidate)
                    .WithMany()
                    .HasForeignKey(s => s.CandidateId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<ResumeParsingResult>(entity =>
            {
                entity.HasOne(r => r.Candidate)
                    .WithOne(c => c.ParsingResult)
                    .HasForeignKey<ResumeParsingResult>(r => r.CandidateId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ActivityLog>(entity =>
            {
                entity.HasOne(l => l.Application)
                    .WithMany(a => a.ActivityLogs)
                    .HasForeignKey(l => l.ApplicationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(l => l.Candidate)
                    .WithMany()
                    .HasForeignKey(l => l.CandidateId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<CandidateNote>(entity =>
            {
                entity.HasOne(n => n.Candidate)
                    .WithMany(c => c.Notes)
                    .HasForeignKey(n => n.CandidateId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(n => n.Application)
                    .WithMany()
                    .HasForeignKey(n => n.ApplicationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ATS.Domain.Entities.Application>(entity =>
            {
                entity.HasIndex(a => a.JobId);
                entity.HasIndex(a => a.CandidateId);
            });
        }
    }

    public class AuditEntry
    {
        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
        }

        public EntityEntry Entry { get; }
        public string UserId { get; set; }
        public string TableName { get; set; }
        public string Action { get; set; }
        public Dictionary<string, object> OldValues { get; } = new();
        public Dictionary<string, object> NewValues { get; } = new();
        public List<PropertyEntry> TemporaryProperties { get; } = new();

        public bool HasTemporaryProperties => TemporaryProperties.Any();

        public AuditLog ToAuditLog()
        {
            var audit = new AuditLog();
            audit.Id = Guid.NewGuid();
            audit.UserId = Guid.TryParse(UserId, out var guid) ? guid : null;
            audit.TableName = TableName;
            audit.Action = Action;
            audit.Timestamp = DateTime.UtcNow;
            audit.OldValues = OldValues.Count == 0 ? "{}" : System.Text.Json.JsonSerializer.Serialize(OldValues);
            audit.NewValues = NewValues.Count == 0 ? "{}" : System.Text.Json.JsonSerializer.Serialize(NewValues);
            return audit;
        }
    }
}
