using ProzorroDataMining.Application.Models.Analytics;
using ProzorroDataMining.Application.Tenders;
using ProzorroDataMining.Domain.Entities.Tenders;

namespace ProzorroDataMining.Application.Abstractions;

public interface ITenderRepository : IRepository<Tender>
{
    Task<Tender?> GetTenderByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tender?> GetByProzorroIdAsync(string prozorroId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByProzorroIdAsync(string prozorroId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Tender>> GetCompletedElectricityTendersAsync(
        string classificationId,
        DateTimeOffset createdFrom,
        DateTimeOffset createdTo,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TenderListItemDto>> SearchCompletedTendersAsync(
        string classificationId,
        DateTimeOffset createdFrom,
        DateTimeOffset createdTo,
        TenderPageCursor? cursor,
        int take,
        CancellationToken cancellationToken = default);

    Task<decimal> GetTotalSavingsAsync(
        string classificationId,
        DateTimeOffset createdFrom,
        DateTimeOffset createdTo,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TopProcuringEntityResult>> GetTopProcuringEntitiesByContractAmountAsync(
        string classificationId,
        DateTimeOffset createdFrom,
        DateTimeOffset createdTo,
        int limit,
        CancellationToken cancellationToken = default);
}
