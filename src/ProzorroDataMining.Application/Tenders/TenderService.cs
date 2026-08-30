using ProzorroDataMining.Application.Abstractions;
using ProzorroDataMining.Application.Analytics;
using ProzorroDataMining.Domain.Entities.Tenders;

namespace ProzorroDataMining.Application.Tenders;

public sealed class TenderService(ITenderRepository tenderRepository) : ITenderService
{
    public async Task<CursorPagedResponse<TenderListItemDto>> TendersAsync(
        AnalyticsFilter filter,
        string? cursor,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (!TenderPageCursor.TryDecode(cursor, out var pageCursor))
        {
            throw new ArgumentException("Invalid tender page cursor.", nameof(cursor));
        }

        var currentPageSize = Math.Clamp(pageSize, 1, 100);
        var tenders = await tenderRepository.SearchCompletedTendersAsync(
            filter.ClassificationId,
            filter.CreatedFrom,
            filter.CreatedTo,
            pageCursor,
            currentPageSize + 1,
            cancellationToken);
        var items = tenders.Take(currentPageSize).ToList();
        var lastItem = items.LastOrDefault();
        var nextCursor = tenders.Count > currentPageSize && lastItem?.DateCreated is not null
            ? new TenderPageCursor(lastItem.DateCreated.Value, lastItem.Id).Encode()
            : null;

        return new CursorPagedResponse<TenderListItemDto>(
            items,
            currentPageSize,
            nextCursor,
            nextCursor is not null);
    }

    public async Task<TenderDetailsDto?> GetTenderByIdAsync(
        Guid tenderId,
        CancellationToken cancellationToken = default)
    {
        var tender = await tenderRepository.GetTenderByIdAsync(tenderId, cancellationToken);

        return tender is null ? null : MapToDetails(tender);
    }

    private static TenderDetailsDto MapToDetails(Tender tender)
    {
        return new TenderDetailsDto(
            tender.Id,
            tender.ProzorroId,
            tender.Status,
            tender.DateCreated,
            tender.ProcuringEntityName,
            tender.ExpectedAmount,
            GetContractAmount(tender),
            tender.Currency,
            tender.ImportedAt,
            tender.UpdatedAt,
            tender.Items
                .Select(item => new TenderItemDto(
                    item.ClassificationId,
                    item.Description))
                .ToList(),
            tender.Contracts
                .Select(contract => new TenderContractDto(
                    contract.ProzorroContractId,
                    contract.AwardId,
                    contract.Amount,
                    contract.Currency,
                    contract.DateSigned))
                .ToList(),
            tender.Suppliers
                .Where(tenderSupplier => tenderSupplier.Supplier is not null)
                .Select(tenderSupplier => new TenderSupplierDto(
                    tenderSupplier.Supplier!.Name,
                    tenderSupplier.Supplier.IdentifierScheme,
                    tenderSupplier.Supplier.IdentifierId,
                    tenderSupplier.AwardId))
                .ToList());
    }

    private static decimal GetContractAmount(Tender tender)
    {
        return tender.Contracts.Sum(contract => contract.Amount);
    }
}