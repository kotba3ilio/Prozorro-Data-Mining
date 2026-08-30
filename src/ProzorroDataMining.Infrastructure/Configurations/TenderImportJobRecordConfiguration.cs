using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProzorroDataMining.Infrastructure.Import;

namespace ProzorroDataMining.Infrastructure.Configurations;

public sealed class TenderImportJobRecordConfiguration : IEntityTypeConfiguration<TenderImportJobRecord>
{
    public void Configure(EntityTypeBuilder<TenderImportJobRecord> builder)
    {
        builder.ToTable("tender_import_jobs");

        builder.HasKey(job => job.Id);

        builder.Property(job => job.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(job => job.ClassificationId)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(job => job.Direction)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(job => job.ResultDirection)
            .HasConversion<int>();

        builder.Property(job => job.NextPageUri)
            .HasMaxLength(2048);

        builder.Property(job => job.PrevPageUri)
            .HasMaxLength(2048);

        builder.Property(job => job.ErrorMessage)
            .HasColumnType("text");

        builder.HasIndex(job => job.CreatedAt);
        builder.HasIndex(job => job.Status);
    }
}
