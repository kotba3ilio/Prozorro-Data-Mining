namespace ProzorroDataMining.Application.Import;

public sealed record ImportTendersRequest(
    string ClassificationId,
    DateTimeOffset CreatedFrom,
    DateTimeOffset CreatedTo,
    int MaxPages,
    int PageSize,
    ImportDirection Direction);