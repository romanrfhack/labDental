namespace LaboratorioTlahuac.Application.WorkOrders;

public sealed record WorkOrderListQuery(
    string? Search,
    Guid? CustomerId,
    Guid? InternalDoctorId,
    string? Status,
    DateOnly? ReceivedDateFrom,
    DateOnly? ReceivedDateTo,
    DateOnly? DeliveryDateFrom,
    DateOnly? DeliveryDateTo,
    bool? IncludeCancelled,
    int? Page,
    int? PageSize);

public sealed record WorkOrderUpsertRequest(
    Guid CustomerId,
    Guid? InternalDoctorId,
    string? PatientName,
    DateOnly ReceivedDate,
    string? ReferenceNumber,
    string? WorkDescription,
    string? DentalColor,
    DateOnly? FirstTrialDate,
    DateOnly? SecondTrialDate,
    DateOnly? DeliveryDate,
    decimal? TotalAmount,
    string? Notes);

public sealed record WorkOrderChangeStatusRequest(
    string? Status,
    string? Notes);

public sealed record WorkOrderPagedResponse<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record WorkOrderListItemResponse(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    string CustomerDisplayName,
    Guid? InternalDoctorId,
    string? InternalDoctorFullName,
    string PatientName,
    string WorkDescription,
    string? DentalColor,
    DateOnly ReceivedDate,
    DateOnly? DeliveryDate,
    string Status,
    string StatusLabel,
    decimal? TotalAmount,
    bool IsCancelled,
    WorkOrderDeliverySummaryResponse? Delivery);

public sealed record WorkOrderDeliverySummaryResponse(
    Guid DeliveryId,
    string DeliveryStatus,
    string DeliveryStatusLabel,
    string? AssignedToUserName,
    DateTimeOffset? DeliveredAtUtc,
    DateTimeOffset? FailedAtUtc);

public sealed record WorkOrderDetailResponse(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    string CustomerDisplayName,
    string CustomerType,
    Guid? InternalDoctorId,
    string? InternalDoctorFullName,
    string PatientName,
    DateOnly ReceivedDate,
    string? ReferenceNumber,
    string WorkDescription,
    string? DentalColor,
    DateOnly? FirstTrialDate,
    DateOnly? SecondTrialDate,
    DateOnly? DeliveryDate,
    string Status,
    string StatusLabel,
    decimal? TotalAmount,
    string? Notes,
    bool IsCancelled,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyCollection<WorkOrderStatusHistoryResponse> StatusHistory);

public sealed record WorkOrderStatusHistoryResponse(
    Guid Id,
    string? FromStatus,
    string? FromStatusLabel,
    string ToStatus,
    string ToStatusLabel,
    string? Notes,
    DateTimeOffset ChangedAtUtc);

public sealed record WorkOrderStatusResponse(
    string Value,
    string Label);
