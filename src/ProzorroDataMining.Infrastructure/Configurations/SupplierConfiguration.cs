using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProzorroDataMining.Domain.Entities.Tenders;

namespace ProzorroDataMining.Infrastructure.Configurations;

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("suppliers");

        builder.HasKey(supplier => supplier.Id);

        builder.Property(supplier => supplier.Name)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(supplier => supplier.IdentifierScheme)
            .HasMaxLength(32);

        builder.Property(supplier => supplier.IdentifierId)
            .HasMaxLength(64);

        builder.HasMany(supplier => supplier.Tenders)
            .WithOne(tenderSupplier => tenderSupplier.Supplier)
            .HasForeignKey(tenderSupplier => tenderSupplier.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(supplier => supplier.Name);
        builder.HasIndex(supplier => new { supplier.IdentifierScheme, supplier.IdentifierId });
    }
}
