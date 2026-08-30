using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProzorroDataMining.Application.Abstractions;
using ProzorroDataMining.Application.Import;
using ProzorroDataMining.Domain.Entities.Tenders;
using ProzorroDataMining.Infrastructure.Prozorro;

namespace ProzorroDataMining.Infrastructure.Import;

public sealed class TenderImportService(
    IProzorroTenderClient prozorroTenderClient,
    ITenderRepository tenderRepository,
    ISupplierRepository supplierRepository,
    IUnitOfWork unitOfWork,
    ApplicationDbContext dbContext,
    IOptions<ProzorroApiOptions> options) : ITenderImportService
{
    public async Task<ImportTendersResponse> ImportAsync(
        ImportTendersRequest request,
        CancellationToken cancellationToken = default)
    {
        var prozorroOptions = options.Value;
        var pageSize = Math.Clamp(
            request.PageSize > 0 ? request.PageSize : prozorroOptions.DefaultPageSize,
            1,
            1000);
        var maxPages = Math.Max(
            1,
            request.MaxPages > 0 ? request.MaxPages : prozorroOptions.DefaultMaxPages);
        var importedAt = DateTimeOffset.UtcNow;
        var maxConcurrentDetailRequests = Math.Clamp(
            prozorroOptions.MaxConcurrentDetailRequests,
            1,
            Math.Min(pageSize, 32));
        var syncState = await GetOrCreateSyncStateAsync(importedAt, cancellationToken);
        var pageUri = await ResolveStartPageUriAsync(
            syncState,
            request.Direction,
            pageSize,
            importedAt,
            cancellationToken);
        var nextPageUri = pageUri;
        string? prevPageUri = null;
        var isCompleted = false;
        var feedItemsScanned = 0;
        var candidatesFound = 0;
        var importedCount = 0;
        var updatedCount = 0;
        var skippedCount = 0;

        for (var page = 0; page < maxPages; page++)
        {
            var feed = await prozorroTenderClient.GetTenderFeedAsync(
                nextPageUri,
                pageSize,
                cancellationToken);

            prevPageUri = feed.PrevPage?.Uri;
            nextPageUri = feed.NextPage?.Uri;

            if (feed.Data.Count == 0)
            {
                isCompleted = request.Direction == ImportDirection.Forward;
                SaveCursor(syncState, request.Direction, nextPageUri, prevPageUri, null, importedAt);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                break;
            }

            feedItemsScanned += feed.Data.Count;

            var candidates = feed.Data
                .Where(feedItem => IsCandidate(feedItem, request.CreatedFrom, request.CreatedTo))
                .ToList();

            candidatesFound += candidates.Count;

            var candidateResults = await ReadTenderDetailsAsync(
                candidates,
                request.ClassificationId,
                maxConcurrentDetailRequests,
                cancellationToken);

            skippedCount += candidateResults.Count(candidate => candidate.TenderDetails is null);

            foreach (var candidate in candidateResults.Where(candidate => candidate.TenderDetails is not null))
            {
                var feedItem = candidate.FeedItem;
                var tenderDetails = candidate.TenderDetails!;

                var tender = await tenderRepository.GetByProzorroIdAsync(
                    tenderDetails.ProzorroId,
                    cancellationToken);
                var tenderImportedAt = DateTimeOffset.UtcNow;

                if (tender is null)
                {
                    tender = Tender.Create(
                        tenderDetails.ProzorroId,
                        tenderDetails.Status,
                        tenderDetails.DateCreated,
                        tenderDetails.ProcuringEntityName,
                        tenderDetails.ExpectedAmount,
                        tenderDetails.Currency,
                        tenderImportedAt);

                    await tenderRepository.AddAsync(tender, cancellationToken);
                    importedCount++;
                }
                else
                {
                    dbContext.TenderItems.RemoveRange(tender.Items);
                    dbContext.TenderContracts.RemoveRange(tender.Contracts);
                    dbContext.TenderSuppliers.RemoveRange(tender.Suppliers);

                    tender.UpdateDetails(
                        tenderDetails.Status,
                        tenderDetails.DateCreated,
                        tenderDetails.ProcuringEntityName,
                        tenderDetails.ExpectedAmount,
                        tenderDetails.Currency,
                        tenderImportedAt);

                    updatedCount++;
                }

                tender.ReplaceItems(tenderDetails.Items);
                tender.ReplaceContracts(tenderDetails.Contracts);

                var tenderSuppliers = new List<TenderSupplier>();

                foreach (var supplierDetails in tenderDetails.Suppliers)
                {
                    var supplier = await GetOrCreateSupplierAsync(supplierDetails, cancellationToken);

                    if (tenderSuppliers.Any(tenderSupplier =>
                            tenderSupplier.SupplierId == supplier.Id &&
                            tenderSupplier.AwardId == supplierDetails.AwardId))
                    {
                        continue;
                    }

                    tenderSuppliers.Add(TenderSupplier.Create(tender, supplier, supplierDetails.AwardId));
                }

                tender.ReplaceSuppliers(tenderSuppliers);

                await dbContext.TenderImportPayloads.AddAsync(
                    new TenderImportPayload(
                        tenderDetails.ProzorroId,
                        candidate.RawJson,
                        tenderImportedAt,
                        tender,
                        feedItem.PublicModified,
                        feedItem.DateModified,
                        ComputeSha256(candidate.RawJson)),
                    cancellationToken);
            }

            SaveCursor(
                syncState,
                request.Direction,
                nextPageUri,
                prevPageUri,
                feed.Data.LastOrDefault()?.PublicModified,
                importedAt);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(nextPageUri))
            {
                isCompleted = request.Direction == ImportDirection.Backward;
                break;
            }
        }

        return new ImportTendersResponse(
            request.Direction,
            feedItemsScanned,
            candidatesFound,
            importedCount,
            updatedCount,
            skippedCount,
            isCompleted,
            nextPageUri,
            prevPageUri);
    }

    private async Task<IReadOnlyList<TenderDetailReadResult>> ReadTenderDetailsAsync(
        IReadOnlyList<ProzorroTenderFeedItem> candidates,
        string classificationId,
        int maxDegreeOfParallelism,
        CancellationToken cancellationToken)
    {
        var results = new TenderDetailReadResult[candidates.Count];

        await Parallel.ForEachAsync(
            Enumerable.Range(0, candidates.Count),
            new ParallelOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism,
                CancellationToken = cancellationToken
            },
            async (index, token) =>
            {
                var feedItem = candidates[index];
                var rawJson = await prozorroTenderClient.GetTenderDetailsJsonAsync(feedItem.Id, token);
                var tenderDetails = TryReadTenderDetails(rawJson);

                if (tenderDetails is not null &&
                    !tenderDetails.Items.Any(item => item.ClassificationId == classificationId))
                {
                    tenderDetails = null;
                }

                results[index] = new TenderDetailReadResult(feedItem, rawJson, tenderDetails);
            });

        return results;
    }

    private async Task<ImportSyncState> GetOrCreateSyncStateAsync(
        DateTimeOffset createdAt,
        CancellationToken cancellationToken)
    {
        const string feedName = "tenders";

        var syncState = await dbContext.ImportSyncStates
            .SingleOrDefaultAsync(state => state.FeedName == feedName, cancellationToken);

        if (syncState is not null)
        {
            return syncState;
        }

        syncState = ImportSyncState.Create(feedName, createdAt);
        await dbContext.ImportSyncStates.AddAsync(syncState, cancellationToken);

        return syncState;
    }

    private async Task<string?> ResolveStartPageUriAsync(
        ImportSyncState syncState,
        ImportDirection direction,
        int pageSize,
        DateTimeOffset syncedAt,
        CancellationToken cancellationToken)
    {
        var pageUri = syncState.GetPageUri(direction);

        if (direction == ImportDirection.Backward || !string.IsNullOrWhiteSpace(pageUri))
        {
            return pageUri;
        }

        var initialFeed = await prozorroTenderClient.GetTenderFeedAsync(
            null,
            pageSize,
            cancellationToken);

        syncState.SaveBackwardCursor(
            initialFeed.NextPage?.Uri,
            initialFeed.PrevPage?.Uri,
            initialFeed.Data.LastOrDefault()?.PublicModified,
            syncedAt);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return syncState.GetPageUri(ImportDirection.Forward);
    }

    private static void SaveCursor(
        ImportSyncState syncState,
        ImportDirection direction,
        string? nextPageUri,
        string? prevPageUri,
        decimal? lastPublicModified,
        DateTimeOffset syncedAt)
    {
        if (direction == ImportDirection.Backward)
        {
            syncState.SaveBackwardCursor(nextPageUri, prevPageUri, lastPublicModified, syncedAt);
            return;
        }

        syncState.SaveForwardCursor(nextPageUri, prevPageUri, lastPublicModified, syncedAt);
    }

    private async Task<Supplier> GetOrCreateSupplierAsync(
        SupplierDetails supplierDetails,
        CancellationToken cancellationToken)
    {
        Supplier? supplier = null;

        if (!string.IsNullOrWhiteSpace(supplierDetails.IdentifierScheme) &&
            !string.IsNullOrWhiteSpace(supplierDetails.IdentifierId))
        {
            supplier = await supplierRepository.GetByIdentifierAsync(
                supplierDetails.IdentifierScheme,
                supplierDetails.IdentifierId,
                cancellationToken);
        }

        supplier ??= await supplierRepository.GetByNameAsync(supplierDetails.Name, cancellationToken);

        if (supplier is not null)
        {
            return supplier;
        }

        supplier = Supplier.Create(
            supplierDetails.Name,
            supplierDetails.IdentifierScheme,
            supplierDetails.IdentifierId);

        await supplierRepository.AddAsync(supplier, cancellationToken);

        return supplier;
    }

    private static bool IsCandidate(
        ProzorroTenderFeedItem feedItem,
        DateTimeOffset createdFrom,
        DateTimeOffset createdTo)
    {
        return string.Equals(feedItem.Status, "complete", StringComparison.OrdinalIgnoreCase) &&
               feedItem.DateCreated >= createdFrom &&
               feedItem.DateCreated < createdTo;
    }

    private static TenderDetails? TryReadTenderDetails(string rawJson)
    {
        try
        {
            using var document = JsonDocument.Parse(rawJson);

            if (!document.RootElement.TryGetProperty("data", out var data))
            {
                return null;
            }

            var prozorroId = GetString(data, "id");

            if (string.IsNullOrWhiteSpace(prozorroId))
            {
                return null;
            }

            var status = ReadTenderStatus(GetString(data, "status"));
            var dateCreated = GetDateTimeOffset(data, "dateCreated");
            var expectedAmount = GetDecimal(data, "value", "amount") ?? 0;
            var currency = GetString(data, "value", "currency");
            var procuringEntityName = GetString(data, "procuringEntity", "name") ?? string.Empty;

            return new TenderDetails(
                prozorroId,
                status,
                dateCreated,
                procuringEntityName,
                expectedAmount,
                currency,
                ReadItems(data),
                ReadContracts(data),
                ReadSuppliers(data));
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static IReadOnlyList<TenderItem> ReadItems(JsonElement data)
    {
        if (!data.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return items.EnumerateArray()
            .Select(item => TenderItem.Create(
                GetString(item, "classification", "id") ?? string.Empty,
                GetString(item, "description")))
            .Where(item => !string.IsNullOrWhiteSpace(item.ClassificationId))
            .ToList();
    }

    private static IReadOnlyList<TenderContract> ReadContracts(JsonElement data)
    {
        if (!data.TryGetProperty("contracts", out var contracts) || contracts.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return contracts.EnumerateArray()
            .Select(contract => TenderContract.Create(
                GetString(contract, "id"),
                GetString(contract, "awardID") ?? GetString(contract, "awardId"),
                GetDecimal(contract, "value", "amount") ?? 0,
                GetString(contract, "value", "currency"),
                GetDateTimeOffset(contract, "dateSigned") ?? GetDateTimeOffset(contract, "date")))
            .ToList();
    }

    private static IReadOnlyList<SupplierDetails> ReadSuppliers(JsonElement data)
    {
        if (!data.TryGetProperty("awards", out var awards) || awards.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var suppliers = new List<SupplierDetails>();

        foreach (var award in awards.EnumerateArray())
        {
            var awardId = GetString(award, "id") ?? string.Empty;

            if (!award.TryGetProperty("suppliers", out var awardSuppliers) ||
                awardSuppliers.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var supplier in awardSuppliers.EnumerateArray())
            {
                var name = GetString(supplier, "name");

                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                suppliers.Add(new SupplierDetails(
                    name,
                    GetString(supplier, "identifier", "scheme"),
                    GetString(supplier, "identifier", "id"),
                    awardId));
            }
        }

        return suppliers;
    }

    private static TenderStatus ReadTenderStatus(string? status)
    {
        return status switch
        {
            "draft" => TenderStatus.Draft,
            "active" => TenderStatus.Active,
            "active.enquiries" => TenderStatus.ActiveEnquiries,
            "active.tendering" => TenderStatus.ActiveTendering,
            "active.pre-qualification" => TenderStatus.ActivePreQualification,
            "active.pre-qualification.stand-still" => TenderStatus.ActivePreQualificationStandStill,
            "active.auction" => TenderStatus.ActiveAuction,
            "active.qualification" => TenderStatus.ActiveQualification,
            "active.awarded" => TenderStatus.ActiveAwarded,
            "active.stage2.pending" => TenderStatus.ActiveStage2Pending,
            "unsuccessful" => TenderStatus.Unsuccessful,
            "complete" => TenderStatus.Complete,
            "cancelled" => TenderStatus.Cancelled,
            _ => TenderStatus.Unknown
        };
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) &&
               property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static string? GetString(JsonElement element, string firstPropertyName, string secondPropertyName)
    {
        return element.TryGetProperty(firstPropertyName, out var firstProperty)
            ? GetString(firstProperty, secondPropertyName)
            : null;
    }

    private static decimal? GetDecimal(JsonElement element, string firstPropertyName, string secondPropertyName)
    {
        if (!element.TryGetProperty(firstPropertyName, out var firstProperty) ||
            !firstProperty.TryGetProperty(secondPropertyName, out var secondProperty))
        {
            return null;
        }

        return secondProperty.ValueKind == JsonValueKind.Number && secondProperty.TryGetDecimal(out var value)
            ? value
            : null;
    }

    private static DateTimeOffset? GetDateTimeOffset(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) &&
               property.ValueKind == JsonValueKind.String &&
               property.TryGetDateTimeOffset(out var value)
            ? value
            : null;
    }

    private static string ComputeSha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));

        return Convert.ToHexString(bytes);
    }

    private sealed record TenderDetails(
        string ProzorroId,
        TenderStatus Status,
        DateTimeOffset? DateCreated,
        string ProcuringEntityName,
        decimal ExpectedAmount,
        string? Currency,
        IReadOnlyList<TenderItem> Items,
        IReadOnlyList<TenderContract> Contracts,
        IReadOnlyList<SupplierDetails> Suppliers);

    private sealed record TenderDetailReadResult(
        ProzorroTenderFeedItem FeedItem,
        string RawJson,
        TenderDetails? TenderDetails);

    private sealed record SupplierDetails(
        string Name,
        string? IdentifierScheme,
        string? IdentifierId,
        string AwardId);
}
