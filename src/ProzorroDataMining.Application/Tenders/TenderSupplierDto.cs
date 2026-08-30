namespace ProzorroDataMining.Application.Tenders;

public sealed record TenderSupplierDto(
    string Name,
    string? IdentifierScheme,
    string? IdentifierId,
    string? AwardId);
