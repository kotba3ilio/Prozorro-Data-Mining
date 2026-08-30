using ProzorroDataMining.Domain.Entities.Tenders;

namespace ProzorroDataMining.Application.Tenders;

public sealed record TenderListItemDto(
    Guid Id,
    string ProzorroId,
    TenderStatus Status,
    DateTimeOffset? DateCreated,
    string ProcuringEntityName,
    decimal ExpectedAmount,
    decimal ContractAmount,
    string? Currency,
    IReadOnlyList<string> Suppliers);
