using JwtAuthenticationManager.Model;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthenticationManager.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options): base(options)
        {
        }

        public DbSet<UserAccount> UserAccounts { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<ApiKey> ApiKeys { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserAccount>(entity =>
            {
                entity.ToTable("UserAccounts");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Email)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasIndex(x => x.Email)
                    .IsUnique();

                entity.Property(x => x.PasswordHash)
                    .IsRequired();

                entity.Property(x => x.Role)
                    .HasConversion<string>();

                entity.Property(x => x.IsActive)
                    .IsRequired();
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshTokens");

                entity.HasKey(x => x.RefreshTokenId);

                entity.Property(x => x.TokenHash)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasIndex(x => x.UserAccountId);

                entity.HasIndex(x => x.TokenHash);
            });

            modelBuilder.Entity<ApiKey>(entity =>
            {
                entity.ToTable("ApiKeys");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.KeyHash).IsRequired().HasMaxLength(256);
                entity.Property(x => x.Prefix).IsRequired().HasMaxLength(32);
                entity.Property(x => x.Name).IsRequired().HasMaxLength(100);

                entity.HasIndex(x => x.KeyHash).IsUnique();
                
                entity.HasOne(x => x.UserAccount)
                      .WithMany()
                      .HasForeignKey(x => x.UserAccountId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
