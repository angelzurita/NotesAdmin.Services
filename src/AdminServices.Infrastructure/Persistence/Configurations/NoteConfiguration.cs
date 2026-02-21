using AdminServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdminServices.Infrastructure.Persistence.Configurations;

public class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("Notes");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title).IsRequired().HasMaxLength(500);
        builder.Property(n => n.Content).IsRequired();
        builder.Property(n => n.Image1).HasColumnType("text");
        builder.Property(n => n.Image2).HasColumnType("text");
        builder.Property(n => n.Metadata).HasColumnType("text");
        builder.Property(n => n.CreatedBy).HasMaxLength(100);
        builder.Property(n => n.UpdatedBy).HasMaxLength(100);

        builder.HasOne(n => n.Category)
            .WithMany(c => c.Notes)
            .HasForeignKey(n => n.CategoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
