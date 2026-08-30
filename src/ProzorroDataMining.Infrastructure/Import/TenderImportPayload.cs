using ProzorroDataMining.Domain.Entities.Tenders;

namespace ProzorroDataMining.Infrastructure.Import;

public sealed class TenderImportPayload
{
    public Guid Id { get; private set; }

    public Guid? TenderId { get; private set; }

    public Tender? Tender { get; private set; }

    public string ProzorroId { get; private set; } = null!;

    public decimal? PublicModified { get; private set; }

    public DateTimeOffset? SourceDateModified { get; private set; }

    public string Payload { get; private set; } = null!;

    public string? PayloadHash { get; private set; }

    public DateTimeOffset ImportedAt { get; private set; }

    private TenderImportPayload()
    {
    }

    public TenderImportPayload(
        string prozorroId,
        string payload,
        DateTimeOffset importedAt,
        Guid? tenderId = null,
        decimal? publicModified = null,
        DateTimeOffset? sourceDateModified = null,
        string? payloadHash = null)
    {
        Id = Guid.NewGuid();
        TenderId = tenderId;
        ProzorroId = prozorroId;
        PublicModified = publicModified;
        SourceDateModified = sourceDateModified?.ToUniversalTime();
        Payload = payload;
        PayloadHash = payloadHash;
        ImportedAt = importedAt.ToUniversalTime();
    }

    public TenderImportPayload(
        string prozorroId,
        string payload,
        DateTimeOffset importedAt,
        Tender tender,
        decimal? publicModified = null,
        DateTimeOffset? sourceDateModified = null,
        string? payloadHash = null)
        : this(
            prozorroId,
            payload,
            importedAt,
            tender.Id,
            publicModified,
            sourceDateModified,
            payloadHash)
    {
        Tender = tender;
    }
}
