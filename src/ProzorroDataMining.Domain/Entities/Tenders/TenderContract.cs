namespace ProzorroDataMining.Domain.Entities.Tenders;

public sealed class TenderContract : Entity
{
    private TenderContract()
    {
    }

    private TenderContract(
        string? prozorroContractId,
        string? awardId,
        decimal amount,
        string? currency,
        DateTimeOffset? dateSigned)
    {
        ProzorroContractId = prozorroContractId;
        AwardId = awardId ?? string.Empty;
        Amount = amount;
        Currency = currency;
        DateSigned = dateSigned?.ToUniversalTime();
    }

    public Guid TenderId { get; private set; }

    public Tender? Tender { get; private set; }

    public string? ProzorroContractId { get; private set; }

    public string AwardId { get; private set; } = string.Empty;

    public decimal Amount { get; private set; }

    public string? Currency { get; private set; }

    public DateTimeOffset? DateSigned { get; private set; }

    public static TenderContract Create(
        string? prozorroContractId,
        string? awardId,
        decimal amount,
        string? currency,
        DateTimeOffset? dateSigned)
    {
        return new TenderContract(
            prozorroContractId,
            awardId,
            amount,
            currency,
            dateSigned);
    }
}
