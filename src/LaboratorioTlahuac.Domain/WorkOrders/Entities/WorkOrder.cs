using LaboratorioTlahuac.Domain.Customers.Entities;
using LaboratorioTlahuac.Domain.Payments.Entities;

namespace LaboratorioTlahuac.Domain.WorkOrders.Entities;

public sealed class WorkOrder
{
    public const int OrderNumberMaxLength = 40;
    public const int PatientNameMaxLength = 150;
    public const int ReferenceNumberMaxLength = 80;
    public const int WorkDescriptionMaxLength = 1000;
    public const int DentalColorMaxLength = 50;
    public const int NotesMaxLength = 1000;

    private WorkOrder()
    {
    }

    private WorkOrder(
        Guid id,
        string orderNumber,
        Guid customerId,
        Guid? internalDoctorId,
        string patientName,
        DateOnly receivedDate,
        string? referenceNumber,
        string workDescription,
        string? dentalColor,
        DateOnly? firstTrialDate,
        DateOnly? secondTrialDate,
        DateOnly? deliveryDate,
        decimal? totalAmount,
        string? notes,
        Guid? createdByUserId,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        OrderNumber = TrimRequired(orderNumber);
        CustomerId = customerId;
        InternalDoctorId = internalDoctorId;
        Status = WorkOrderStatus.Received;
        CreatedAtUtc = createdAtUtc;
        CreatedByUserId = createdByUserId;
        UpdatedAtUtc = createdAtUtc;
        UpdatedByUserId = createdByUserId;

        SetDetails(
            customerId,
            internalDoctorId,
            patientName,
            receivedDate,
            referenceNumber,
            workDescription,
            dentalColor,
            firstTrialDate,
            secondTrialDate,
            deliveryDate,
            totalAmount,
            notes);

        StatusHistory.Add(WorkOrderStatusHistory.Create(
            Id,
            fromStatus: null,
            WorkOrderStatus.Received,
            notes: null,
            createdByUserId,
            createdAtUtc));
    }

    public Guid Id { get; private set; }

    public string OrderNumber { get; private set; } = string.Empty;

    public Guid CustomerId { get; private set; }

    public Guid? InternalDoctorId { get; private set; }

    public string PatientName { get; private set; } = string.Empty;

    public DateOnly ReceivedDate { get; private set; }

    public string? ReferenceNumber { get; private set; }

    public string WorkDescription { get; private set; } = string.Empty;

    public string? DentalColor { get; private set; }

    public DateOnly? FirstTrialDate { get; private set; }

    public DateOnly? SecondTrialDate { get; private set; }

    public DateOnly? DeliveryDate { get; private set; }

    public WorkOrderStatus Status { get; private set; }

    public decimal? TotalAmount { get; private set; }

