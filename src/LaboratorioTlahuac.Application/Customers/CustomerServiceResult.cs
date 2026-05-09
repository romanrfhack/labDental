namespace LaboratorioTlahuac.Application.Customers;

public enum CustomerServiceStatus
{
    Success = 1,
    ValidationError = 2,
    NotFound = 3,
    Conflict = 4
}

public sealed record CustomerServiceResult<T>(
    CustomerServiceStatus Status,
    T? Value,
    IReadOnlyDictionary<string, string[]> Errors,
    string? Message);

public static class CustomerServiceResult
{
    public static CustomerServiceResult<T> Success<T>(T value)
    {
        return new CustomerServiceResult<T>(
            CustomerServiceStatus.Success,
            value,
            new Dictionary<string, string[]>(),
            null);
    }

    public static CustomerServiceResult<T> Validation<T>(IReadOnlyDictionary<string, string[]> errors)
    {
        return new CustomerServiceResult<T>(
            CustomerServiceStatus.ValidationError,
            default,
            errors,
            "The request is invalid.");
    }

    public static CustomerServiceResult<T> NotFound<T>(string message)
    {
        return new CustomerServiceResult<T>(
            CustomerServiceStatus.NotFound,
            default,
            new Dictionary<string, string[]>(),
            message);
    }

    public static CustomerServiceResult<T> Conflict<T>(string message)
    {
        return new CustomerServiceResult<T>(
            CustomerServiceStatus.Conflict,
            default,
            new Dictionary<string, string[]>(),
            message);
    }
}
