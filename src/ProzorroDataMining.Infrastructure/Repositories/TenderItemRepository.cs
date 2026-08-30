using Microsoft.EntityFrameworkCore;
using ProzorroDataMining.Application.Abstractions;
using ProzorroDataMining.Domain.Entities.Tenders;

namespace ProzorroDataMining.Infrastructure.Repositories;

public sealed class TenderItemRepository : Repository<TenderItem>, ITenderItemRepository
{
    private readonly DbSet<TenderItem> _dbSet;

    public TenderItemRepository(ApplicationDbContext context)
        : base(context)
    {
        _dbSet = context.Set<TenderItem>();
    }

    public async Task<bool> TenderHasClassificationAsync(
        Guid tenderId,
        string classificationId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(item =>
                item.TenderId == tenderId &&
                item.ClassificationId == classificationId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> GetTenderIdsByClassificationAsync(
        string classificationId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(item => item.ClassificationId == classificationId)
            .Select(item => item.TenderId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
