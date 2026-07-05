using LaboratorioTlahuac.Domain.Security.Entities;
using LaboratorioTlahuac.Domain.WorkOrders.Entities;

namespace LaboratorioTlahuac.Domain.Deliveries.Entities;

public sealed class WorkOrderDelivery
{
    public const int RecipientNameMaxLength = 150;
    public const int DeliveryNotesMaxLength = 1000;
    public const int FailedReasonMaxLength = 1000;

    private WorkOrderDelivery()
    {
    }

    private WorkOrderDelivery(
        Guid id,
        Guid workOrderId,
        string? deliveryNotes,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        WorkOrderId = workOrderId;
        Status = DeliveryStatus.PendingAssignment;
        DeliveryNotes = TrimOptional(deliveryNotes);
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid WorkOrderId { get; private set; }

    public Guid? AssignedToUserId { get; private set; }

    public DeliveryStatus Status { get; private set; }

    public string? RecipientName { get; private set; }

    public string? DeliveryNotes { get; private set; }

    public string? FailedReason { get; private set; }

    public DateTimeOffset? AssignedAtUtc { get; private set; }

    public DateTimeOffset? OutForDeliveryAtUtc { get; private set; }

    public DateTimeOffset? DeliveredAtUtc { get; private set; }

    public DateTimeOffset? FailedAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public WorkOrder? WorkOrder { get; private set; }

    public User? AssignedToUser { get; private set; }

    public static WorkOrderDelivery Create(
        Guid workOrderId,
        string? deliveryNotes,
        DateTimeOffset createdAtUtc)
    {
        if (workOrderId == Guid.Empty)
        {
            throw new ArgumentException("Work order id is required.", nameof(workOrderId));
        }

        return new WorkOrderDelivery(Guid.NewGuid(), workOrderId, deliveryNotes, createdAtUtc);
    }

    public void Assign(Guid assignedToUserId, string? deliveryNotes, DateTimeOffset assignedAtUtc)
    {
        if (assignedToUserId == Guid.Empty)
        {
            throw new ArgumentException("Assigned user id is required.", nameof(assignedToUserId));
        }

        if (Status is not DeliveryStatus.PendingAssignment and not DeliveryStatus.Assigned)
        {
            throw new InvalidOperationException("Delivery can only be assigned while pending or assigned.");
        }

        AssignedToUserId = assignedToUserId;
        AssignedAtUtc = assignedAtUtc;
        Status = DeliveryStatus.Assigned;
        SetNotes(deliveryNotes);
        Touch(assignedAtUtc);
    }

    public void MarkOutForDelivery(string? deliveryNotes, DateTimeOffset outForDeliveryAtUtc)
    {
        if (Status != DeliveryStatus.Assigned)
        {
            throw new InvalidOperationException("Delivery can only go out for delivery after assignment.");
        }

        OutForDeliveryAtUtc = outForDeliveryAtUtc;
        Status = DeliveryStatus.OutForDelivery;
        SetNotes(deliveryNotes);
        Touch(outForDeliveryAtUtc);
    }

    public void Complete(string recipientName, string? deliveryNotes, DateTimeOffset deliveredAtUtc)
    {
        if (Status != DeliveryStatus.OutForDelivery)
        {
            throw new InvalidOperationException("Delivery can only be completed while out for delivery.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(recipientName);

        RecipientName = recipientName.Trim();
        DeliveredAtUtc = deliveredAtUtc;
        Status = DeliveryStatus.Delivered;
        SetNotes(deliveryNotes);
        Touch(deliveredAtUtc);
    }

    public void MarkFailed(string failedReason, string? deliveryNotes, DateTimeOffset failedAtUtc)
    {
        if (Status is not DeliveryStatus.Assigned and not DeliveryStatus.OutForDelivery)
        {
            throw new InvalidOperationException("Delivery can only fail after assignment or while out for delivery.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(failedReason);

        FailedReason = failedReason.Trim();
        FailedAtUtc = failedAtUtc;
        Status = DeliveryStatus.FailedDelivery;
        SetNotes(deliveryNotes);
        Touch(failedAtUtc);
    }

    public void Retry(string? deliveryNotes, DateTimeOffset outForDeliveryAtUtc)
    {
        if (Status != DeliveryStatus.FailedDelivery)
        {
            throw new InvalidOperationException("Delivery can only be retried after a failed delivery.");
        }

        if (!AssignedToUserId.HasValue)
        {
            throw new InvalidOperationException("Delivery requires an assigned driver before retry.");
        }

        FailedReason = null;
        FailedAtUtc = null;
        OutForDeliveryAtUtc = outForDeliveryAtUtc;
        Status = DeliveryStatus.OutForDelivery;
        SetNotes(deliveryNotes);
        Touch(outForDeliveryAtUtc);
    }

    private void SetNotes(string? deliveryNotes)
    {
        DeliveryNotes = TrimOptional(deliveryNotes);
    }

    private void Touch(DateTimeOffset updatedAtUtc)
    {
        UpdatedAtUtc = updatedAtUtc;
    }

    private static string? TrimOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
