using Dapper;
using Microsoft.EntityFrameworkCore;
using ProzorroDataMining.Application.Abstractions;
using ProzorroDataMining.Application.Models.Analytics;
using ProzorroDataMining.Domain.Entities.Tenders;

namespace ProzorroDataMining.Infrastructure.Repositories;

public sealed class SupplierRepository : Repository<Supplier>, ISupplierRepository
{
    private const int CompleteTenderStatus = (int)TenderStatus.Complete;
    private readonly DbSet<Supplier> _dbSet;
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public SupplierRepository(
        ApplicationDbContext context,
        IDbConnectionFactory dbConnectionFactory)
        : base(context)
    {
        _dbSet = context.Set<Supplier>();
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Supplier?> GetByIdentifierAsync(
        string identifierScheme,
        string identifierId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(supplier =>
                supplier.IdentifierScheme == identifierScheme &&
                supplier.IdentifierId == identifierId,
                cancellationToken);
    }

    public async Task<Supplier?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(supplier => supplier.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<TopSupplierResult>> GetTopSuppliersByContractAmountAsync(
        string classificationId,
        DateTimeOffset createdFrom,
        DateTimeOffset createdTo,
        int limit,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            WITH filtered_tenders AS (
                SELECT t."Id"
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
            ),
            supplier_contracts AS (
                SELECT DISTINCT ts."SupplierId", c."Id" AS "ContractId", c."Amount"
                FROM tender_contracts c
                INNER JOIN filtered_tenders ft ON ft."Id" = c."TenderId"
                INNER JOIN tender_suppliers ts
                    ON ts."TenderId" = c."TenderId"
                   AND ts."AwardId" = c."AwardId"
                WHERE c."AwardId" <> ''
            )
            SELECT s."Name" AS "SupplierName", COALESCE(SUM(sc."Amount"), 0) AS "ContractAmount"
            FROM supplier_contracts sc
            INNER JOIN suppliers s ON s."Id" = sc."SupplierId"
            GROUP BY s."Name"
            ORDER BY "ContractAmount" DESC
            LIMIT @Limit;
            """;

        using var connection = _dbConnectionFactory.CreateConnection();
        var results = await connection.QueryAsync<TopSupplierResult>(
            new CommandDefinition(
                sql,
                new
                {
                    ClassificationId = classificationId,
                    CreatedFrom = createdFrom.ToUniversalTime(),
                    CreatedTo = createdTo.ToUniversalTime(),
                    CompleteStatus = CompleteTenderStatus,
                    Limit = limit
                },
                cancellationToken: cancellationToken));

        return results.ToList();
    }
}
