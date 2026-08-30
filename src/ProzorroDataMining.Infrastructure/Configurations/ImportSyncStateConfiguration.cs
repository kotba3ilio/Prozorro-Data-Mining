using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProzorroDataMining.Infrastructure.Import;

namespace ProzorroDataMining.Infrastructure.Configurations;

public sealed class ImportSyncStateConfiguration : IEntityTypeConfiguration<ImportSyncState>
{
    public void Configure(EntityTypeBuilder<ImportSyncState> builder)
    {
        builder.ToTable("import_sync_states");

        builder.HasKey(syncState => syncState.Id);

        builder.Property(syncState => syncState.FeedName)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(syncState => syncState.FeedName)
            .IsUnique();

        builder.Property(syncState => syncState.BackwardNextPageUri)
            .HasMaxLength(2048);

        builder.Property(syncState => syncState.ForwardStartPageUri)
            .HasMaxLength(2048);

        builder.Property(syncState => syncState.ForwardNextPageUri)
            .HasMaxLength(2048);

        builder.Property(syncState => syncState.LastDirection)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(syncState => syncState.LastPublicModified)
            .HasPrecision(20, 6);

        builder.Property(syncState => syncState.CreatedAt)
            .IsRequired();

        builder.Property(syncState => syncState.UpdatedAt)
            .IsRequired();
    }
}
