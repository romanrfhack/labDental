namespace LaboratorioTlahuac.Application.WorkOrders;

public enum WorkOrderServiceStatus
{
    Success = 1,
    ValidationError = 2,
    NotFound = 3,
    Conflict = 4
}

public sealed record WorkOrderServiceResult<T>(
    WorkOrderServiceStatus Status,
    T? Value,
    IReadOnlyDictionary<string, string[]> Errors,
    string? Message);

public static class WorkOrderServiceResult
{
    public static WorkOrderServiceResult<T> Success<T>(T value)
    {
        return new WorkOrderServiceResult<T>(
            WorkOrderServiceStatus.Success,
            value,
            new Dictionary<string, string[]>(),
            null);
    }

    public static WorkOrderServiceResult<T> Validation<T>(IReadOnlyDictionary<string, string[]> errors)
    {
        return new WorkOrderServiceResult<T>(
            WorkOrderServiceStatus.ValidationError,
            default,
            errors,
            "The request is invalid.");
    }

    public static WorkOrderServiceResult<T> NotFound<T>(string message)
    {
        return new WorkOrderServiceResult<T>(
            WorkOrderServiceStatus.NotFound,
            default,
            new Dictionary<string, string[]>(),
            message);
    }

    public static WorkOrderServiceResult<T> Conflict<T>(string message)
    {
        return new WorkOrderServiceResult<T>(
            WorkOrderServiceStatus.Conflict,
            default,
            new Dictionary<string, string[]>(),
            message);
    }
}
