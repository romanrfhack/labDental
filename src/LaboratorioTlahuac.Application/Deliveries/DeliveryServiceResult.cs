namespace LaboratorioTlahuac.Application.Deliveries;

public enum DeliveryServiceStatus
{
    Success = 1,
    ValidationError = 2,
    NotFound = 3,
    Conflict = 4,
    Forbidden = 5
}

public sealed record DeliveryServiceResult<T>(
    DeliveryServiceStatus Status,
    T? Value,
    IReadOnlyDictionary<string, string[]> Errors,
    string? Message);

public static class DeliveryServiceResult
{
    public static DeliveryServiceResult<T> Success<T>(T value)
    {
        return new DeliveryServiceResult<T>(
            DeliveryServiceStatus.Success,
            value,
            new Dictionary<string, string[]>(),
            null);
    }

    public static DeliveryServiceResult<T> Validation<T>(IReadOnlyDictionary<string, string[]> errors)
    {
        return new DeliveryServiceResult<T>(
            DeliveryServiceStatus.ValidationError,
            default,
            errors,
            "The request is invalid.");
    }

    public static DeliveryServiceResult<T> NotFound<T>(string message)
    {
        return new DeliveryServiceResult<T>(
            DeliveryServiceStatus.NotFound,
            default,
            new Dictionary<string, string[]>(),
            message);
    }

    public static DeliveryServiceResult<T> Conflict<T>(string message)
    {
        return new DeliveryServiceResult<T>(
            DeliveryServiceStatus.Conflict,
            default,
            new Dictionary<string, string[]>(),
            message);
    }

    public static DeliveryServiceResult<T> Forbidden<T>(string message)
    {
        return new DeliveryServiceResult<T>(
            DeliveryServiceStatus.Forbidden,
            default,
            new Dictionary<string, string[]>(),
            message);
    }
}
