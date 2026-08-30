using ProzorroDataMining.Domain.Entities.Tenders;

namespace ProzorroDataMining.Domain.Tests;

public sealed class TenderTests
{
    [Fact]
    public void Create_NormalizesDatesAndStoresDetails()
    {
        var dateCreated = new DateTimeOffset(2025, 12, 10, 12, 0, 0, TimeSpan.FromHours(2));
        var importedAt = new DateTimeOffset(2025, 12, 11, 9, 30, 0, TimeSpan.FromHours(2));

        var tender = Tender.Create(
            "UA-2025-12-10-000001-a",
            TenderStatus.Complete,
            dateCreated,
            "Замовник",
            1_000m,
            "UAH",
            importedAt);

        Assert.Equal("UA-2025-12-10-000001-a", tender.ProzorroId);
        Assert.Equal(TenderStatus.Complete, tender.Status);
        Assert.Equal(dateCreated.ToUniversalTime(), tender.DateCreated);
        Assert.Equal(importedAt.ToUniversalTime(), tender.ImportedAt);
        Assert.Equal(importedAt.ToUniversalTime(), tender.UpdatedAt);
    }

    [Fact]
    public void ReplaceCollections_ReplacesExistingValues()
    {
        var tender = CreateTender();
        tender.ReplaceItems([
            TenderItem.Create("09310000-5", "Електроенергія"),
            TenderItem.Create("09100000-0", "Паливо")
        ]);
        tender.ReplaceContracts([
            TenderContract.Create("contract-1", "award-1", 100m, "UAH", DateTimeOffset.UtcNow)
        ]);

        tender.ReplaceItems([TenderItem.Create("09310000-5", "Оновлена електроенергія")]);
        tender.ReplaceContracts([TenderContract.Create("contract-2", "award-2", 250m, "UAH", null)]);

        var item = Assert.Single(tender.Items);
        var contract = Assert.Single(tender.Contracts);
        Assert.Equal("Оновлена електроенергія", item.Description);
        Assert.Equal("contract-2", contract.ProzorroContractId);
        Assert.Equal("award-2", contract.AwardId);
    }

    [Fact]
    public void TenderContractCreate_UsesEmptyAwardIdWhenMissing()
    {
        var contract = TenderContract.Create("contract-1", null, 500m, "UAH", null);

        Assert.Equal(string.Empty, contract.AwardId);
    }

    private static Tender CreateTender()
    {
        return Tender.Create(
            "UA-2025-12-10-000001-a",
            TenderStatus.Complete,
            DateTimeOffset.UtcNow,
            "Замовник",
            1_000m,
            "UAH",
            DateTimeOffset.UtcNow);
    }
}