    public string? Notes { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public Guid? CreatedByUserId { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public Guid? UpdatedByUserId { get; private set; }

    public Customer? Customer { get; private set; }

    public InternalDoctor? InternalDoctor { get; private set; }

    public ICollection<WorkOrderStatusHistory> StatusHistory { get; private set; } =
        new List<WorkOrderStatusHistory>();

    public ICollection<Payment> Payments { get; private set; } = new List<Payment>();

    public static WorkOrder Create(
        string orderNumber,
        Guid customerId,
        Guid? internalDoctorId,
        string patientName,
        DateOnly receivedDate,
        string? referenceNumber,
        string workDescription,
        string? dentalColor,
        DateOnly? firstTrialDate,
        DateOnly? secondTrialDate,
        DateOnly? deliveryDate,
        decimal? totalAmount,
        string? notes,
        Guid? createdByUserId,
        DateTimeOffset createdAtUtc)
    {
        return new WorkOrder(
            Guid.NewGuid(),
            orderNumber,
            customerId,
            internalDoctorId,
            patientName,
            receivedDate,
            referenceNumber,
            workDescription,
            dentalColor,
            firstTrialDate,
            secondTrialDate,
            deliveryDate,
            totalAmount,
            notes,
            createdByUserId,
            createdAtUtc);
    }

    public void UpdateGeneral(
        Guid customerId,
        Guid? internalDoctorId,
        string patientName,
        DateOnly receivedDate,
        string? referenceNumber,
        string workDescription,
        string? dentalColor,
        DateOnly? firstTrialDate,
        DateOnly? secondTrialDate,
        DateOnly? deliveryDate,
        decimal? totalAmount,
        string? notes,
        Guid? updatedByUserId,
        DateTimeOffset updatedAtUtc)
    {
        if (Status == WorkOrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled work orders cannot be edited.");
        }

        SetDetails(
            customerId,
            internalDoctorId,
            patientName,
            receivedDate,
            referenceNumber,
            workDescription,
            dentalColor,
            firstTrialDate,
            secondTrialDate,
            deliveryDate,
            totalAmount,
            notes);

        Touch(updatedByUserId, updatedAtUtc);
    }

    public WorkOrderStatusHistory? ChangeStatus(
        WorkOrderStatus status,
        string? notes,
        Guid? changedByUserId,
        DateTimeOffset changedAtUtc)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), "Work order status is invalid.");
        }

        if (Status == WorkOrderStatus.Cancelled && status != WorkOrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled work orders cannot change status.");
        }

        if (Status == status)
        {
            return null;
        }

        var previousStatus = Status;
        Status = status;
        var history = WorkOrderStatusHistory.Create(
            Id,
            previousStatus,
            status,
            notes,
            changedByUserId,
            changedAtUtc);
        StatusHistory.Add(history);

        Touch(changedByUserId, changedAtUtc);

        return history;
    }

    private void SetDetails(
        Guid customerId,
        Guid? internalDoctorId,
        string patientName,
        DateOnly receivedDate,
        string? referenceNumber,
        string workDescription,
        string? dentalColor,
        DateOnly? firstTrialDate,
        DateOnly? secondTrialDate,
        DateOnly? deliveryDate,
        decimal? totalAmount,
        string? notes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(patientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(workDescription);

        if (receivedDate == default)
        {
            throw new ArgumentOutOfRangeException(nameof(receivedDate), "Received date is required.");
        }

        if (firstTrialDate.HasValue && firstTrialDate.Value < receivedDate)
        {
            throw new ArgumentOutOfRangeException(nameof(firstTrialDate), "First trial date cannot be before received date.");
        }

        if (secondTrialDate.HasValue && secondTrialDate.Value < receivedDate)
        {
            throw new ArgumentOutOfRangeException(nameof(secondTrialDate), "Second trial date cannot be before received date.");
        }

        if (firstTrialDate.HasValue
            && secondTrialDate.HasValue
            && secondTrialDate.Value < firstTrialDate.Value)
        {
            throw new ArgumentOutOfRangeException(nameof(secondTrialDate), "Second trial date cannot be before first trial date.");
        }

        if (deliveryDate.HasValue && deliveryDate.Value < receivedDate)
        {
            throw new ArgumentOutOfRangeException(nameof(deliveryDate), "Delivery date cannot be before received date.");
        }

        if (totalAmount.HasValue && totalAmount.Value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalAmount), "Total amount cannot be negative.");
        }

        CustomerId = customerId;
        InternalDoctorId = internalDoctorId;
        PatientName = TrimRequired(patientName);
        ReceivedDate = receivedDate;
        ReferenceNumber = TrimOptional(referenceNumber);
        WorkDescription = TrimRequired(workDescription);
        DentalColor = TrimOptional(dentalColor);
        FirstTrialDate = firstTrialDate;
        SecondTrialDate = secondTrialDate;
        DeliveryDate = deliveryDate;
        TotalAmount = totalAmount;
        Notes = TrimOptional(notes);
    }

    private void Touch(Guid? updatedByUserId, DateTimeOffset updatedAtUtc)
    {
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;
    }

    private static string TrimRequired(string value)
    {
        return value.Trim();
    }

    private static string? TrimOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
