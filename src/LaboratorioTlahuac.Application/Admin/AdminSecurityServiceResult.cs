namespace LaboratorioTlahuac.Application.Admin;

public enum AdminSecurityServiceStatus
{
    Success = 1,
    ValidationError = 2,
    NotFound = 3,
    Conflict = 4
}

public sealed record AdminSecurityServiceResult<T>(
    AdminSecurityServiceStatus Status,
    T? Value,
    IReadOnlyDictionary<string, string[]> Errors,
    string? Message);

public static class AdminSecurityServiceResult
{
    public static AdminSecurityServiceResult<T> Success<T>(T value)
    {
        return new AdminSecurityServiceResult<T>(
            AdminSecurityServiceStatus.Success,
            value,
            new Dictionary<string, string[]>(),
            null);
    }

    public static AdminSecurityServiceResult<T> Validation<T>(IReadOnlyDictionary<string, string[]> errors)
    {
        return new AdminSecurityServiceResult<T>(
            AdminSecurityServiceStatus.ValidationError,
            default,
            errors,
            "The request is invalid.");
    }

    public static AdminSecurityServiceResult<T> NotFound<T>(string message)
    {
        return new AdminSecurityServiceResult<T>(
            AdminSecurityServiceStatus.NotFound,
            default,
            new Dictionary<string, string[]>(),
            message);
    }

    public static AdminSecurityServiceResult<T> Conflict<T>(string message)
    {
        return new AdminSecurityServiceResult<T>(
            AdminSecurityServiceStatus.Conflict,
            default,
            new Dictionary<string, string[]>(),
            message);
    }
}
