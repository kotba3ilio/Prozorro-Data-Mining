using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProzorroDataMining.Domain.Entities.Tenders;

namespace ProzorroDataMining.Infrastructure.Configurations;

public sealed class TenderContractConfiguration : IEntityTypeConfiguration<TenderContract>
{
    public void Configure(EntityTypeBuilder<TenderContract> builder)
    {
        builder.ToTable("tender_contracts");

        builder.HasKey(contract => contract.Id);

        builder.Property(contract => contract.ProzorroContractId)
            .HasMaxLength(64);

        builder.Property(contract => contract.AwardId)
            .HasMaxLength(64)
            .HasDefaultValue(string.Empty)
            .IsRequired();

        builder.Property(contract => contract.Amount)
            .HasPrecision(18, 2);

        builder.Property(contract => contract.Currency)
            .HasMaxLength(3);

        builder.HasIndex(contract => contract.TenderId);
        builder.HasIndex(contract => new { contract.TenderId, contract.AwardId });
        builder.HasIndex(contract => contract.ProzorroContractId);
        builder.HasIndex(contract => contract.DateSigned);
    }
}
