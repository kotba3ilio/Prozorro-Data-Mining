namespace ProzorroDataMining.Application.Models.Analytics;

public sealed record TopProcuringEntityResult(
    string ProcuringEntityName,
    decimal ContractAmount);