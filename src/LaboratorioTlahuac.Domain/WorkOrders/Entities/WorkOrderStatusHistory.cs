using LaboratorioTlahuac.Domain.WorkOrders;

namespace LaboratorioTlahuac.Domain.WorkOrders.Entities;

public sealed class WorkOrderStatusHistory
{
    public const int NotesMaxLength = 1000;

    private WorkOrderStatusHistory()
    {
    }

    private WorkOrderStatusHistory(
        Guid id,
        Guid workOrderId,
        WorkOrderStatus? fromStatus,
        WorkOrderStatus toStatus,
        string? notes,
        Guid? changedByUserId,
        DateTimeOffset changedAtUtc)
    {
        Id = id;
        WorkOrderId = workOrderId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        Notes = TrimOptional(notes);
        ChangedByUserId = changedByUserId;
        ChangedAtUtc = changedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid WorkOrderId { get; private set; }

    public WorkOrderStatus? FromStatus { get; private set; }

    public WorkOrderStatus ToStatus { get; private set; }

    public string? Notes { get; private set; }

    public DateTimeOffset ChangedAtUtc { get; private set; }

    public Guid? ChangedByUserId { get; private set; }

    public WorkOrder? WorkOrder { get; private set; }

    public static WorkOrderStatusHistory Create(
        Guid workOrderId,
        WorkOrderStatus? fromStatus,
        WorkOrderStatus toStatus,
        string? notes,
        Guid? changedByUserId,
        DateTimeOffset changedAtUtc)
    {
        if (!Enum.IsDefined(toStatus))
        {
            throw new ArgumentOutOfRangeException(nameof(toStatus), "Work order status is invalid.");
        }

        if (fromStatus.HasValue && !Enum.IsDefined(fromStatus.Value))
        {
            throw new ArgumentOutOfRangeException(nameof(fromStatus), "Previous work order status is invalid.");
        }

        return new WorkOrderStatusHistory(
            Guid.NewGuid(),
            workOrderId,
            fromStatus,
            toStatus,
            notes,
            changedByUserId,
            changedAtUtc);
    }

    private static string? TrimOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
