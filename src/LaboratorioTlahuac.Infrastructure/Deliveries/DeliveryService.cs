using Microsoft.EntityFrameworkCore;
using LaboratorioTlahuac.Application.Abstractions.Security;
using LaboratorioTlahuac.Application.Abstractions.Time;
using LaboratorioTlahuac.Application.Deliveries;
using LaboratorioTlahuac.Domain.Deliveries;
using LaboratorioTlahuac.Domain.Deliveries.Entities;
using LaboratorioTlahuac.Domain.Security;
using LaboratorioTlahuac.Domain.Security.Entities;
using LaboratorioTlahuac.Domain.WorkOrders;
using LaboratorioTlahuac.Domain.WorkOrders.Entities;
using LaboratorioTlahuac.Infrastructure.Persistence;

namespace LaboratorioTlahuac.Infrastructure.Deliveries;

public sealed class DeliveryService(
    LaboratorioTlahuacDbContext dbContext,
    IClock clock,
    ICurrentUser currentUser)
    : IDeliveryService
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    public async Task<DeliveryServiceResult<DeliveryPagedResponse<DeliveryResponse>>> ListAsync(
        DeliveryListQuery query,
        CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var page = query.Page ?? DefaultPage;
        var pageSize = query.PageSize ?? DefaultPageSize;
        var status = default(DeliveryStatus?);

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

        if (errors.Count > 0)
        {
            return DeliveryServiceResult.Validation<DeliveryPagedResponse<DeliveryResponse>>(errors);
        }

        var deliveriesQuery = ApplyVisibility(
            LoadDeliveryQuery().AsNoTracking(),
            query.AssignedToMe == true);

        if (status.HasValue)
        {
            deliveriesQuery = deliveriesQuery.Where(delivery => delivery.Status == status.Value);
        }

        var totalCount = await deliveriesQuery.CountAsync(cancellationToken);
        var deliveries = await deliveriesQuery
            .OrderBy(delivery => delivery.WorkOrder!.DeliveryDate == null)
            .ThenBy(delivery => delivery.WorkOrder!.DeliveryDate)
            .ThenByDescending(delivery => delivery.WorkOrder!.ReceivedDate)
            .ThenBy(delivery => delivery.WorkOrder!.OrderNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return DeliveryServiceResult.Success(
            new DeliveryPagedResponse<DeliveryResponse>(
                deliveries.Select(MapDelivery).ToArray(),
                page,
                pageSize,
                totalCount));
    }

    public async Task<DeliveryServiceResult<DeliveryResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var delivery = await ApplyVisibility(LoadDeliveryQuery().AsNoTracking(), assignedToMeOnly: false)
            .FirstOrDefaultAsync(currentDelivery => currentDelivery.Id == id, cancellationToken);

        return delivery is null
            ? DeliveryServiceResult.NotFound<DeliveryResponse>("Delivery was not found.")
            : DeliveryServiceResult.Success(MapDelivery(delivery));
    }

    public async Task<DeliveryServiceResult<DeliveryResponse>> GetByWorkOrderIdAsync(
        Guid workOrderId,
        CancellationToken cancellationToken = default)
    {
        var delivery = await ApplyVisibility(LoadDeliveryQuery().AsNoTracking(), assignedToMeOnly: false)
            .FirstOrDefaultAsync(currentDelivery => currentDelivery.WorkOrderId == workOrderId, cancellationToken);

        return delivery is null
            ? DeliveryServiceResult.NotFound<DeliveryResponse>("Delivery was not found.")
            : DeliveryServiceResult.Success(MapDelivery(delivery));
    }

    public async Task<DeliveryServiceResult<DeliveryResponse>> CreateForWorkOrderAsync(
        Guid workOrderId,
        DeliveryCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var deliveryNotes = NormalizeAndValidateMax(
            errors,
            nameof(request.DeliveryNotes),
            request.DeliveryNotes,
            WorkOrderDelivery.DeliveryNotesMaxLength);

        if (workOrderId == Guid.Empty)
        {
            AddError(errors, nameof(workOrderId), "WorkOrderId is required.");
        }

        if (errors.Count > 0)
        {
            return DeliveryServiceResult.Validation<DeliveryResponse>(errors);
        }

        var workOrder = await dbContext.WorkOrders
            .FirstOrDefaultAsync(order => order.Id == workOrderId, cancellationToken);

        if (workOrder is null)
        {
            return DeliveryServiceResult.NotFound<DeliveryResponse>("Work order was not found.");
        }

        if (workOrder.Status == WorkOrderStatus.Cancelled)
        {
            return DeliveryServiceResult.Conflict<DeliveryResponse>(
                "Cancelled work orders cannot create a delivery.");
        }

        var existingDelivery = await dbContext.WorkOrderDeliveries
            .AsNoTracking()
            .AnyAsync(delivery => delivery.WorkOrderId == workOrderId, cancellationToken);

        if (existingDelivery)
        {
            return DeliveryServiceResult.Conflict<DeliveryResponse>(
                "Work order already has a delivery in this MVP.");
        }

        var delivery = WorkOrderDelivery.Create(workOrderId, deliveryNotes, clock.UtcNow);

        dbContext.WorkOrderDeliveries.Add(delivery);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await LoadSavedDeliveryAsync(delivery.Id, cancellationToken);
    }

    public async Task<DeliveryServiceResult<DeliveryResponse>> AssignAsync(
        Guid id,
        DeliveryAssignRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var assignedToUserId = request.AssignedToUserId;
        var deliveryNotes = NormalizeAndValidateMax(
            errors,
            nameof(request.DeliveryNotes),
            request.DeliveryNotes,
            WorkOrderDelivery.DeliveryNotesMaxLength);

        if (!assignedToUserId.HasValue || assignedToUserId.Value == Guid.Empty)
        {
            AddError(errors, nameof(request.AssignedToUserId), "AssignedToUserId is required.");
        }

        if (errors.Count > 0 || !assignedToUserId.HasValue)
        {
            return DeliveryServiceResult.Validation<DeliveryResponse>(errors);
        }

        var assignee = await LoadUserWithPermissionsAsync(assignedToUserId.Value, cancellationToken);

        if (assignee is null || !assignee.IsActive)
        {
            return DeliveryServiceResult.Conflict<DeliveryResponse>("Assigned user must be active.");
        }

        if (!UserGrantsPermission(assignee, Permissions.DeliveriesView))
        {
            return DeliveryServiceResult.Conflict<DeliveryResponse>(
                "Assigned user must have deliveries.view.");
        }

        var delivery = await LoadDeliveryForMutationAsync(id, cancellationToken);

        if (delivery is null)
        {
            return DeliveryServiceResult.NotFound<DeliveryResponse>("Delivery was not found.");
        }

        if (delivery.WorkOrder?.Status == WorkOrderStatus.Cancelled)
        {
            return DeliveryServiceResult.Conflict<DeliveryResponse>(
                "Cancelled work orders cannot be assigned for delivery.");
        }

        try
        {
            delivery.Assign(assignedToUserId.Value, deliveryNotes, clock.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            return InvalidTransition<DeliveryResponse>(ex.Message);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return await LoadSavedDeliveryAsync(delivery.Id, cancellationToken);
    }

    public async Task<DeliveryServiceResult<DeliveryResponse>> MarkOutForDeliveryAsync(
        Guid id,
        DeliveryOutForDeliveryRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var deliveryNotes = NormalizeAndValidateMax(
            errors,
            nameof(request.DeliveryNotes),
            request.DeliveryNotes,
            WorkOrderDelivery.DeliveryNotesMaxLength);

        if (errors.Count > 0)
        {
            return DeliveryServiceResult.Validation<DeliveryResponse>(errors);
        }

        var delivery = await LoadDeliveryForMutationAsync(id, cancellationToken);

        if (delivery is null)
        {
            return DeliveryServiceResult.NotFound<DeliveryResponse>("Delivery was not found.");
        }

        if (delivery.WorkOrder?.Status == WorkOrderStatus.Cancelled)
        {
            return DeliveryServiceResult.Conflict<DeliveryResponse>(
                "Cancelled work orders cannot go out for delivery.");
        }

        try
        {
            delivery.MarkOutForDelivery(deliveryNotes, clock.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            return InvalidTransition<DeliveryResponse>(ex.Message);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return await LoadSavedDeliveryAsync(delivery.Id, cancellationToken);
    }

    public async Task<DeliveryServiceResult<DeliveryResponse>> CompleteAsync(
        Guid id,
        DeliveryCompleteRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var recipientName = NormalizeRequired(errors, nameof(request.RecipientName), request.RecipientName);
        var deliveryNotes = NormalizeAndValidateMax(
            errors,
            nameof(request.DeliveryNotes),
            request.DeliveryNotes,
            WorkOrderDelivery.DeliveryNotesMaxLength);

        if (recipientName is not null)
        {
            ValidateMaxLength(
                errors,
                nameof(request.RecipientName),
                recipientName,
                WorkOrderDelivery.RecipientNameMaxLength);
        }

        if (errors.Count > 0 || recipientName is null)
        {
            return DeliveryServiceResult.Validation<DeliveryResponse>(errors);
        }

        var delivery = await LoadDeliveryForMutationAsync(id, cancellationToken);

        if (delivery is null)
        {
            return DeliveryServiceResult.NotFound<DeliveryResponse>("Delivery was not found.");
        }

        if (!CanMutateDelivery(delivery))
        {
            return DeliveryServiceResult.Forbidden<DeliveryResponse>("Delivery is not assigned to the current user.");
        }

        if (delivery.WorkOrder?.Status == WorkOrderStatus.Cancelled)
        {
            return DeliveryServiceResult.Conflict<DeliveryResponse>(
                "Cancelled work orders cannot be completed for delivery.");
        }

        var now = clock.UtcNow;

        try
        {
            delivery.Complete(recipientName, deliveryNotes, now);
        }
        catch (InvalidOperationException ex)
        {
            return InvalidTransition<DeliveryResponse>(ex.Message);
        }

        if (delivery.WorkOrder is not null && delivery.WorkOrder.Status != WorkOrderStatus.Delivered)
        {
            var history = delivery.WorkOrder.ChangeStatus(
                WorkOrderStatus.Delivered,
                "Entrega completada.",
                currentUser.UserId,
                now);

            if (history is not null)
            {
                dbContext.WorkOrderStatusHistory.Add(history);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return await LoadSavedDeliveryAsync(delivery.Id, cancellationToken);
    }

    public async Task<DeliveryServiceResult<DeliveryResponse>> MarkFailedAsync(
        Guid id,
        DeliveryFailedRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var failedReason = NormalizeRequired(errors, nameof(request.FailedReason), request.FailedReason);
        var deliveryNotes = NormalizeAndValidateMax(
            errors,
            nameof(request.DeliveryNotes),
            request.DeliveryNotes,
            WorkOrderDelivery.DeliveryNotesMaxLength);

        if (failedReason is not null)
        {
            ValidateMaxLength(
                errors,
                nameof(request.FailedReason),
                failedReason,
                WorkOrderDelivery.FailedReasonMaxLength);
        }

        if (errors.Count > 0 || failedReason is null)
        {
            return DeliveryServiceResult.Validation<DeliveryResponse>(errors);
        }

        var delivery = await LoadDeliveryForMutationAsync(id, cancellationToken);

        if (delivery is null)
        {
            return DeliveryServiceResult.NotFound<DeliveryResponse>("Delivery was not found.");
        }

        if (!CanMutateDelivery(delivery))
        {
            return DeliveryServiceResult.Forbidden<DeliveryResponse>("Delivery is not assigned to the current user.");
        }

        if (delivery.WorkOrder?.Status == WorkOrderStatus.Cancelled)
        {
            return DeliveryServiceResult.Conflict<DeliveryResponse>(
                "Cancelled work orders cannot be marked as failed delivery.");
        }

        try
        {
            delivery.MarkFailed(failedReason, deliveryNotes, clock.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            return InvalidTransition<DeliveryResponse>(ex.Message);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return await LoadSavedDeliveryAsync(delivery.Id, cancellationToken);
    }

    private IQueryable<WorkOrderDelivery> LoadDeliveryQuery()
    {
        return dbContext.WorkOrderDeliveries
            .Include(delivery => delivery.AssignedToUser)
            .Include(delivery => delivery.WorkOrder)
                .ThenInclude(order => order!.Customer)
            .Include(delivery => delivery.WorkOrder)
                .ThenInclude(order => order!.InternalDoctor);
    }

    private async Task<WorkOrderDelivery?> LoadDeliveryForMutationAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await dbContext.WorkOrderDeliveries
            .Include(delivery => delivery.WorkOrder)
            .FirstOrDefaultAsync(delivery => delivery.Id == id, cancellationToken);
    }

    private async Task<DeliveryServiceResult<DeliveryResponse>> LoadSavedDeliveryAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var saved = await LoadDeliveryQuery()
            .AsNoTracking()
            .FirstAsync(delivery => delivery.Id == id, cancellationToken);

        return DeliveryServiceResult.Success(MapDelivery(saved));
    }

    private IQueryable<WorkOrderDelivery> ApplyVisibility(
        IQueryable<WorkOrderDelivery> query,
        bool assignedToMeOnly)
    {
        if (!assignedToMeOnly && CanViewAllDeliveries())
        {
            return query;
        }

        var userId = currentUser.UserId;

        return userId.HasValue
            ? query.Where(delivery => delivery.AssignedToUserId == userId.Value)
            : query.Where(delivery => false);
    }

    private bool CanViewAllDeliveries()
    {
        return currentUser.Permissions.Contains(Permissions.DeliveriesAssign, StringComparer.Ordinal)
            || currentUser.Permissions.Contains(Permissions.OrdersView, StringComparer.Ordinal);
    }

    private bool CanMutateDelivery(WorkOrderDelivery delivery)
    {
        return CanViewAllDeliveries()
            || (currentUser.UserId.HasValue && delivery.AssignedToUserId == currentUser.UserId.Value);
    }

    private async Task<User?> LoadUserWithPermissionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                    .ThenInclude(role => role!.RolePermissions)
                        .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);
    }

    private static bool UserGrantsPermission(User user, string permission)
    {
        return user.UserRoles
            .Select(userRole => userRole.Role)
            .Where(role => role is not null)
            .SelectMany(role => role!.RolePermissions)
            .Select(rolePermission => rolePermission.Permission)
            .Any(currentPermission => currentPermission?.Key == permission);
    }

    private static DeliveryResponse MapDelivery(WorkOrderDelivery delivery)
    {
        var workOrder = delivery.WorkOrder;
        var customer = workOrder?.Customer;

        return new DeliveryResponse(
            delivery.Id,
            delivery.WorkOrderId,
            workOrder?.OrderNumber ?? string.Empty,
            workOrder?.CustomerId ?? Guid.Empty,
            customer?.DisplayName ?? string.Empty,
            customer?.Address,
            customer?.ContactName,
            customer?.Phone,
            customer?.WhatsApp,
            workOrder?.InternalDoctorId,
            workOrder?.InternalDoctor?.FullName,
            workOrder?.PatientName ?? string.Empty,
            workOrder?.ReferenceNumber,
            workOrder?.WorkDescription ?? string.Empty,
            workOrder?.DeliveryDate,
            workOrder?.Status.ToString() ?? string.Empty,
            workOrder is not null ? GetWorkOrderStatusLabel(workOrder.Status) : string.Empty,
            delivery.Status.ToString(),
            GetDeliveryStatusLabel(delivery.Status),
            delivery.AssignedToUserId,
            delivery.AssignedToUser?.FullName,
            delivery.RecipientName,
            delivery.DeliveryNotes,
            delivery.FailedReason,
            delivery.AssignedAtUtc,
            delivery.OutForDeliveryAtUtc,
            delivery.DeliveredAtUtc,
            delivery.FailedAtUtc,
            delivery.CreatedAtUtc,
            delivery.UpdatedAtUtc);
    }

    private static bool TryParseStatus(string? value, out DeliveryStatus status)
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

    private static string GetWorkOrderStatusLabel(WorkOrderStatus status)
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

    private static DeliveryServiceResult<T> InvalidTransition<T>(string message)
    {
        return DeliveryServiceResult.Validation<T>(
            new Dictionary<string, string[]>
            {
                ["Status"] = [message]
            });
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
}
