using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProzorroDataMining.Domain.Entities.Tenders;

namespace ProzorroDataMining.Infrastructure.Configurations;

public sealed class TenderConfiguration : IEntityTypeConfiguration<Tender>
{
    public void Configure(EntityTypeBuilder<Tender> builder)
    {
        builder.ToTable("tenders");

        builder.HasKey(tender => tender.Id);

        builder.Property(tender => tender.ProzorroId)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(tender => tender.ProzorroId)
            .IsUnique();

        builder.Property(tender => tender.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(tender => tender.ProcuringEntityName)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(tender => tender.ExpectedAmount)
            .HasPrecision(18, 2);

        builder.Property(tender => tender.Currency)
            .HasMaxLength(3);

        builder.HasMany(tender => tender.Items)
            .WithOne(item => item.Tender)
            .HasForeignKey(item => item.TenderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(tender => tender.Contracts)
            .WithOne(contract => contract.Tender)
            .HasForeignKey(contract => contract.TenderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(tender => tender.Suppliers)
            .WithOne(tenderSupplier => tenderSupplier.Tender)
            .HasForeignKey(tenderSupplier => tenderSupplier.TenderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(tender => tender.Status);
        builder.HasIndex(tender => tender.DateCreated);
        builder.HasIndex(tender => tender.ProcuringEntityName);
        builder.HasIndex(tender => tender.ImportedAt);
    }
}
