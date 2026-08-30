namespace ProzorroDataMining.Application.Tenders;

public sealed record TenderContractDto(
    string? ProzorroContractId,
    string AwardId,
    decimal Amount,
    string? Currency,
    DateTimeOffset? DateSigned);
