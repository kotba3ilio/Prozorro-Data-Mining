using Dapper;
using Microsoft.EntityFrameworkCore;
using ProzorroDataMining.Application.Abstractions;
using ProzorroDataMining.Application.Models.Analytics;
using ProzorroDataMining.Application.Tenders;
using ProzorroDataMining.Domain.Entities;
using ProzorroDataMining.Domain.Entities.Tenders;
using static Dapper.SqlMapper;

namespace ProzorroDataMining.Infrastructure.Repositories;

public sealed class TenderRepository : Repository<Tender>, ITenderRepository
{
    private const int CompleteTenderStatus = (int)TenderStatus.Complete;
    private readonly DbSet<Tender> _dbSet;
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public TenderRepository(
        ApplicationDbContext context,
        IDbConnectionFactory dbConnectionFactory)
        : base(context)
    {
        _dbSet = context.Set<Tender>();
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Tender?> GetTenderByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(tender => tender.Contracts)
            .Include(tender => tender.Items)
            .Include(tender => tender.Suppliers)
                .ThenInclude(tenderSupplier => tenderSupplier.Supplier)
            .AsSplitQuery()
            .FirstOrDefaultAsync(tender => tender.Id == id, cancellationToken);
    }

    public async Task<Tender?> GetByProzorroIdAsync(
        string prozorroId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(tender => tender.Items)
            .Include(tender => tender.Contracts)
            .Include(tender => tender.Suppliers)
                .ThenInclude(tenderSupplier => tenderSupplier.Supplier)
            .AsSplitQuery()
            .FirstOrDefaultAsync(tender => tender.ProzorroId == prozorroId, cancellationToken);
    }

    public async Task<bool> ExistsByProzorroIdAsync(
        string prozorroId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(tender => tender.ProzorroId == prozorroId, cancellationToken);
    }

    public async Task<IReadOnlyList<Tender>> GetCompletedElectricityTendersAsync(
        string classificationId,
        DateTimeOffset createdFrom,
        DateTimeOffset createdTo,
        CancellationToken cancellationToken = default)
    {
        return await GetCompletedTendersQuery(classificationId, createdFrom, createdTo)
            .Include(tender => tender.Items)
            .Include(tender => tender.Contracts)
            .Include(tender => tender.Suppliers)
                .ThenInclude(tenderSupplier => tenderSupplier.Supplier)
            .ToListAsync(cancellationToken);
    }


    public async Task<IReadOnlyList<TenderListItemDto>> SearchCompletedTendersAsync(
        string classificationId,
        DateTimeOffset createdFrom,
        DateTimeOffset createdTo,
        TenderPageCursor? cursor,
        int take,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            WITH filtered_tenders AS (
                SELECT
                    t."Id",
                    t."ProzorroId",
                    t."Status",
                    t."DateCreated",
                    t."ProcuringEntityName",
                    t."ExpectedAmount",
                    t."Currency"
                FROM tenders t
                WHERE t."Status" = @CompleteStatus
                  AND t."DateCreated" >= @CreatedFrom
                  AND t."DateCreated" < @CreatedTo
                  AND EXISTS (
                      SELECT 1
                      FROM tender_items i
                      WHERE i."TenderId" = t."Id"
                        AND i."ClassificationId" = @ClassificationId
                  )
                  AND (
                      @CursorDateCreated IS NULL
                      OR t."DateCreated" < @CursorDateCreated
                      OR (t."DateCreated" = @CursorDateCreated AND t."Id" > @CursorId)
                  )
                ORDER BY t."DateCreated" DESC, t."Id"
                LIMIT @Take
            ),
            contract_totals AS (
                SELECT c."TenderId", SUM(c."Amount") AS "ContractAmount"
                FROM tender_contracts c
                INNER JOIN filtered_tenders ft ON ft."Id" = c."TenderId"
                GROUP BY c."TenderId"
            ),
            supplier_names AS (
                SELECT ts."TenderId", ARRAY_AGG(DISTINCT s."Name" ORDER BY s."Name") AS "Suppliers"
                FROM tender_suppliers ts
                INNER JOIN suppliers s ON s."Id" = ts."SupplierId"
                INNER JOIN filtered_tenders ft ON ft."Id" = ts."TenderId"
                GROUP BY ts."TenderId"
            )
            SELECT
                ft."Id",
                ft."ProzorroId",
                ft."Status",
                ft."DateCreated",
                ft."ProcuringEntityName",
                ft."ExpectedAmount",
                COALESCE(ct."ContractAmount", 0) AS "ContractAmount",
                ft."Currency",
                COALESCE(sn."Suppliers", ARRAY[]::text[]) AS "Suppliers"
            FROM filtered_tenders ft
            LEFT JOIN contract_totals ct ON ct."TenderId" = ft."Id"
            LEFT JOIN supplier_names sn ON sn."TenderId" = ft."Id"
            ORDER BY ft."DateCreated" DESC, ft."Id";
            """;

        var currentTake = Math.Clamp(take, 1, 101);

        using var connection = _dbConnectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<TenderListItemRow>(
            new CommandDefinition(
                sql,
                new
                {
                    ClassificationId = classificationId,
                    CreatedFrom = createdFrom.ToUniversalTime(),
                    CreatedTo = createdTo.ToUniversalTime(),
                    CompleteStatus = CompleteTenderStatus,
                    CursorDateCreated = cursor?.DateCreated.ToUniversalTime(),
                    CursorId = cursor?.Id,
                    Take = currentTake
                },
                cancellationToken: cancellationToken));

        return rows
            .Select(row => new TenderListItemDto(
                row.Id,
                row.ProzorroId,
                (TenderStatus)row.Status,
                ToDateTimeOffset(row.DateCreated),
                row.ProcuringEntityName,
                row.ExpectedAmount,
                row.ContractAmount,
                row.Currency,
                row.Suppliers))
            .ToList();
    }

    public async Task<decimal> GetTotalSavingsAsync(
        string classificationId,
        DateTimeOffset createdFrom,
        DateTimeOffset createdTo,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            WITH filtered_tenders AS (
                SELECT t."Id", t."ExpectedAmount"
                FROM tenders t
                WHERE t."Status" = @CompleteStatus
                  AND t."DateCreated" >= @CreatedFrom
                  AND t."DateCreated" < @CreatedTo
                  AND EXISTS (
                      SELECT 1
                      FROM tender_items i
                      WHERE i."TenderId" = t."Id"
                        AND i."ClassificationId" = @ClassificationId
                  )
            )
            SELECT COALESCE(SUM(ft."ExpectedAmount"), 0)
                - COALESCE((
                    SELECT SUM(c."Amount")
                    FROM tender_contracts c
                    INNER JOIN filtered_tenders contract_tenders ON contract_tenders."Id" = c."TenderId"
                ), 0)
            FROM filtered_tenders ft;
            """;

        using var connection = _dbConnectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<decimal>(
            CreateCommand(sql, classificationId, createdFrom, createdTo, null, cancellationToken));
    }

