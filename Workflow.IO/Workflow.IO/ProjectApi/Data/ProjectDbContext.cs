using Microsoft.EntityFrameworkCore;
using ProjectApi.Model.Domain.Entities;
using Workflow.IO.Shared.Contracts;

namespace ProjectApi.Data
{
    public class ProjectDbContext : DbContext
    {
        private readonly ITenantContext? _tenantContext;

        public ProjectDbContext(
            DbContextOptions<ProjectDbContext> options,
            ITenantContext? tenantContext = null) : base(options)
        {
            _tenantContext = tenantContext;
        }

        public DbSet<Project> Projects { get; set; }

        public DbSet<ProjectMember> ProjectMembers { get; set; }

        public DbSet<Client> Clients { get; set; }

        public DbSet<Workspace> Workspaces { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("project");

            // ✅ Workspace Configuration
            
            modelBuilder.Entity<Workspace>()
                .HasKey(x => x.WorkspaceId);

            modelBuilder.Entity<Workspace>()
                .Property(x => x.WorkspaceId)
                .ValueGeneratedNever();
                
            modelBuilder.Entity<Workspace>()
                .HasQueryFilter(x => _tenantContext == null || _tenantContext.CurrentOrganizationId == null ||
                                     x.OrganizationId == _tenantContext.CurrentOrganizationId);
                                     
            modelBuilder.Entity<Workspace>()
                .Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            // ✅ Project Configuration

            modelBuilder.Entity<Project>()
                .HasKey(x => x.ProjectId);

            modelBuilder.Entity<Project>()
                .Property(x => x.ProjectId)
                .ValueGeneratedNever();

            modelBuilder.Entity<Project>()
                .HasQueryFilter(x => _tenantContext == null || _tenantContext.CurrentOrganizationId == null ||
                                     x.OrganizationId == _tenantContext.CurrentOrganizationId);

            modelBuilder.Entity<Project>()
                .Property(x => x.Key)
                .IsRequired()
                .HasMaxLength(10);

            modelBuilder.Entity<Project>()
                .HasIndex(x => x.Key)
                .IsUnique();

            modelBuilder.Entity<Project>()
                .Property(x => x.ProjectType)
                .HasConversion<string>()
                .HasMaxLength(20);

            // ✅ ProjectMember Configuration
            modelBuilder.Entity<ProjectMember>()
                .HasKey(x => new { x.ProjectId, x.UserId });

            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.Project)
                .WithMany()
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectMember>()
                .Property(x => x.Role)
                .HasConversion<string>()
                .HasMaxLength(20);

            // ✅ Client Configuration

            modelBuilder.Entity<Client>()
                .HasKey(x => x.ClientId);

            modelBuilder.Entity<Client>()
                .Property(x => x.ClientId)
                .ValueGeneratedNever();

            modelBuilder.Entity<Client>()
                .HasQueryFilter(x => _tenantContext == null || _tenantContext.CurrentOrganizationId == null ||
                                     x.OrganizationId == _tenantContext.CurrentOrganizationId);

            modelBuilder.Entity<Client>()
                .Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            modelBuilder.Entity<Client>()
                .Property(x => x.Industry)
                .HasMaxLength(100);

            modelBuilder.Entity<Client>()
                .Property(x => x.ContactPerson)
                .HasMaxLength(150);

            modelBuilder.Entity<Client>()
                .Property(x => x.Email)
                .HasMaxLength(150);

            modelBuilder.Entity<Client>()
                .Property(x => x.Keywords)
                .HasMaxLength(500);
        }
    }
}
