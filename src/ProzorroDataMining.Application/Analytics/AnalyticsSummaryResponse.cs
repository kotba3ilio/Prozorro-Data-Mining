using ProzorroDataMining.Application.Models.Analytics;

namespace ProzorroDataMining.Application.Analytics;

public sealed record AnalyticsSummaryResponse(
    decimal TotalSavings,
    IReadOnlyList<TopProcuringEntityResult> TopProcuringEntities,
    IReadOnlyList<TopSupplierResult> TopSuppliers);