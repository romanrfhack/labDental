namespace LaboratorioTlahuac.Application.Payments;

public sealed record WorkOrderPaymentListQuery(bool? IncludeCancelled);

public sealed record PaymentListQuery(
    string? Search,
    Guid? CustomerId,
    Guid? WorkOrderId,
    string? Method,
    DateOnly? PaymentDateFrom,
    DateOnly? PaymentDateTo,
    bool? IncludeCancelled,
    int? Page,
    int? PageSize);

public sealed record PaymentCreateRequest(
    DateOnly PaymentDate,
    decimal Amount,
    string? Method,
    string? Reference,
    string? Notes);

public sealed record PaymentCancelRequest(string? Reason);

public sealed record PaymentPagedResponse<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record PaymentResponse(
    Guid Id,
    Guid WorkOrderId,
    DateOnly PaymentDate,
    decimal Amount,
    string Method,
    string MethodLabel,
    string? Reference,
    string? Notes,
    bool IsCancelled,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CancelledAtUtc,
    string? CancellationReason);

public sealed record PaymentListItemResponse(
    Guid Id,
    Guid WorkOrderId,
    string OrderNumber,
    string CustomerDisplayName,
    string PatientName,
    DateOnly PaymentDate,
    decimal Amount,
    string Method,
    string MethodLabel,
    string? Reference,
    bool IsCancelled);

public sealed record PaymentSummaryResponse(
    Guid WorkOrderId,
    string OrderNumber,
    decimal? TotalAmount,
    decimal PaidAmount,
    decimal? Balance,
    string PaymentStatus,
    string PaymentStatusLabel,
    int ActivePaymentsCount,
    int CancelledPaymentsCount);

public sealed record PaymentMutationResponse(
    PaymentResponse Payment,
    PaymentSummaryResponse Summary);

public sealed record PaymentOptionResponse(string Value, string Label);
