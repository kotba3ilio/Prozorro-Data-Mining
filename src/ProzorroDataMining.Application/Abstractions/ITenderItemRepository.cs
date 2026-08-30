using ProzorroDataMining.Domain.Entities.Tenders;

namespace ProzorroDataMining.Application.Abstractions;

public interface ITenderItemRepository : IRepository<TenderItem>
{
    Task<bool> TenderHasClassificationAsync(
        Guid tenderId,
        string classificationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetTenderIdsByClassificationAsync(
        string classificationId,
        CancellationToken cancellationToken = default);
}
