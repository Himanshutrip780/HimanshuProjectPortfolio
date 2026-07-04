using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ATS.Domain.Entities;

namespace ATS.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Company> Companies { get; }
        DbSet<ApplicationUser> Users { get; }
        DbSet<Department> Departments { get; }
        DbSet<Job> Jobs { get; }
        DbSet<JobSkill> JobSkills { get; }
        DbSet<Candidate> Candidates { get; }
        DbSet<CandidateSkill> CandidateSkills { get; }
        DbSet<ATS.Domain.Entities.Application> Applications { get; }
        DbSet<ApplicationStage> ApplicationStages { get; }
        DbSet<Interview> Interviews { get; }
        DbSet<InterviewFeedback> InterviewFeedbacks { get; }
        DbSet<Offer> Offers { get; }
        DbSet<EmailTemplate> EmailTemplates { get; }
        DbSet<Notification> Notifications { get; }
        DbSet<ResumeParsingResult> ResumeParsingResults { get; }
        DbSet<AIScore> AIScores { get; }
        DbSet<ActivityLog> ActivityLogs { get; }
        DbSet<AuditLog> AuditLogs { get; }
        DbSet<CandidateNote> CandidateNotes { get; }
        DbSet<InterviewQuestionTemplate> InterviewQuestionTemplates { get; }
        DbSet<TenantVerificationRequest> TenantVerificationRequests { get; }
        DbSet<TenantSmtpSettings> TenantSmtpSettings { get; }
        DbSet<CandidateSearchIndex> CandidateSearchIndices { get; }
        DbSet<EmailOutbox> EmailOutboxes { get; }
        DbSet<EmailVerificationRequest> EmailVerificationRequests { get; }
        DbSet<OTPVerificationAttempt> OTPVerificationAttempts { get; }
        DbSet<EmailVerificationAuditLog> EmailVerificationAuditLogs { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
