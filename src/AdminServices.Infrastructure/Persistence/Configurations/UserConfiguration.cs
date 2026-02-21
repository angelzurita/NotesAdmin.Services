using AdminServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdminServices.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(512);
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Role).IsRequired().HasMaxLength(50);
        builder.Property(u => u.CreatedBy).HasMaxLength(100);
        builder.Property(u => u.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(u => u.Email).IsUnique();
    }
}
