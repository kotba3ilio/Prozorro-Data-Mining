using ProzorroDataMining.Application.Abstractions;
using ProzorroDataMining.Application.Analytics;
using ProzorroDataMining.Application.Models.Analytics;
using ProzorroDataMining.Application.Tenders;
using ProzorroDataMining.Domain.Entities.Tenders;

namespace ProzorroDataMining.Application.Tests;

public sealed class TenderServiceTests
{
    [Fact]
    public async Task TendersAsync_ClampsPageSizeAndReturnsNextCursor()
    {
        var firstTender = CreateTender("UA-2025-12-01-000001-a", "Замовник Б", 1_000m);
        var firstItem = CreateListItem(firstTender, 650m, ["Постачальник А", "Постачальник Б"]);
        var repository = new FakeTenderRepository([firstTender], [firstItem]);
        var service = new TenderService(repository);

        var response = await service.TendersAsync(CreateFilter(), cursor: null, pageSize: 1_000);

        Assert.Equal(100, response.PageSize);
        Assert.False(response.HasNextPage);
        Assert.Null(response.NextCursor);
        var item = Assert.Single(response.Items);
        Assert.Equal("UA-2025-12-01-000001-a", item.ProzorroId);
        Assert.Equal(650m, item.ContractAmount);
        Assert.Equal(["Постачальник А", "Постачальник Б"], item.Suppliers);
        Assert.Null(repository.LastCursor);
        Assert.Equal(101, repository.LastTake);
    }

    [Fact]
    public async Task TendersAsync_ReturnsNextCursorWhenMoreRowsExist()
    {
        var firstTender = CreateTender("UA-2025-12-01-000001-a", "Замовник Б", 1_000m);
        var secondTender = CreateTender("UA-2025-12-01-000002-a", "Замовник В", 2_000m);
        var repository = new FakeTenderRepository(
            [firstTender, secondTender],
            [CreateListItem(firstTender, 650m, ["Постачальник А"]), CreateListItem(secondTender, 900m, ["Постачальник В"])]);
        var service = new TenderService(repository);

        var response = await service.TendersAsync(CreateFilter(), cursor: null, pageSize: 1);

        Assert.True(response.HasNextPage);
        Assert.NotNull(response.NextCursor);
        var item = Assert.Single(response.Items);
        Assert.Equal(firstTender.Id, item.Id);
    }

    [Fact]
    public async Task TendersAsync_UsesDecodedCursorForKeysetSearch()
    {
        var cursor = new TenderPageCursor(
            new DateTimeOffset(2025, 12, 10, 0, 0, 0, TimeSpan.Zero),
            Guid.NewGuid());
        var repository = new FakeTenderRepository([], []);
        var service = new TenderService(repository);

        var response = await service.TendersAsync(CreateFilter(), cursor.Encode(), pageSize: 20);

        Assert.Empty(response.Items);
        Assert.Equal(cursor.DateCreated, repository.LastCursor?.DateCreated);
        Assert.Equal(cursor.Id, repository.LastCursor?.Id);
        Assert.Equal(21, repository.LastTake);
    }

    [Fact]
    public async Task GetTenderByIdAsync_MapsDetailsWithNestedCollections()
    {
        var tender = CreateTender("UA-2025-12-02-000002-a", "Замовник", 2_000m);
        tender.ReplaceItems([TenderItem.Create("09310000-5", "Електрична енергія")]);
        tender.ReplaceContracts([TenderContract.Create("contract-1", "award-1", 1_500m, "UAH", DateTimeOffset.UtcNow)]);
        tender.ReplaceSuppliers([TenderSupplier.Create(tender, Supplier.Create("Постачальник", "UA-EDR", "12345678"), "award-1")]);
        var repository = new FakeTenderRepository([tender], []);
        var service = new TenderService(repository);

        var result = await service.GetTenderByIdAsync(tender.Id);

        Assert.NotNull(result);
        Assert.Equal(tender.Id, result.Id);
        Assert.Equal("UA-2025-12-02-000002-a", result.ProzorroId);
        Assert.Equal(1_500m, result.ContractAmount);
        Assert.Single(result.Items);
        Assert.Single(result.Contracts);
        Assert.Single(result.Suppliers);
        Assert.Equal("award-1", result.Contracts[0].AwardId);
        Assert.Equal("12345678", result.Suppliers[0].IdentifierId);
    }

    private static AnalyticsFilter CreateFilter()
    {
        return new AnalyticsFilter(
            "09310000-5",
            new DateTimeOffset(2025, 12, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            10);
    }

    private static Tender CreateTender(string prozorroId, string procuringEntityName, decimal expectedAmount)
    {
        return Tender.Create(
            prozorroId,
            TenderStatus.Complete,
            new DateTimeOffset(2025, 12, 10, 0, 0, 0, TimeSpan.Zero),
            procuringEntityName,
            expectedAmount,
            "UAH",
            DateTimeOffset.UtcNow);
    }

    private static TenderListItemDto CreateListItem(
        Tender tender,
        decimal contractAmount,
        IReadOnlyList<string> suppliers)
    {
        return new TenderListItemDto(
            tender.Id,
            tender.ProzorroId,
            tender.Status,
            tender.DateCreated,
            tender.ProcuringEntityName,
            tender.ExpectedAmount,
            contractAmount,
            tender.Currency,
            suppliers);
    }

    private sealed class FakeTenderRepository(
        IReadOnlyList<Tender> tenders,
        IReadOnlyList<TenderListItemDto> listItems) : ITenderRepository
    {
        public TenderPageCursor? LastCursor { get; private set; }
        public int LastTake { get; private set; }

        public Task<Tender?> GetTenderByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(tenders.SingleOrDefault(tender => tender.Id == id));
        }

        public Task<IReadOnlyList<TenderListItemDto>> SearchCompletedTendersAsync(
            string classificationId,
            DateTimeOffset createdFrom,
            DateTimeOffset createdTo,
            TenderPageCursor? cursor,
            int take,
            CancellationToken cancellationToken = default)
        {
            LastCursor = cursor;
            LastTake = take;
            return Task.FromResult<IReadOnlyList<TenderListItemDto>>(listItems.Take(take).ToList());
        }

        public Task AddAsync(Tender entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Delete(Tender entity) { }
        public Task<bool> ExistsByProzorroIdAsync(string prozorroId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<IReadOnlyList<Tender>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(tenders);
        public Task<Tender?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => GetTenderByIdAsync(id, cancellationToken);
        public Task<Tender?> GetByProzorroIdAsync(string prozorroId, CancellationToken cancellationToken = default) => Task.FromResult(tenders.SingleOrDefault(tender => tender.ProzorroId == prozorroId));
        public Task<IReadOnlyList<Tender>> GetCompletedElectricityTendersAsync(string classificationId, DateTimeOffset createdFrom, DateTimeOffset createdTo, CancellationToken cancellationToken = default) => Task.FromResult(tenders);
        public Task<IReadOnlyList<TopProcuringEntityResult>> GetTopProcuringEntitiesByContractAmountAsync(string classificationId, DateTimeOffset createdFrom, DateTimeOffset createdTo, int limit, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<TopProcuringEntityResult>>([]);
        public Task<decimal> GetTotalSavingsAsync(string classificationId, DateTimeOffset createdFrom, DateTimeOffset createdTo, CancellationToken cancellationToken = default) => Task.FromResult(0m);
        public void Update(Tender entity) { }
    }
}