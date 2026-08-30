using Microsoft.EntityFrameworkCore;
using ProzorroDataMining.Application.Abstractions;
using ProzorroDataMining.Domain.Entities.Tenders;
using ProzorroDataMining.Infrastructure.Import;

namespace ProzorroDataMining.Infrastructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Tender> Tenders => Set<Tender>();

    public DbSet<TenderItem> TenderItems => Set<TenderItem>();

    public DbSet<TenderContract> TenderContracts => Set<TenderContract>();

    public DbSet<Supplier> Suppliers => Set<Supplier>();

    public DbSet<TenderSupplier> TenderSuppliers => Set<TenderSupplier>();

    public DbSet<TenderImportPayload> TenderImportPayloads => Set<TenderImportPayload>();

    public DbSet<ImportSyncState> ImportSyncStates => Set<ImportSyncState>();

    public DbSet<TenderImportJobRecord> TenderImportJobs => Set<TenderImportJobRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}