using ProzorroDataMining.Application.Abstractions;
using ProzorroDataMining.Application.Models.Analytics;

namespace ProzorroDataMining.Application.Analytics;

public sealed class AnalyticsService(
    ITenderRepository tenderRepository,
    ISupplierRepository supplierRepository) : IAnalyticsService
{
    public async Task<decimal> GetTotalSavingsAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default)
    {
        return await tenderRepository.GetTotalSavingsAsync(
            filter.ClassificationId,
            filter.CreatedFrom,
            filter.CreatedTo,
            cancellationToken);
    }

    public async Task<IReadOnlyList<TopProcuringEntityResult>> GetTopProcuringEntitiesAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default)
    {
        return await tenderRepository.GetTopProcuringEntitiesByContractAmountAsync(
            filter.ClassificationId,
            filter.CreatedFrom,
            filter.CreatedTo,
            filter.Limit,
            cancellationToken);
    }

    public async Task<IReadOnlyList<TopSupplierResult>> GetTopSuppliersAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default)
    {
        return await supplierRepository.GetTopSuppliersByContractAmountAsync(
            filter.ClassificationId,
            filter.CreatedFrom,
            filter.CreatedTo,
            filter.Limit,
            cancellationToken);
    }

    public async Task<AnalyticsSummaryResponse> GetSummaryAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default)
    {
        var totalSavings = await GetTotalSavingsAsync(filter, cancellationToken);
        var topProcuringEntities = await GetTopProcuringEntitiesAsync(filter, cancellationToken);
        var topSuppliers = await GetTopSuppliersAsync(filter, cancellationToken);

        return new AnalyticsSummaryResponse(
            totalSavings,
            topProcuringEntities,
            topSuppliers);
    }
}
