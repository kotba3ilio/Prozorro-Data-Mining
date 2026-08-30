using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProzorroDataMining.Domain.Entities.Tenders;
using ProzorroDataMining.Infrastructure.Import;

namespace ProzorroDataMining.Infrastructure.Configurations;

public sealed class TenderImportPayloadConfiguration : IEntityTypeConfiguration<TenderImportPayload>
{
    public void Configure(EntityTypeBuilder<TenderImportPayload> builder)
    {
        builder.ToTable("tender_import_payloads");

        builder.HasKey(payload => payload.Id);

        builder.Property(payload => payload.ProzorroId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(payload => payload.Payload)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(payload => payload.PayloadHash)
            .HasMaxLength(128);

        builder.HasOne<Tender>(payload => payload.Tender)
            .WithMany()
            .HasForeignKey(payload => payload.TenderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(payload => payload.ProzorroId);
        builder.HasIndex(payload => payload.TenderId);
        builder.HasIndex(payload => payload.PublicModified);
        builder.HasIndex(payload => payload.ImportedAt);
    }
}
