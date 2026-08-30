using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProzorroDataMining.Domain.Entities.Tenders;

namespace ProzorroDataMining.Infrastructure.Configurations;

public sealed class TenderSupplierConfiguration : IEntityTypeConfiguration<TenderSupplier>
{
    public void Configure(EntityTypeBuilder<TenderSupplier> builder)
    {
        builder.ToTable("tender_suppliers");

        builder.HasKey(tenderSupplier => new
        {
            tenderSupplier.TenderId,
            tenderSupplier.SupplierId,
            tenderSupplier.AwardId
        });

        builder.Property(tenderSupplier => tenderSupplier.AwardId)
            .HasMaxLength(64)
            .HasDefaultValue(string.Empty)
            .IsRequired();

        builder.HasOne(tenderSupplier => tenderSupplier.Tender)
            .WithMany(tender => tender.Suppliers)
            .HasForeignKey(tenderSupplier => tenderSupplier.TenderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tenderSupplier => tenderSupplier.Supplier)
            .WithMany(supplier => supplier.Tenders)
            .HasForeignKey(tenderSupplier => tenderSupplier.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(tenderSupplier => tenderSupplier.SupplierId);
        builder.HasIndex(tenderSupplier => tenderSupplier.AwardId);
    }
}
