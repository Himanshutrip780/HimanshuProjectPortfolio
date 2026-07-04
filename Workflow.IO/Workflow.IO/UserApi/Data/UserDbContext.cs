using Microsoft.EntityFrameworkCore;
using UserApi.Model.Domian;
using UserApi.Model.Domian.Entities;
using Workflow.IO.Shared.Contracts;

namespace UserApi.Data
{
    public class UserDbContext : DbContext
    {
        private readonly ITenantContext _tenantContext;

        public UserDbContext(DbContextOptions<UserDbContext> options, ITenantContext tenantContext) : base(options)
        {
            _tenantContext = tenantContext;
        }

        public Guid? CurrentTenantId => _tenantContext.CurrentOrganizationId;

        public DbSet<User> Users { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<EmailVerificationRequest> EmailVerificationRequests { get; set; }
        public DbSet<OTPVerificationAttempt> OTPVerificationAttempts { get; set; }
        public DbSet<EmailVerificationAuditLog> EmailVerificationAuditLogs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(UserDbContext).Assembly);

            modelBuilder.Entity<User>()
                .HasQueryFilter(x => x.OrganizationId == CurrentTenantId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
