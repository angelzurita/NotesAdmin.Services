using AdminServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdminServices.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for Category
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.CreatedBy)
            .HasMaxLength(100);

        builder.Property(c => c.UpdatedBy)
            .HasMaxLength(100);

        builder.HasMany(c => c.Notes)
            .WithOne(n => n.Category)
            .HasForeignKey(n => n.CategoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
