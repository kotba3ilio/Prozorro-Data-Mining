using Microsoft.EntityFrameworkCore;
using ProzorroDataMining.Application.Abstractions;
using ProzorroDataMining.Domain.Entities.Tenders;

namespace ProzorroDataMining.Infrastructure.Repositories;

public sealed class TenderContractRepository : Repository<TenderContract>, ITenderContractRepository
{
    private readonly DbSet<TenderContract> _dbSet;

    public TenderContractRepository(ApplicationDbContext context)
        : base(context)
    {
        _dbSet = context.Set<TenderContract>();
    }

    public async Task<decimal> GetTotalAmountByTenderIdAsync(
        Guid tenderId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(contract => contract.TenderId == tenderId)
            .SumAsync(contract => contract.Amount, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, decimal>> GetTotalAmountsByTenderIdsAsync(
        IReadOnlyCollection<Guid> tenderIds,
        CancellationToken cancellationToken = default)
    {
        if (tenderIds.Count == 0)
        {
            return new Dictionary<Guid, decimal>();
        }

        return await _dbSet
            .AsNoTracking()
            .Where(contract => tenderIds.Contains(contract.TenderId))
            .GroupBy(contract => contract.TenderId)
            .Select(group => new
            {
                TenderId = group.Key,
                Amount = group.Sum(contract => contract.Amount)
            })
            .ToDictionaryAsync(
                item => item.TenderId,
                item => item.Amount,
                cancellationToken);
    }
}
