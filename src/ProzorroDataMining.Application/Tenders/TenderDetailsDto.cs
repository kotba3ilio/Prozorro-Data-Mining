using ProzorroDataMining.Domain.Entities.Tenders;

namespace ProzorroDataMining.Application.Tenders;

public sealed record TenderDetailsDto(
    Guid Id,
    string ProzorroId,
    TenderStatus Status,
    DateTimeOffset? DateCreated,
    string ProcuringEntityName,
    decimal ExpectedAmount,
    decimal ContractAmount,
    string? Currency,
    DateTimeOffset ImportedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyList<TenderItemDto> Items,
    IReadOnlyList<TenderContractDto> Contracts,
    IReadOnlyList<TenderSupplierDto> Suppliers);
