using Microsoft.EntityFrameworkCore;
using LaboratorioTlahuac.Application.Abstractions.Security;
using LaboratorioTlahuac.Application.Abstractions.Time;
using LaboratorioTlahuac.Application.WorkOrders;
using LaboratorioTlahuac.Domain.Customers;
using LaboratorioTlahuac.Domain.Customers.Entities;
using LaboratorioTlahuac.Domain.Deliveries;
using LaboratorioTlahuac.Domain.WorkOrders;
using LaboratorioTlahuac.Domain.WorkOrders.Entities;
using LaboratorioTlahuac.Infrastructure.Persistence;

namespace LaboratorioTlahuac.Infrastructure.WorkOrders;

public sealed class WorkOrderService(
    LaboratorioTlahuacDbContext dbContext,
    IClock clock,
    ICurrentUser currentUser,
    IWorkOrderNumberGenerator orderNumberGenerator)
    : IWorkOrderService
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;
    private const int MaxOrderNumberAttempts = 5;

    private static readonly WorkOrderStatusResponse[] Statuses =
    [
        new(nameof(WorkOrderStatus.Received), "Recibida"),
        new(nameof(WorkOrderStatus.InProcess), "En proceso"),
        new(nameof(WorkOrderStatus.FirstTrial), "En primera prueba"),
        new(nameof(WorkOrderStatus.SecondTrial), "En segunda prueba"),
        new(nameof(WorkOrderStatus.ReadyForDelivery), "Lista para entrega"),
        new(nameof(WorkOrderStatus.Delivered), "Entregada"),
        new(nameof(WorkOrderStatus.Cancelled), "Cancelada")
    ];

    public async Task<WorkOrderServiceResult<WorkOrderPagedResponse<WorkOrderListItemResponse>>> ListAsync(
        WorkOrderListQuery query,
        CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var page = query.Page ?? DefaultPage;
        var pageSize = query.PageSize ?? DefaultPageSize;
        var status = default(WorkOrderStatus?);

        if (page < 1)
        {
            AddError(errors, nameof(query.Page), "Page must be greater than or equal to 1.");
        }

        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            AddError(errors, nameof(query.PageSize), "PageSize must be between 1 and 100.");
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (!TryParseStatus(query.Status, out var parsedStatus))
            {
                AddError(errors, nameof(query.Status), "Status is invalid.");
            }
            else
            {
                status = parsedStatus;
            }
        }

        if (query.ReceivedDateFrom.HasValue
            && query.ReceivedDateTo.HasValue
            && query.ReceivedDateTo.Value < query.ReceivedDateFrom.Value)
        {
            AddError(errors, nameof(query.ReceivedDateTo), "ReceivedDateTo cannot be before ReceivedDateFrom.");
        }

        if (query.DeliveryDateFrom.HasValue
            && query.DeliveryDateTo.HasValue
            && query.DeliveryDateTo.Value < query.DeliveryDateFrom.Value)
        {
            AddError(errors, nameof(query.DeliveryDateTo), "DeliveryDateTo cannot be before DeliveryDateFrom.");
        }

        if (errors.Count > 0)
        {
            return WorkOrderServiceResult.Validation<WorkOrderPagedResponse<WorkOrderListItemResponse>>(errors);
        }

        var ordersQuery = dbContext.WorkOrders
            .AsNoTracking()
            .Include(order => order.Customer)
            .Include(order => order.InternalDoctor)
            .AsQueryable();

        if (query.IncludeCancelled != true)
        {
            ordersQuery = ordersQuery.Where(order => order.Status != WorkOrderStatus.Cancelled);
        }

        if (query.CustomerId.HasValue)
        {
            ordersQuery = ordersQuery.Where(order => order.CustomerId == query.CustomerId.Value);
        }

        if (query.InternalDoctorId.HasValue)
        {
            ordersQuery = ordersQuery.Where(order => order.InternalDoctorId == query.InternalDoctorId.Value);
        }

        if (status.HasValue)
        {
            ordersQuery = ordersQuery.Where(order => order.Status == status.Value);
        }

        if (query.ReceivedDateFrom.HasValue)
        {
            ordersQuery = ordersQuery.Where(order => order.ReceivedDate >= query.ReceivedDateFrom.Value);
        }

        if (query.ReceivedDateTo.HasValue)
        {
            ordersQuery = ordersQuery.Where(order => order.ReceivedDate <= query.ReceivedDateTo.Value);
        }

        if (query.DeliveryDateFrom.HasValue)
        {
            ordersQuery = ordersQuery.Where(order =>
                order.DeliveryDate.HasValue && order.DeliveryDate.Value >= query.DeliveryDateFrom.Value);
        }

        if (query.DeliveryDateTo.HasValue)
        {
            ordersQuery = ordersQuery.Where(order =>
                order.DeliveryDate.HasValue && order.DeliveryDate.Value <= query.DeliveryDateTo.Value);
        }

        var search = NormalizeOptional(query.Search);

        if (search is not null)
        {
            var pattern = $"%{search}%";
            ordersQuery = ordersQuery.Where(order =>
                EF.Functions.Like(order.OrderNumber, pattern)
                || (order.ReferenceNumber != null && EF.Functions.Like(order.ReferenceNumber, pattern))
                || EF.Functions.Like(order.PatientName, pattern)
                || EF.Functions.Like(order.WorkDescription, pattern)
                || (order.Customer != null && EF.Functions.Like(order.Customer.DisplayName, pattern))
                || (order.InternalDoctor != null && EF.Functions.Like(order.InternalDoctor.FullName, pattern)));
        }

        var totalCount = await ordersQuery.CountAsync(cancellationToken);
        var rows = await ordersQuery
            .OrderBy(order => order.DeliveryDate == null)
            .ThenBy(order => order.DeliveryDate)
            .ThenByDescending(order => order.ReceivedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(order => new WorkOrderListItemProjection(
                order.Id,
                order.OrderNumber,
                order.CustomerId,
                order.Customer != null ? order.Customer.DisplayName : string.Empty,
                order.InternalDoctorId,
                order.InternalDoctor != null ? order.InternalDoctor.FullName : null,
                order.PatientName,
                order.WorkDescription,
                order.DentalColor,
                order.ReceivedDate,
                order.DeliveryDate,
                order.Status.ToString(),
                GetStatusLabel(order.Status),
                order.TotalAmount,
                order.Status == WorkOrderStatus.Cancelled,
                dbContext.WorkOrderDeliveries
                    .Where(delivery => delivery.WorkOrderId == order.Id)
                    .Select(delivery => new WorkOrderDeliverySummaryProjection(
                        delivery.Id,
                        delivery.Status,
                        delivery.AssignedToUser != null ? delivery.AssignedToUser.FullName : null,
                        delivery.DeliveredAtUtc,
                        delivery.FailedAtUtc))
                    .FirstOrDefault()))
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new WorkOrderListItemResponse(
                row.Id,
                row.OrderNumber,
                row.CustomerId,
                row.CustomerDisplayName,
                row.InternalDoctorId,
                row.InternalDoctorFullName,
                row.PatientName,
                row.WorkDescription,
                row.DentalColor,
                row.ReceivedDate,
                row.DeliveryDate,
                row.Status,
                row.StatusLabel,
                row.TotalAmount,
                row.IsCancelled,
                row.Delivery is null
                    ? null
                    : new WorkOrderDeliverySummaryResponse(
                        row.Delivery.DeliveryId,
                        row.Delivery.DeliveryStatus.ToString(),
                        GetDeliveryStatusLabel(row.Delivery.DeliveryStatus),
                        row.Delivery.AssignedToUserName,
                        row.Delivery.DeliveredAtUtc,
                        row.Delivery.FailedAtUtc)))
            .ToArray();

        return WorkOrderServiceResult.Success(
            new WorkOrderPagedResponse<WorkOrderListItemResponse>(items, page, pageSize, totalCount));
    }

    public async Task<WorkOrderServiceResult<WorkOrderDetailResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderDetailQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(currentOrder => currentOrder.Id == id, cancellationToken);

        return order is null
            ? WorkOrderServiceResult.NotFound<WorkOrderDetailResponse>("Work order was not found.")
            : WorkOrderServiceResult.Success(MapDetail(order));
    }

    public async Task<WorkOrderServiceResult<WorkOrderDetailResponse>> CreateAsync(
        WorkOrderUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        var input = await ValidateUpsertAsync(
            request,
            existingOrder: null,
            isCreate: true,
            cancellationToken);

        if (input.Errors.Count > 0 || input.Value is null)
        {
            return WorkOrderServiceResult.Validation<WorkOrderDetailResponse>(input.Errors);
        }

        if (input.Value.BusinessConflict is not null)
        {
            return WorkOrderServiceResult.Conflict<WorkOrderDetailResponse>(input.Value.BusinessConflict);
        }

        var now = clock.UtcNow;
        var orderNumber = await GenerateUniqueOrderNumberAsync(now, cancellationToken);
        var order = WorkOrder.Create(
            orderNumber,
            input.Value.CustomerId,
            input.Value.InternalDoctorId,
            input.Value.PatientName,
            input.Value.ReceivedDate,
            input.Value.ReferenceNumber,
            input.Value.WorkDescription,
            input.Value.DentalColor,
            input.Value.FirstTrialDate,
            input.Value.SecondTrialDate,
            input.Value.DeliveryDate,
            input.Value.TotalAmount,
            input.Value.Notes,
            currentUser.UserId,
            now);

        dbContext.WorkOrders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);

        var created = await LoadOrderDetailQuery()
            .AsNoTracking()
            .FirstAsync(currentOrder => currentOrder.Id == order.Id, cancellationToken);

        return WorkOrderServiceResult.Success(MapDetail(created));
    }

    public async Task<WorkOrderServiceResult<WorkOrderDetailResponse>> UpdateAsync(
        Guid id,
        WorkOrderUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await dbContext.WorkOrders
            .FirstOrDefaultAsync(currentOrder => currentOrder.Id == id, cancellationToken);

        if (order is null)
        {
            return WorkOrderServiceResult.NotFound<WorkOrderDetailResponse>("Work order was not found.");
        }

        if (order.Status == WorkOrderStatus.Cancelled)
        {
            return WorkOrderServiceResult.Conflict<WorkOrderDetailResponse>(
                "Cancelled work orders cannot be edited in the MVP.");
        }

        var input = await ValidateUpsertAsync(
            request,
            order,
            isCreate: false,
            cancellationToken);

        if (input.Errors.Count > 0 || input.Value is null)
        {
            return WorkOrderServiceResult.Validation<WorkOrderDetailResponse>(input.Errors);
        }

        if (input.Value.BusinessConflict is not null)
        {
            return WorkOrderServiceResult.Conflict<WorkOrderDetailResponse>(input.Value.BusinessConflict);
        }

        order.UpdateGeneral(
            input.Value.CustomerId,
            input.Value.InternalDoctorId,
            input.Value.PatientName,
            input.Value.ReceivedDate,
            input.Value.ReferenceNumber,
            input.Value.WorkDescription,
            input.Value.DentalColor,
            input.Value.FirstTrialDate,
            input.Value.SecondTrialDate,
            input.Value.DeliveryDate,
            input.Value.TotalAmount,
            input.Value.Notes,
            currentUser.UserId,
            clock.UtcNow);

        await dbContext.SaveChangesAsync(cancellationToken);

        var updated = await LoadOrderDetailQuery()
            .AsNoTracking()
            .FirstAsync(currentOrder => currentOrder.Id == order.Id, cancellationToken);

        return WorkOrderServiceResult.Success(MapDetail(updated));
    }

    public async Task<WorkOrderServiceResult<WorkOrderDetailResponse>> ChangeStatusAsync(
        Guid id,
        WorkOrderChangeStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

        if (!TryParseStatus(request.Status, out var status))
        {
            AddError(errors, nameof(request.Status), "Status is invalid.");
        }

        var notes = NormalizeAndValidateMax(
            errors,
            nameof(request.Notes),
            request.Notes,
            WorkOrderStatusHistory.NotesMaxLength);

        if (errors.Count == 0 && status == WorkOrderStatus.Cancelled && notes is null)
        {
            AddError(errors, nameof(request.Notes), "Notes are required when cancelling a work order.");
        }

        if (errors.Count > 0)
        {
            return WorkOrderServiceResult.Validation<WorkOrderDetailResponse>(errors);
        }

        var order = await dbContext.WorkOrders
            .FirstOrDefaultAsync(currentOrder => currentOrder.Id == id, cancellationToken);

        if (order is null)
        {
            return WorkOrderServiceResult.NotFound<WorkOrderDetailResponse>("Work order was not found.");
        }

        if (order.Status == WorkOrderStatus.Cancelled && status != WorkOrderStatus.Cancelled)
        {
            return WorkOrderServiceResult.Conflict<WorkOrderDetailResponse>(
                "Cancelled work orders cannot change to another status in the MVP.");
        }

        var history = order.ChangeStatus(status, notes, currentUser.UserId, clock.UtcNow);

        if (history is not null)
        {
            dbContext.WorkOrderStatusHistory.Add(history);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var updated = await LoadOrderDetailQuery()
            .AsNoTracking()
            .FirstAsync(currentOrder => currentOrder.Id == order.Id, cancellationToken);

        return WorkOrderServiceResult.Success(MapDetail(updated));
    }

    public IReadOnlyCollection<WorkOrderStatusResponse> GetStatuses()
    {
        return Statuses;
    }

    private async Task<string> GenerateUniqueOrderNumberAsync(
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxOrderNumberAttempts; attempt++)
        {
            var orderNumber = orderNumberGenerator.Generate(now);
            var exists = await dbContext.WorkOrders
                .AnyAsync(order => order.OrderNumber == orderNumber, cancellationToken);

            if (!exists)
            {
                return orderNumber;
            }
        }

        throw new InvalidOperationException("Unable to generate a unique work order number.");
    }

    private async Task<ValidationResult<WorkOrderInput>> ValidateUpsertAsync(
        WorkOrderUpsertRequest request,
        WorkOrder? existingOrder,
        bool isCreate,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var patientName = NormalizeRequired(errors, nameof(request.PatientName), request.PatientName);
        var workDescription = NormalizeRequired(errors, nameof(request.WorkDescription), request.WorkDescription);
        var referenceNumber = NormalizeAndValidateMax(
            errors,
            nameof(request.ReferenceNumber),
            request.ReferenceNumber,
            WorkOrder.ReferenceNumberMaxLength);
        var dentalColor = NormalizeAndValidateMax(
            errors,
            nameof(request.DentalColor),
            request.DentalColor,
            WorkOrder.DentalColorMaxLength);
        var notes = NormalizeAndValidateMax(
            errors,
            nameof(request.Notes),
            request.Notes,
            WorkOrder.NotesMaxLength);

        if (patientName is not null)
        {
            ValidateMaxLength(errors, nameof(request.PatientName), patientName, WorkOrder.PatientNameMaxLength);
        }

        if (workDescription is not null)
        {
            ValidateMaxLength(
                errors,
                nameof(request.WorkDescription),
                workDescription,
                WorkOrder.WorkDescriptionMaxLength);
        }

        if (request.CustomerId == Guid.Empty)
        {
            AddError(errors, nameof(request.CustomerId), "CustomerId is required.");
        }

        if (request.InternalDoctorId == Guid.Empty)
        {
            AddError(errors, nameof(request.InternalDoctorId), "InternalDoctorId is invalid.");
        }

        if (request.ReceivedDate == default)
        {
            AddError(errors, nameof(request.ReceivedDate), "ReceivedDate is required.");
        }

        if (request.FirstTrialDate.HasValue && request.FirstTrialDate.Value < request.ReceivedDate)
        {
            AddError(errors, nameof(request.FirstTrialDate), "FirstTrialDate cannot be before ReceivedDate.");
        }

        if (request.SecondTrialDate.HasValue && request.SecondTrialDate.Value < request.ReceivedDate)
        {
            AddError(errors, nameof(request.SecondTrialDate), "SecondTrialDate cannot be before ReceivedDate.");
        }

        if (request.FirstTrialDate.HasValue
            && request.SecondTrialDate.HasValue
            && request.SecondTrialDate.Value < request.FirstTrialDate.Value)
        {
            AddError(errors, nameof(request.SecondTrialDate), "SecondTrialDate cannot be before FirstTrialDate.");
        }

        if (request.DeliveryDate.HasValue && request.DeliveryDate.Value < request.ReceivedDate)
        {
            AddError(errors, nameof(request.DeliveryDate), "DeliveryDate cannot be before ReceivedDate.");
        }

        if (request.TotalAmount.HasValue && request.TotalAmount.Value < 0)
        {
            AddError(errors, nameof(request.TotalAmount), "TotalAmount must be greater than or equal to 0.");
        }

        if (errors.Count > 0
            || patientName is null
            || workDescription is null)
        {
            return new ValidationResult<WorkOrderInput>(errors, null);
        }

        var customer = await dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(currentCustomer => currentCustomer.Id == request.CustomerId, cancellationToken);

        if (customer is null)
        {
            return new ValidationResult<WorkOrderInput>(
                errors,
                WorkOrderInput.WithConflict("Customer must exist before creating or updating a work order."));
        }

        if (isCreate && !customer.IsActive)
        {
            return new ValidationResult<WorkOrderInput>(
                errors,
                WorkOrderInput.WithConflict("New work orders require an active customer."));
        }

        if (!isCreate
            && existingOrder is not null
            && existingOrder.CustomerId != request.CustomerId
            && !customer.IsActive)
        {
            return new ValidationResult<WorkOrderInput>(
                errors,
                WorkOrderInput.WithConflict("Changing a work order to an inactive customer is not allowed."));
        }

        if (request.InternalDoctorId.HasValue)
        {
            if (customer.Type != CustomerType.Clinic)
            {
                return new ValidationResult<WorkOrderInput>(
                    errors,
                    WorkOrderInput.WithConflict("InternalDoctorId is only valid when Customer is a Clinic."));
            }

            var internalDoctor = await dbContext.InternalDoctors
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    doctor => doctor.Id == request.InternalDoctorId.Value,
                    cancellationToken);

            if (internalDoctor is null || internalDoctor.CustomerId != request.CustomerId)
            {
                return new ValidationResult<WorkOrderInput>(
                    errors,
                    WorkOrderInput.WithConflict("InternalDoctor must belong to the selected Customer."));
            }

            var keepsSameDoctor = !isCreate
                && existingOrder is not null
                && existingOrder.InternalDoctorId == request.InternalDoctorId;

            if (!keepsSameDoctor && !internalDoctor.IsActive)
            {
                return new ValidationResult<WorkOrderInput>(
                    errors,
                    WorkOrderInput.WithConflict("Newly selected internal doctors must be active."));
            }
        }

        return new ValidationResult<WorkOrderInput>(
            errors,
            new WorkOrderInput(
                request.CustomerId,
                request.InternalDoctorId,
                patientName,
                request.ReceivedDate,
                referenceNumber,
                workDescription,
                dentalColor,
                request.FirstTrialDate,
                request.SecondTrialDate,
                request.DeliveryDate,
                request.TotalAmount,
                notes,
                BusinessConflict: null));
    }

    private IQueryable<WorkOrder> LoadOrderDetailQuery()
    {
        return dbContext.WorkOrders
            .Include(order => order.Customer)
            .Include(order => order.InternalDoctor)
            .Include(order => order.StatusHistory);
    }

    private static WorkOrderDetailResponse MapDetail(WorkOrder order)
    {
        return new WorkOrderDetailResponse(
            order.Id,
            order.OrderNumber,
            order.CustomerId,
            order.Customer?.DisplayName ?? string.Empty,
            order.Customer?.Type.ToString() ?? string.Empty,
            order.InternalDoctorId,
            order.InternalDoctor?.FullName,
            order.PatientName,
            order.ReceivedDate,
            order.ReferenceNumber,
            order.WorkDescription,
            order.DentalColor,
            order.FirstTrialDate,
            order.SecondTrialDate,
            order.DeliveryDate,
            order.Status.ToString(),
            GetStatusLabel(order.Status),
            order.TotalAmount,
            order.Notes,
            order.Status == WorkOrderStatus.Cancelled,
            order.CreatedAtUtc,
            order.UpdatedAtUtc,
            order.StatusHistory
                .OrderByDescending(history => history.ChangedAtUtc)
                .Select(history => new WorkOrderStatusHistoryResponse(
                    history.Id,
                    history.FromStatus?.ToString(),
                    history.FromStatus.HasValue ? GetStatusLabel(history.FromStatus.Value) : null,
                    history.ToStatus.ToString(),
                    GetStatusLabel(history.ToStatus),
                    history.Notes,
                    history.ChangedAtUtc))
                .ToArray());
    }

    private static bool TryParseStatus(string? value, out WorkOrderStatus status)
    {
        if (string.IsNullOrWhiteSpace(value)
            || !Enum.TryParse(value.Trim(), ignoreCase: true, out status)
            || !Enum.IsDefined(status))
        {
            status = default;
            return false;
        }

        return true;
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

    private static string GetDeliveryStatusLabel(DeliveryStatus status)
    {
        return status switch
        {
            DeliveryStatus.PendingAssignment => "Pendiente de asignar",
            DeliveryStatus.Assigned => "Asignada",
            DeliveryStatus.OutForDelivery => "En ruta",
            DeliveryStatus.Delivered => "Entregada",
            DeliveryStatus.FailedDelivery => "No entregada",
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

    private sealed record WorkOrderInput(
        Guid CustomerId,
        Guid? InternalDoctorId,
        string PatientName,
        DateOnly ReceivedDate,
        string? ReferenceNumber,
        string WorkDescription,
        string? DentalColor,
        DateOnly? FirstTrialDate,
        DateOnly? SecondTrialDate,
        DateOnly? DeliveryDate,
        decimal? TotalAmount,
        string? Notes,
        string? BusinessConflict)
    {
        public static WorkOrderInput WithConflict(string message)
        {
            return new WorkOrderInput(
                Guid.Empty,
                null,
                string.Empty,
                default,
                null,
                string.Empty,
                null,
                null,
                null,
                null,
                null,
                null,
                message);
        }
    }

    private sealed record WorkOrderListItemProjection(
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
        WorkOrderDeliverySummaryProjection? Delivery);

    private sealed record WorkOrderDeliverySummaryProjection(
        Guid DeliveryId,
        DeliveryStatus DeliveryStatus,
        string? AssignedToUserName,
        DateTimeOffset? DeliveredAtUtc,
        DateTimeOffset? FailedAtUtc);

    private sealed record ValidationResult<T>(
        IReadOnlyDictionary<string, string[]> Errors,
        T? Value);
}
