using Microsoft.EntityFrameworkCore;
using LaboratorioTlahuac.Application.Abstractions.Security;
using LaboratorioTlahuac.Application.Abstractions.Time;
using LaboratorioTlahuac.Application.Payments;
using LaboratorioTlahuac.Domain.Payments;
using LaboratorioTlahuac.Domain.Payments.Entities;
using LaboratorioTlahuac.Domain.WorkOrders;
using LaboratorioTlahuac.Infrastructure.Persistence;

namespace LaboratorioTlahuac.Infrastructure.Payments;

public sealed class PaymentService(
    LaboratorioTlahuacDbContext dbContext,
    IClock clock,
    ICurrentUser currentUser)
    : IPaymentService
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private static readonly PaymentOptionResponse[] Methods =
    [
        new(nameof(PaymentMethod.Cash), "Efectivo"),
        new(nameof(PaymentMethod.BankTransfer), "Transferencia"),
        new(nameof(PaymentMethod.Card), "Tarjeta"),
        new(nameof(PaymentMethod.Other), "Otro")
    ];

    private static readonly PaymentOptionResponse[] Statuses =
    [
        new(nameof(PaymentStatus.TotalNotSet), "Total no definido"),
        new(nameof(PaymentStatus.Unpaid), "Sin pago"),
        new(nameof(PaymentStatus.Partial), "Pago parcial"),
        new(nameof(PaymentStatus.Paid), "Pagada"),
        new(nameof(PaymentStatus.Overpaid), "Saldo a favor / revisar")
    ];

    public async Task<PaymentServiceResult<IReadOnlyCollection<PaymentResponse>>> ListForWorkOrderAsync(
        Guid workOrderId,
        WorkOrderPaymentListQuery query,
        CancellationToken cancellationToken = default)
    {
        var workOrderExists = await dbContext.WorkOrders
            .AsNoTracking()
            .AnyAsync(workOrder => workOrder.Id == workOrderId, cancellationToken);

        if (!workOrderExists)
        {
            return PaymentServiceResult.NotFound<IReadOnlyCollection<PaymentResponse>>(
                "Work order was not found.");
        }

        var paymentsQuery = dbContext.Payments
            .AsNoTracking()
            .Where(payment => payment.WorkOrderId == workOrderId);

        if (query.IncludeCancelled != true)
        {
            paymentsQuery = paymentsQuery.Where(payment => !payment.IsCancelled);
        }

        var payments = await paymentsQuery
            .OrderByDescending(payment => payment.PaymentDate)
            .ThenByDescending(payment => payment.Id)
            .ToListAsync(cancellationToken);

        return PaymentServiceResult.Success<IReadOnlyCollection<PaymentResponse>>(
            payments.Select(MapPayment).ToArray());
    }

    public Task<PaymentServiceResult<PaymentSummaryResponse>> GetSummaryAsync(
        Guid workOrderId,
        CancellationToken cancellationToken = default)
    {
        return BuildSummaryResultAsync(workOrderId, cancellationToken);
    }

    public async Task<PaymentServiceResult<PaymentMutationResponse>> CreateAsync(
        Guid workOrderId,
        PaymentCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var input = ValidateCreate(request);

        if (input.Errors.Count > 0 || input.Value is null)
        {
            return PaymentServiceResult.Validation<PaymentMutationResponse>(input.Errors);
        }

        var order = await dbContext.WorkOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(workOrder => workOrder.Id == workOrderId, cancellationToken);

        if (order is null)
        {
            return PaymentServiceResult.NotFound<PaymentMutationResponse>("Work order was not found.");
        }

        if (!order.TotalAmount.HasValue)
        {
            return PaymentServiceResult.Conflict<PaymentMutationResponse>(
                "Work order total amount must be defined before registering payments.");
        }

        if (order.Status == WorkOrderStatus.Cancelled)
        {
            return PaymentServiceResult.Conflict<PaymentMutationResponse>(
                "Payments cannot be registered for cancelled work orders in the MVP.");
        }

        var payment = Payment.Create(
            workOrderId,
            input.Value.PaymentDate,
            input.Value.Amount,
            input.Value.Method,
            input.Value.Reference,
            input.Value.Notes,
            currentUser.UserId,
            clock.UtcNow);

        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync(cancellationToken);

        var summary = await BuildSummaryAsync(workOrderId, cancellationToken);

        return PaymentServiceResult.Success(new PaymentMutationResponse(MapPayment(payment), summary));
    }

    public async Task<PaymentServiceResult<PaymentMutationResponse>> CancelAsync(
        Guid workOrderId,
        Guid paymentId,
        PaymentCancelRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var reason = NormalizeRequired(errors, nameof(request.Reason), request.Reason);

        if (reason is not null)
        {
            ValidateMaxLength(errors, nameof(request.Reason), reason, Payment.CancellationReasonMaxLength);
        }

        if (errors.Count > 0 || reason is null)
        {
            return PaymentServiceResult.Validation<PaymentMutationResponse>(errors);
        }

        var workOrderExists = await dbContext.WorkOrders
            .AsNoTracking()
            .AnyAsync(workOrder => workOrder.Id == workOrderId, cancellationToken);

        if (!workOrderExists)
        {
            return PaymentServiceResult.NotFound<PaymentMutationResponse>("Work order was not found.");
        }

        var payment = await dbContext.Payments
            .FirstOrDefaultAsync(currentPayment => currentPayment.Id == paymentId, cancellationToken);

        if (payment is null)
        {
            return PaymentServiceResult.NotFound<PaymentMutationResponse>("Payment was not found.");
        }

        if (payment.WorkOrderId != workOrderId)
        {
            return PaymentServiceResult.Conflict<PaymentMutationResponse>(
                "Payment does not belong to the requested work order.");
        }

        if (payment.IsCancelled)
        {
            return PaymentServiceResult.Conflict<PaymentMutationResponse>(
                "Payment is already cancelled.");
        }

        payment.Cancel(reason, currentUser.UserId, clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        var summary = await BuildSummaryAsync(workOrderId, cancellationToken);

        return PaymentServiceResult.Success(new PaymentMutationResponse(MapPayment(payment), summary));
    }

    public async Task<PaymentServiceResult<PaymentPagedResponse<PaymentListItemResponse>>> ListAsync(
        PaymentListQuery query,
        CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var page = query.Page ?? DefaultPage;
        var pageSize = query.PageSize ?? DefaultPageSize;
        var method = default(PaymentMethod?);

        if (page < 1)
        {
            AddError(errors, nameof(query.Page), "Page must be greater than or equal to 1.");
        }

        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            AddError(errors, nameof(query.PageSize), "PageSize must be between 1 and 100.");
        }

        if (!string.IsNullOrWhiteSpace(query.Method))
        {
            if (!TryParseMethod(query.Method, out var parsedMethod))
            {
                AddError(errors, nameof(query.Method), "Method is invalid.");
            }
            else
            {
                method = parsedMethod;
            }
        }

        if (query.PaymentDateFrom.HasValue
            && query.PaymentDateTo.HasValue
            && query.PaymentDateTo.Value < query.PaymentDateFrom.Value)
        {
            AddError(errors, nameof(query.PaymentDateTo), "PaymentDateTo cannot be before PaymentDateFrom.");
        }

        if (errors.Count > 0)
        {
            return PaymentServiceResult.Validation<PaymentPagedResponse<PaymentListItemResponse>>(errors);
        }

        var paymentsQuery = dbContext.Payments
            .AsNoTracking()
            .AsQueryable();

        if (query.IncludeCancelled != true)
        {
            paymentsQuery = paymentsQuery.Where(payment => !payment.IsCancelled);
        }

        if (query.CustomerId.HasValue)
        {
            paymentsQuery = paymentsQuery.Where(payment =>
                payment.WorkOrder != null && payment.WorkOrder.CustomerId == query.CustomerId.Value);
        }

        if (query.WorkOrderId.HasValue)
        {
            paymentsQuery = paymentsQuery.Where(payment => payment.WorkOrderId == query.WorkOrderId.Value);
        }

        if (method.HasValue)
        {
            paymentsQuery = paymentsQuery.Where(payment => payment.Method == method.Value);
        }

        if (query.PaymentDateFrom.HasValue)
        {
            paymentsQuery = paymentsQuery.Where(payment => payment.PaymentDate >= query.PaymentDateFrom.Value);
        }

        if (query.PaymentDateTo.HasValue)
        {
            paymentsQuery = paymentsQuery.Where(payment => payment.PaymentDate <= query.PaymentDateTo.Value);
        }

        var search = NormalizeOptional(query.Search);

        if (search is not null)
        {
            var pattern = $"%{search}%";
            paymentsQuery = paymentsQuery.Where(payment =>
                payment.WorkOrder != null
                && (EF.Functions.Like(payment.WorkOrder.OrderNumber, pattern)
                    || EF.Functions.Like(payment.WorkOrder.PatientName, pattern)
                    || (payment.WorkOrder.ReferenceNumber != null
                        && EF.Functions.Like(payment.WorkOrder.ReferenceNumber, pattern))
                    || (payment.WorkOrder.Customer != null
                        && EF.Functions.Like(payment.WorkOrder.Customer.DisplayName, pattern))
                    || (payment.Reference != null && EF.Functions.Like(payment.Reference, pattern))
                    || (payment.Notes != null && EF.Functions.Like(payment.Notes, pattern))));
        }

        var totalCount = await paymentsQuery.CountAsync(cancellationToken);
        var items = await paymentsQuery
            .OrderByDescending(payment => payment.PaymentDate)
            .ThenByDescending(payment => payment.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(payment => new PaymentListProjection(
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
                payment.Reference,
                payment.IsCancelled))
            .ToListAsync(cancellationToken);

        return PaymentServiceResult.Success(
            new PaymentPagedResponse<PaymentListItemResponse>(
                items.Select(MapPaymentListItem).ToArray(),
                page,
                pageSize,
                totalCount));
    }

    public IReadOnlyCollection<PaymentOptionResponse> GetMethods()
    {
        return Methods;
    }

    public IReadOnlyCollection<PaymentOptionResponse> GetStatuses()
    {
        return Statuses;
    }

    private async Task<PaymentServiceResult<PaymentSummaryResponse>> BuildSummaryResultAsync(
        Guid workOrderId,
        CancellationToken cancellationToken)
    {
        var workOrderExists = await dbContext.WorkOrders
            .AsNoTracking()
            .AnyAsync(workOrder => workOrder.Id == workOrderId, cancellationToken);

        if (!workOrderExists)
        {
            return PaymentServiceResult.NotFound<PaymentSummaryResponse>("Work order was not found.");
        }

        return PaymentServiceResult.Success(await BuildSummaryAsync(workOrderId, cancellationToken));
    }

    private async Task<PaymentSummaryResponse> BuildSummaryAsync(
        Guid workOrderId,
        CancellationToken cancellationToken)
    {
        var order = await dbContext.WorkOrders
            .AsNoTracking()
            .Where(workOrder => workOrder.Id == workOrderId)
            .Select(workOrder => new
            {
                workOrder.Id,
                workOrder.OrderNumber,
                workOrder.TotalAmount
            })
            .FirstAsync(cancellationToken);

        var paymentRows = await dbContext.Payments
            .AsNoTracking()
            .Where(payment => payment.WorkOrderId == workOrderId)
            .Select(payment => new
            {
                payment.Amount,
                payment.IsCancelled
            })
            .ToListAsync(cancellationToken);

        var paidAmount = paymentRows
            .Where(payment => !payment.IsCancelled)
            .Sum(payment => payment.Amount);
        var activePaymentsCount = paymentRows.Count(payment => !payment.IsCancelled);
        var cancelledPaymentsCount = paymentRows.Count(payment => payment.IsCancelled);
        var balance = order.TotalAmount.HasValue ? order.TotalAmount.Value - paidAmount : (decimal?)null;
        var status = CalculateStatus(order.TotalAmount, paidAmount);

        return new PaymentSummaryResponse(
            order.Id,
            order.OrderNumber,
            order.TotalAmount,
            paidAmount,
            balance,
            status.ToString(),
            GetStatusLabel(status),
            activePaymentsCount,
            cancelledPaymentsCount);
    }

    private static ValidationResult<PaymentInput> ValidateCreate(PaymentCreateRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var reference = NormalizeAndValidateMax(
            errors,
            nameof(request.Reference),
            request.Reference,
            Payment.ReferenceMaxLength);
        var notes = NormalizeAndValidateMax(
            errors,
            nameof(request.Notes),
            request.Notes,
            Payment.NotesMaxLength);

        if (request.PaymentDate == default)
        {
            AddError(errors, nameof(request.PaymentDate), "PaymentDate is required.");
        }

        if (request.Amount <= 0)
        {
            AddError(errors, nameof(request.Amount), "Amount must be greater than 0.");
        }

        if (!TryParseMethod(request.Method, out var method))
        {
            AddError(errors, nameof(request.Method), "Method is invalid.");
        }

        return errors.Count > 0
            ? new ValidationResult<PaymentInput>(errors, null)
            : new ValidationResult<PaymentInput>(
                errors,
                new PaymentInput(request.PaymentDate, request.Amount, method, reference, notes));
    }

    private static bool TryParseMethod(string? value, out PaymentMethod method)
    {
        if (string.IsNullOrWhiteSpace(value)
            || !Enum.TryParse(value.Trim(), ignoreCase: true, out method)
            || !Enum.IsDefined(method))
        {
            method = default;
            return false;
        }

        return true;
    }

    private static PaymentStatus CalculateStatus(decimal? totalAmount, decimal paidAmount)
    {
        if (!totalAmount.HasValue)
        {
            return PaymentStatus.TotalNotSet;
        }

        if (paidAmount <= 0)
        {
            return totalAmount.Value == 0 ? PaymentStatus.Paid : PaymentStatus.Unpaid;
        }

        if (paidAmount < totalAmount.Value)
        {
            return PaymentStatus.Partial;
        }

        return paidAmount == totalAmount.Value ? PaymentStatus.Paid : PaymentStatus.Overpaid;
    }

    private static PaymentResponse MapPayment(Payment payment)
    {
        return new PaymentResponse(
            payment.Id,
            payment.WorkOrderId,
            payment.PaymentDate,
            payment.Amount,
            payment.Method.ToString(),
            GetMethodLabel(payment.Method),
            payment.Reference,
            payment.Notes,
            payment.IsCancelled,
            payment.CreatedAtUtc,
            payment.CancelledAtUtc,
            payment.CancellationReason);
    }

    private static PaymentListItemResponse MapPaymentListItem(PaymentListProjection payment)
    {
        return new PaymentListItemResponse(
            payment.Id,
            payment.WorkOrderId,
            payment.OrderNumber,
            payment.CustomerDisplayName,
            payment.PatientName,
            payment.PaymentDate,
            payment.Amount,
            payment.Method.ToString(),
            GetMethodLabel(payment.Method),
            payment.Reference,
            payment.IsCancelled);
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

    private static string GetStatusLabel(PaymentStatus status)
    {
        return status switch
        {
            PaymentStatus.TotalNotSet => "Total no definido",
            PaymentStatus.Unpaid => "Sin pago",
            PaymentStatus.Partial => "Pago parcial",
            PaymentStatus.Paid => "Pagada",
            PaymentStatus.Overpaid => "Saldo a favor / revisar",
            _ => status.ToString()
        };
    }

    private static string? NormalizeRequired(
        IDictionary<string, string[]> errors,
        string fieldName,
        string? value)
    {
        var normalized = NormalizeOptional(value);

        if (normalized is null)
        {
            AddError(errors, fieldName, $"{fieldName} is required.");
        }

        return normalized;
    }

    private static string? NormalizeAndValidateMax(
        IDictionary<string, string[]> errors,
        string fieldName,
        string? value,
        int maxLength)
    {
        var normalized = NormalizeOptional(value);

        if (normalized is not null)
        {
            ValidateMaxLength(errors, fieldName, normalized, maxLength);
        }

        return normalized;
    }

    private static void ValidateMaxLength(
        IDictionary<string, string[]> errors,
        string fieldName,
        string value,
        int maxLength)
    {
        if (value.Length > maxLength)
        {
            AddError(errors, fieldName, $"{fieldName} must be {maxLength} characters or fewer.");
        }
    }

    private static void AddError(IDictionary<string, string[]> errors, string fieldName, string error)
    {
        errors[fieldName] = errors.TryGetValue(fieldName, out var existingErrors)
            ? [.. existingErrors, error]
            : [error];
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private sealed record PaymentInput(
        DateOnly PaymentDate,
        decimal Amount,
        PaymentMethod Method,
        string? Reference,
        string? Notes);

    private sealed record PaymentListProjection(
        Guid Id,
        Guid WorkOrderId,
        string OrderNumber,
        string CustomerDisplayName,
        string PatientName,
        DateOnly PaymentDate,
        decimal Amount,
        PaymentMethod Method,
        string? Reference,
        bool IsCancelled);

    private sealed record ValidationResult<T>(
        IReadOnlyDictionary<string, string[]> Errors,
        T? Value);
}
