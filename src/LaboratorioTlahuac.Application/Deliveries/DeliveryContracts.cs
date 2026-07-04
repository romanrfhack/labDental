namespace LaboratorioTlahuac.Application.Deliveries;

public sealed record DeliveryListQuery(
    string? Status,
    bool? AssignedToMe,
    int? Page,
    int? PageSize);

public sealed record DeliveryCreateRequest(string? DeliveryNotes);

public sealed record DeliveryAssignRequest(Guid? AssignedToUserId, string? DeliveryNotes);

public sealed record DeliveryOutForDeliveryRequest(string? DeliveryNotes);

public sealed record DeliveryCompleteRequest(string? RecipientName, string? DeliveryNotes);

public sealed record DeliveryFailedRequest(string? FailedReason, string? DeliveryNotes);

public sealed record DeliveryPagedResponse<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record DeliveryResponse(
    Guid Id,
    Guid WorkOrderId,
    string OrderNumber,
    Guid CustomerId,
    string CustomerDisplayName,
    string? CustomerAddress,
    string? CustomerContactName,
    string? CustomerPhone,
    string? CustomerWhatsApp,
    Guid? InternalDoctorId,
    string? InternalDoctorFullName,
    string PatientName,
    string? ReferenceNumber,
    string WorkSummary,
    DateOnly? DeliveryDate,
    string WorkOrderStatus,
    string WorkOrderStatusLabel,
    string Status,
    string StatusLabel,
    Guid? AssignedToUserId,
    string? AssignedToUserFullName,
    string? RecipientName,
    string? DeliveryNotes,
    string? FailedReason,
    DateTimeOffset? AssignedAtUtc,
    DateTimeOffset? OutForDeliveryAtUtc,
    DateTimeOffset? DeliveredAtUtc,
    DateTimeOffset? FailedAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
