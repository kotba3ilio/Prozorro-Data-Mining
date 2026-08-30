using ProzorroDataMining.Application.Models.Analytics;
using ProzorroDataMining.Domain.Entities.Tenders;

namespace ProzorroDataMining.Application.Abstractions;

public interface ISupplierRepository : IRepository<Supplier>
{
    Task<Supplier?> GetByIdentifierAsync(
        string identifierScheme,
        string identifierId,
        CancellationToken cancellationToken = default);

    Task<Supplier?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TopSupplierResult>> GetTopSuppliersByContractAmountAsync(
        string classificationId,
        DateTimeOffset createdFrom,
        DateTimeOffset createdTo,
        int limit,
        CancellationToken cancellationToken = default);
}
