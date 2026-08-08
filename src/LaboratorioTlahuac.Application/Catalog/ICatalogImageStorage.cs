namespace LaboratorioTlahuac.Application.Catalog;

public interface ICatalogImageStorage
{
    Task<CatalogImageStoreResult> StoreAsync(
        CatalogImageUploadRequest request,
        CancellationToken cancellationToken = default);

    Task<CatalogImageContent?> OpenReadAsync(
        string fileName,
        CancellationToken cancellationToken = default);

    Task TryDeleteAsync(
        string fileName,
        CancellationToken cancellationToken = default);
}

public enum CatalogImageStoreStatus
{
    Success = 1,
    ValidationError = 2,
    PayloadTooLarge = 3,
    ServiceUnavailable = 4
}

public sealed record CatalogImageStoreResult(
    CatalogImageStoreStatus Status,
    string? FileName,
    IReadOnlyDictionary<string, string[]> Errors);
