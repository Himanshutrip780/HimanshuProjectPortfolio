using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserApi.Model.Domian.Entities;

namespace UserApi.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users", "identity");
            builder.HasKey(x => x.UserId);
            builder.HasQueryFilter(x => !x.IsDeleted);
            //builder.HasIndex(x => x.ema).IsUnique();
            //builder.Property(x => x.Email).IsRequired().HasMaxLength(200);
            builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.AvatarUrl).HasColumnType("text");
            //builder.Property(x => x.Role).IsRequired().HasConversion<string>().HasMaxLength(50);
            builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
            builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("now()");
            builder.Property(x => x.UpdatedAt).IsRequired().HasDefaultValueSql("now()");

            builder.HasOne(x => x.Organization)
                   .WithMany(o => o.Users)
                   .HasForeignKey(x => x.OrganizationId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
