using ProzorroDataMining.Domain.Entities.Tenders;

namespace ProzorroDataMining.Application.Abstractions;

public interface ITenderContractRepository : IRepository<TenderContract>
{
    Task<decimal> GetTotalAmountByTenderIdAsync(
        Guid tenderId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, decimal>> GetTotalAmountsByTenderIdsAsync(
        IReadOnlyCollection<Guid> tenderIds,
        CancellationToken cancellationToken = default);
}
