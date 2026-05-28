using Microsoft.EntityFrameworkCore;
using LaboratorioTlahuac.Application.Abstractions.Security;
using LaboratorioTlahuac.Application.Abstractions.Time;
using LaboratorioTlahuac.Application.Dashboard;
using LaboratorioTlahuac.Domain.Customers;
using LaboratorioTlahuac.Domain.Payments;
using LaboratorioTlahuac.Domain.Security;
using LaboratorioTlahuac.Domain.WorkOrders;
using LaboratorioTlahuac.Infrastructure.Persistence;

namespace LaboratorioTlahuac.Infrastructure.Dashboard;

public sealed class DashboardService(
    LaboratorioTlahuacDbContext dbContext,
    IClock clock,
    ICurrentUser currentUser,
    DashboardOptions dashboardOptions)
    : IDashboardService
{
    private const int ShortListSize = 5;

    private static readonly WorkOrderStatus[] WorkOrderStatuses = Enum.GetValues<WorkOrderStatus>();
    private readonly TimeZoneInfo businessTimeZone =
        DashboardTimeZoneResolver.Resolve(dashboardOptions.BusinessTimeZone);

    public async Task<DashboardSummaryResponse> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var generatedAtUtc = clock.UtcNow;
        var permissions = currentUser.Permissions.ToHashSet(StringComparer.Ordinal);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(generatedAtUtc, businessTimeZone).Date);

        var customerSummary = permissions.Contains(Permissions.CustomersView)
            ? await BuildCustomerSummaryAsync(cancellationToken)
            : null;
        var operationalSummary = permissions.Contains(Permissions.OrdersView)
            ? await BuildOperationalSummaryAsync(today, cancellationToken)
            : null;
        var financialSummary = permissions.Contains(Permissions.PaymentsView)
            ? await BuildFinancialSummaryAsync(cancellationToken)
            : null;

        return new DashboardSummaryResponse(
            generatedAtUtc,
            customerSummary,
            operationalSummary,
            financialSummary);
    }

    private async Task<CustomerSummaryResponse> BuildCustomerSummaryAsync(
        CancellationToken cancellationToken)
    {
        var aggregate = await dbContext.Customers
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(group => new
            {
                ActiveCustomersCount = group.Count(customer => customer.IsActive),
                ActiveDoctorsCount = group.Count(customer =>
                    customer.IsActive && customer.Type == CustomerType.Doctor),
                ActiveClinicsCount = group.Count(customer =>
                    customer.IsActive && customer.Type == CustomerType.Clinic),
                InactiveCustomersCount = group.Count(customer => !customer.IsActive)
            })
            .SingleOrDefaultAsync(cancellationToken);

        return aggregate is null
            ? new CustomerSummaryResponse(0, 0, 0, 0)
            : new CustomerSummaryResponse(
                aggregate.ActiveCustomersCount,
                aggregate.ActiveDoctorsCount,
                aggregate.ActiveClinicsCount,
                aggregate.InactiveCustomersCount);
    }

    private async Task<OperationalSummaryResponse> BuildOperationalSummaryAsync(
        DateOnly today,
        CancellationToken cancellationToken)
    {
        var upcomingEnd = today.AddDays(7);
        var aggregate = await dbContext.WorkOrders
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(group => new
            {
                ActiveWorkOrdersCount = group.Count(order =>
                    order.Status != WorkOrderStatus.Cancelled
                    && order.Status != WorkOrderStatus.Delivered),
                DeliveredWorkOrdersCount = group.Count(order => order.Status == WorkOrderStatus.Delivered),
                CancelledWorkOrdersCount = group.Count(order => order.Status == WorkOrderStatus.Cancelled),
                DueTodayCount = group.Count(order =>
                    order.DeliveryDate == today
                    && order.Status != WorkOrderStatus.Delivered
                    && order.Status != WorkOrderStatus.Cancelled),
                OverdueCount = group.Count(order =>
                    order.DeliveryDate < today
                    && order.Status != WorkOrderStatus.Delivered
                    && order.Status != WorkOrderStatus.Cancelled),
                UpcomingDueCount = group.Count(order =>
                    order.DeliveryDate >= today
                    && order.DeliveryDate <= upcomingEnd
                    && order.Status != WorkOrderStatus.Delivered
                    && order.Status != WorkOrderStatus.Cancelled)
            })
            .SingleOrDefaultAsync(cancellationToken);

        var statusCounts = await dbContext.WorkOrders
            .AsNoTracking()
            .GroupBy(order => order.Status)
            .Select(group => new StatusCountRow(group.Key, group.Count()))
            .ToListAsync(cancellationToken);
        var countsByStatus = statusCounts.ToDictionary(row => row.Status, row => row.Count);
        var byStatus = WorkOrderStatuses
            .Select(status => new WorkOrderStatusSummaryResponse(
                status.ToString(),
                GetStatusLabel(status),
                countsByStatus.GetValueOrDefault(status)))
            .ToArray();

        var latestWorkOrders = await dbContext.WorkOrders
            .AsNoTracking()
            .Select(order => new WorkOrderDashboardRow(
                order.Id,
                order.OrderNumber,
                order.Customer != null ? order.Customer.DisplayName : string.Empty,
                order.PatientName,
                order.Status,
                order.DeliveryDate,
                order.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        var dueSoonWorkOrders = await dbContext.WorkOrders
            .AsNoTracking()
            .Where(order =>
                order.DeliveryDate >= today
                && order.Status != WorkOrderStatus.Delivered
                && order.Status != WorkOrderStatus.Cancelled)
            .Select(order => new WorkOrderDashboardRow(
                order.Id,
                order.OrderNumber,
                order.Customer != null ? order.Customer.DisplayName : string.Empty,
                order.PatientName,
                order.Status,
                order.DeliveryDate,
                order.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return new OperationalSummaryResponse(
            aggregate?.ActiveWorkOrdersCount ?? 0,
            aggregate?.DeliveredWorkOrdersCount ?? 0,
            aggregate?.CancelledWorkOrdersCount ?? 0,
            aggregate?.DueTodayCount ?? 0,
            aggregate?.OverdueCount ?? 0,
            aggregate?.UpcomingDueCount ?? 0,
            byStatus,
            latestWorkOrders
                .OrderByDescending(order => order.CreatedAtUtc)
                .ThenByDescending(order => order.Id)
                .Take(ShortListSize)
                .Select(MapWorkOrder)
                .ToArray(),
            dueSoonWorkOrders
                .OrderBy(order => order.DeliveryDate)
                .ThenByDescending(order => order.CreatedAtUtc)
                .Take(ShortListSize)
                .Select(MapWorkOrder)
                .ToArray());
    }

    private async Task<FinancialSummaryResponse> BuildFinancialSummaryAsync(
        CancellationToken cancellationToken)
    {
        var orderRows = await dbContext.WorkOrders
            .AsNoTracking()
            .Where(order => order.TotalAmount.HasValue)
            .Select(order => new FinancialOrderRow(
                order.Id,
                order.Status,
                order.TotalAmount!.Value))
            .ToListAsync(cancellationToken);
        var paidRows = await dbContext.Payments
            .AsNoTracking()
            .Where(payment =>
                !payment.IsCancelled
                && payment.WorkOrder != null
                && payment.WorkOrder.TotalAmount.HasValue)
            .Select(payment => new PaymentAmountRow(payment.WorkOrderId, payment.Amount))
            .ToListAsync(cancellationToken);
        var paidByOrder = paidRows
            .GroupBy(payment => payment.WorkOrderId)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(payment => payment.Amount));

        var totalReceivable = 0m;
        var ordersWithPendingBalanceCount = 0;
        var paidOrdersCount = 0;
        var partialPaymentOrdersCount = 0;
        var unpaidOrdersCount = 0;
        var overpaidOrdersCount = 0;

        foreach (var order in orderRows)
        {
            if (order.Status == WorkOrderStatus.Cancelled)
            {
                continue;
            }

            var paidAmount = paidByOrder.GetValueOrDefault(order.Id);
            var balance = order.TotalAmount - paidAmount;

            if (balance > 0)
            {
                ordersWithPendingBalanceCount++;
                totalReceivable += balance;
            }

            switch (CalculatePaymentStatus(order.TotalAmount, paidAmount))
            {
                case PaymentStatus.Unpaid:
                    unpaidOrdersCount++;
                    break;
                case PaymentStatus.Partial:
                    partialPaymentOrdersCount++;
                    break;
                case PaymentStatus.Paid:
                    paidOrdersCount++;
                    break;
                case PaymentStatus.Overpaid:
                    overpaidOrdersCount++;
                    break;
            }
        }

        var cancelledPaymentsCount = await dbContext.Payments
            .AsNoTracking()
            .CountAsync(payment => payment.IsCancelled, cancellationToken);
        var latestPayments = await dbContext.Payments
            .AsNoTracking()
            .Where(payment => !payment.IsCancelled)
            .Select(payment => new PaymentDashboardRow(
                payment.Id,
                payment.WorkOrderId,
                payment.WorkOrder != null ? payment.WorkOrder.OrderNumber : string.Empty,
                payment.WorkOrder != null && payment.WorkOrder.Customer != null
                    ? payment.WorkOrder.Customer.DisplayName
                    : string.Empty,
                payment.WorkOrder != null ? payment.WorkOrder.PatientName : string.Empty,
                payment.PaymentDate,
                payment.Amount,
                payment.Method,
                payment.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return new FinancialSummaryResponse(
            totalReceivable,
            ordersWithPendingBalanceCount,
            paidOrdersCount,
            partialPaymentOrdersCount,
            unpaidOrdersCount,
            overpaidOrdersCount,
            cancelledPaymentsCount,
            latestPayments
                .OrderByDescending(payment => payment.CreatedAtUtc)
                .ThenByDescending(payment => payment.Id)
                .Take(ShortListSize)
                .Select(MapPayment)
                .ToArray());
    }

    private static PaymentStatus CalculatePaymentStatus(decimal totalAmount, decimal paidAmount)
    {
        if (paidAmount <= 0)
        {
            return totalAmount == 0 ? PaymentStatus.Paid : PaymentStatus.Unpaid;
        }

        if (paidAmount < totalAmount)
        {
            return PaymentStatus.Partial;
        }

        return paidAmount == totalAmount ? PaymentStatus.Paid : PaymentStatus.Overpaid;
    }

    private static DashboardWorkOrderResponse MapWorkOrder(WorkOrderDashboardRow order)
    {
        return new DashboardWorkOrderResponse(
            order.Id,
            order.OrderNumber,
            order.CustomerDisplayName,
            order.PatientName,
            order.Status.ToString(),
            GetStatusLabel(order.Status),
            order.DeliveryDate);
    }

    private static DashboardPaymentResponse MapPayment(PaymentDashboardRow payment)
    {
        return new DashboardPaymentResponse(
            payment.Id,
            payment.WorkOrderId,
            payment.OrderNumber,
            payment.CustomerDisplayName,
            payment.PatientName,
            payment.PaymentDate,
            payment.Amount,
            payment.Method.ToString(),
            GetMethodLabel(payment.Method));
    }

    private static string GetStatusLabel(WorkOrderStatus status)
    {
        return status switch
        {
            WorkOrderStatus.Received => "Recibida",
            WorkOrderStatus.InProcess => "En proceso",
            WorkOrderStatus.FirstTrial => "En primera prueba",
            WorkOrderStatus.SecondTrial => "En segunda prueba",
            WorkOrderStatus.ReadyForDelivery => "Lista para entrega",
            WorkOrderStatus.Delivered => "Entregada",
            WorkOrderStatus.Cancelled => "Cancelada",
            _ => status.ToString()
        };
    }

    private static string GetMethodLabel(PaymentMethod method)
    {
        return method switch
        {
            PaymentMethod.Cash => "Efectivo",
            PaymentMethod.BankTransfer => "Transferencia",
            PaymentMethod.Card => "Tarjeta",
            PaymentMethod.Other => "Otro",
            _ => method.ToString()
        };
    }

    private sealed record StatusCountRow(WorkOrderStatus Status, int Count);

    private sealed record WorkOrderDashboardRow(
        Guid Id,
        string OrderNumber,
        string CustomerDisplayName,
        string PatientName,
        WorkOrderStatus Status,
        DateOnly? DeliveryDate,
        DateTimeOffset CreatedAtUtc);

    private sealed record FinancialOrderRow(
        Guid Id,
        WorkOrderStatus Status,
        decimal TotalAmount);

    private sealed record PaymentAmountRow(
        Guid WorkOrderId,
        decimal Amount);

    private sealed record PaymentDashboardRow(
        Guid Id,
        Guid WorkOrderId,
        string OrderNumber,
        string CustomerDisplayName,
        string PatientName,
        DateOnly PaymentDate,
        decimal Amount,
        PaymentMethod Method,
        DateTimeOffset CreatedAtUtc);
}
