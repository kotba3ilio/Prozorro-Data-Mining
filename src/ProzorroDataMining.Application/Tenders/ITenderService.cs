using ProzorroDataMining.Application.Analytics;

namespace ProzorroDataMining.Application.Tenders;

public interface ITenderService
{
    Task<CursorPagedResponse<TenderListItemDto>> TendersAsync(
        AnalyticsFilter filter,
        string? cursor,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<TenderDetailsDto?> GetTenderByIdAsync(
        Guid tenderId,
        CancellationToken cancellationToken = default);
}