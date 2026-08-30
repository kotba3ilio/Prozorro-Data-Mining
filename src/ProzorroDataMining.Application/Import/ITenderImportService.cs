namespace ProzorroDataMining.Application.Import;

public interface ITenderImportService
{
    Task<ImportTendersResponse> ImportAsync(
        ImportTendersRequest request,
        CancellationToken cancellationToken = default);
}