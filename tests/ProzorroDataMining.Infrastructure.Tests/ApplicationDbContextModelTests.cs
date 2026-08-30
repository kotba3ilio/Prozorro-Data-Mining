using Microsoft.EntityFrameworkCore;
using ProzorroDataMining.Domain.Entities.Tenders;
using ProzorroDataMining.Infrastructure;
using ProzorroDataMining.Infrastructure.Import;

namespace ProzorroDataMining.Infrastructure.Tests;

public sealed class ApplicationDbContextModelTests
{
    [Fact]
    public void TenderItemDescription_IsMappedAsText()
    {
        using var dbContext = CreateDbContext();
        var entityType = dbContext.Model.FindEntityType(typeof(TenderItem));
        var description = entityType?.FindProperty(nameof(TenderItem.Description));

        Assert.NotNull(description);
        Assert.Equal("text", description.GetColumnType());
        Assert.Null(description.GetMaxLength());
    }

    [Fact]
    public void TenderContractAwardId_IsRequiredAndIndexedWithTenderId()
    {
        using var dbContext = CreateDbContext();
        var entityType = dbContext.Model.FindEntityType(typeof(TenderContract));
        var awardId = entityType?.FindProperty(nameof(TenderContract.AwardId));
        var tenderId = entityType?.FindProperty(nameof(TenderContract.TenderId));

        Assert.NotNull(awardId);
        Assert.NotNull(tenderId);
        Assert.False(awardId.IsNullable);
        Assert.Equal(64, awardId.GetMaxLength());
        Assert.Contains(entityType!.GetIndexes(), index =>
            index.Properties.Select(property => property.Name).SequenceEqual([
                nameof(TenderContract.TenderId),
                nameof(TenderContract.AwardId)
            ]));
    }

    [Fact]
    public void TenderImportJobRecord_IsMappedToPersistentJobTable()
    {
        using var dbContext = CreateDbContext();
        var entityType = dbContext.Model.FindEntityType(typeof(TenderImportJobRecord));
        var status = entityType?.FindProperty(nameof(TenderImportJobRecord.Status));
        var direction = entityType?.FindProperty(nameof(TenderImportJobRecord.Direction));
        var errorMessage = entityType?.FindProperty(nameof(TenderImportJobRecord.ErrorMessage));

        Assert.NotNull(entityType);
        Assert.Equal("tender_import_jobs", entityType.GetTableName());
        Assert.NotNull(status);
        Assert.NotNull(direction);
        Assert.NotNull(errorMessage);
        Assert.Equal("text", errorMessage.GetColumnType());
        Assert.Contains(entityType.GetIndexes(), index =>
            index.Properties.Select(property => property.Name).SequenceEqual([nameof(TenderImportJobRecord.CreatedAt)]));
        Assert.Contains(entityType.GetIndexes(), index =>
            index.Properties.Select(property => property.Name).SequenceEqual([nameof(TenderImportJobRecord.Status)]));
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        return new ApplicationDbContext(options);
    }
}