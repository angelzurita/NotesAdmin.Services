using AdminServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdminServices.Infrastructure.Persistence.Configurations;

public class LocalDocumentConfiguration : IEntityTypeConfiguration<LocalDocument>
{
    public void Configure(EntityTypeBuilder<LocalDocument> builder)
    {
        builder.ToTable("LocalDocuments");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(d => d.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.FileDataBase64)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(d => d.FileSizeBytes)
            .IsRequired();

        builder.Property(d => d.Description)
            .HasMaxLength(1000);

        builder.Property(d => d.Tags)
            .HasMaxLength(500);

        builder.Property(d => d.CreatedBy).HasMaxLength(100);
        builder.Property(d => d.UpdatedBy).HasMaxLength(100);

        // FK to StorageFolder (optional — documents can live at root)
        builder.HasOne(d => d.Folder)
            .WithMany(f => f.Documents)
            .HasForeignKey(d => d.FolderId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
