namespace LaboratorioTlahuac.Application.Payments;

public enum PaymentServiceStatus
{
    Success = 1,
    ValidationError = 2,
    NotFound = 3,
    Conflict = 4
}

public sealed record PaymentServiceResult<T>(
    PaymentServiceStatus Status,
    T? Value,
    IReadOnlyDictionary<string, string[]> Errors,
    string? Message);

public static class PaymentServiceResult
{
    public static PaymentServiceResult<T> Success<T>(T value)
    {
        return new PaymentServiceResult<T>(
            PaymentServiceStatus.Success,
            value,
            new Dictionary<string, string[]>(),
            null);
    }

    public static PaymentServiceResult<T> Validation<T>(IReadOnlyDictionary<string, string[]> errors)
    {
        return new PaymentServiceResult<T>(
            PaymentServiceStatus.ValidationError,
            default,
            errors,
            "The request is invalid.");
    }

    public static PaymentServiceResult<T> NotFound<T>(string message)
    {
        return new PaymentServiceResult<T>(
            PaymentServiceStatus.NotFound,
            default,
            new Dictionary<string, string[]>(),
            message);
    }

    public static PaymentServiceResult<T> Conflict<T>(string message)
    {
        return new PaymentServiceResult<T>(
            PaymentServiceStatus.Conflict,
            default,
            new Dictionary<string, string[]>(),
            message);
    }
}
