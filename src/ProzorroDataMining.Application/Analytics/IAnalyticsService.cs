using ProzorroDataMining.Application.Models.Analytics;

namespace ProzorroDataMining.Application.Analytics;

public interface IAnalyticsService
{
    Task<decimal> GetTotalSavingsAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TopProcuringEntityResult>> GetTopProcuringEntitiesAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TopSupplierResult>> GetTopSuppliersAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default);

    Task<AnalyticsSummaryResponse> GetSummaryAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default);
}