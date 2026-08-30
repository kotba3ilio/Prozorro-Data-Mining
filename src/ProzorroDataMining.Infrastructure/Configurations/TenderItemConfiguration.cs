using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProzorroDataMining.Domain.Entities.Tenders;

namespace ProzorroDataMining.Infrastructure.Configurations;

public sealed class TenderItemConfiguration : IEntityTypeConfiguration<TenderItem>
{
    public void Configure(EntityTypeBuilder<TenderItem> builder)
    {
        builder.ToTable("tender_items");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.ClassificationId)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(item => item.Description)
            .HasColumnType("text");

        builder.HasIndex(item => item.TenderId);
        builder.HasIndex(item => item.ClassificationId);
        builder.HasIndex(item => new { item.ClassificationId, item.TenderId });
    }
}
