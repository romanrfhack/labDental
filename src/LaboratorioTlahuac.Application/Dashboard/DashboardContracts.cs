namespace LaboratorioTlahuac.Application.Dashboard;

public sealed record DashboardSummaryResponse(
    DateTimeOffset GeneratedAtUtc,
    CustomerSummaryResponse? CustomerSummary,
    OperationalSummaryResponse? OperationalSummary,
    FinancialSummaryResponse? FinancialSummary);

public sealed record CustomerSummaryResponse(
    int ActiveCustomersCount,
    int ActiveDoctorsCount,
    int ActiveClinicsCount,
    int InactiveCustomersCount);

public sealed record OperationalSummaryResponse(
    int ActiveWorkOrdersCount,
    int DeliveredWorkOrdersCount,
    int CancelledWorkOrdersCount,
    int DueTodayCount,
    int OverdueCount,
    int UpcomingDueCount,
    IReadOnlyCollection<WorkOrderStatusSummaryResponse> ByStatus,
    IReadOnlyCollection<DashboardWorkOrderResponse> LatestWorkOrders,
    IReadOnlyCollection<DashboardWorkOrderResponse> DueSoonWorkOrders);

public sealed record WorkOrderStatusSummaryResponse(
    string Status,
    string Label,
    int Count);

public sealed record DashboardWorkOrderResponse(
    Guid Id,
    string OrderNumber,
    string CustomerDisplayName,
    string PatientName,
    string Status,
    string StatusLabel,
    DateOnly? DeliveryDate);

public sealed record FinancialSummaryResponse(
    decimal TotalReceivable,
    int OrdersWithPendingBalanceCount,
    int PaidOrdersCount,
    int PartialPaymentOrdersCount,
    int UnpaidOrdersCount,
    int OverpaidOrdersCount,
    int CancelledPaymentsCount,
    IReadOnlyCollection<DashboardPaymentResponse> LatestPayments);

public sealed record DashboardPaymentResponse(
    Guid Id,
    Guid WorkOrderId,
    string OrderNumber,
    string CustomerDisplayName,
    string PatientName,
    DateOnly PaymentDate,
    decimal Amount,
    string Method,
    string MethodLabel);
