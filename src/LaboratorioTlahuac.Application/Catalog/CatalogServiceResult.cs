namespace LaboratorioTlahuac.Application.Catalog;

public enum CatalogServiceStatus
{
    Success = 1,
    ValidationError = 2,
    NotFound = 3,
    Conflict = 4,
    PayloadTooLarge = 5,
    ServiceUnavailable = 6
}

public sealed record CatalogServiceResult<T>(
    CatalogServiceStatus Status,
    T? Value,
    IReadOnlyDictionary<string, string[]> Errors,
    string? Message);

public static class CatalogServiceResult
{
    public static CatalogServiceResult<T> Success<T>(T value)
    {
        return new CatalogServiceResult<T>(
            CatalogServiceStatus.Success,
            value,
            new Dictionary<string, string[]>(),
            null);
    }

    public static CatalogServiceResult<T> Validation<T>(IReadOnlyDictionary<string, string[]> errors)
    {
        return new CatalogServiceResult<T>(
            CatalogServiceStatus.ValidationError,
            default,
            errors,
            "The request is invalid.");
    }

    public static CatalogServiceResult<T> NotFound<T>(string message)
    {
        return new CatalogServiceResult<T>(
            CatalogServiceStatus.NotFound,
            default,
            new Dictionary<string, string[]>(),
            message);
    }

    public static CatalogServiceResult<T> Conflict<T>(string message)
    {
        return new CatalogServiceResult<T>(
            CatalogServiceStatus.Conflict,
            default,
            new Dictionary<string, string[]>(),
            message);
    }

    public static CatalogServiceResult<T> PayloadTooLarge<T>(string message)
    {
        return new CatalogServiceResult<T>(
            CatalogServiceStatus.PayloadTooLarge,
            default,
            new Dictionary<string, string[]>(),
            message);
    }

    public static CatalogServiceResult<T> ServiceUnavailable<T>(string message)
    {
        return new CatalogServiceResult<T>(
            CatalogServiceStatus.ServiceUnavailable,
            default,
            new Dictionary<string, string[]>(),
            message);
    }
}