    public async Task<IReadOnlyList<TopProcuringEntityResult>> GetTopProcuringEntitiesByContractAmountAsync(
        string classificationId,
        DateTimeOffset createdFrom,
        DateTimeOffset createdTo,
        int limit,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            WITH filtered_tenders AS (
                SELECT t."Id", t."ProcuringEntityName"
                FROM tenders t
                WHERE t."Status" = @CompleteStatus
                  AND t."DateCreated" >= @CreatedFrom
                  AND t."DateCreated" < @CreatedTo
                  AND EXISTS (
                      SELECT 1
                      FROM tender_items i
                      WHERE i."TenderId" = t."Id"
                        AND i."ClassificationId" = @ClassificationId
                  )
            )
            SELECT ft."ProcuringEntityName", COALESCE(SUM(c."Amount"), 0) AS "ContractAmount"
            FROM filtered_tenders ft
            INNER JOIN tender_contracts c ON c."TenderId" = ft."Id"
            GROUP BY ft."ProcuringEntityName"
            ORDER BY "ContractAmount" DESC
            LIMIT @Limit;
            """;

        using var connection = _dbConnectionFactory.CreateConnection();
        var results = await connection.QueryAsync<TopProcuringEntityResult>(
            CreateCommand(sql, classificationId, createdFrom, createdTo, limit, cancellationToken));

        return results.ToList();
    }

    private static CommandDefinition CreateCommand(
        string sql,
        string classificationId,
        DateTimeOffset createdFrom,
        DateTimeOffset createdTo,
        int? limit,
        CancellationToken cancellationToken)
    {
        return new CommandDefinition(
            sql,
            new
            {
                ClassificationId = classificationId,
                CreatedFrom = createdFrom.ToUniversalTime(),
                CreatedTo = createdTo.ToUniversalTime(),
                CompleteStatus = CompleteTenderStatus,
                Limit = limit
            },
            cancellationToken: cancellationToken);
    }

    private IQueryable<Tender> GetCompletedTendersQuery(
        string classificationId,
        DateTimeOffset createdFrom,
        DateTimeOffset createdTo)
    {
        var createdFromUtc = createdFrom.ToUniversalTime();
        var createdToUtc = createdTo.ToUniversalTime();

        return _dbSet
            .AsNoTracking()
            .Where(tender =>
                tender.Status == TenderStatus.Complete &&
                tender.DateCreated >= createdFromUtc &&
                tender.DateCreated < createdToUtc &&
                tender.Items.Any(item => item.ClassificationId == classificationId));
    }

    private static DateTimeOffset? ToDateTimeOffset(DateTime? value)
    {
        return value is null
            ? null
            : new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));
    }

    private sealed class TenderListItemRow
    {
        public Guid Id { get; init; }

        public string ProzorroId { get; init; } = string.Empty;

        public int Status { get; init; }

        public DateTime? DateCreated { get; init; }

        public string ProcuringEntityName { get; init; } = string.Empty;

        public decimal ExpectedAmount { get; init; }

        public decimal ContractAmount { get; init; }

        public string? Currency { get; init; }

        public string[] Suppliers { get; init; } = [];
    }
}
