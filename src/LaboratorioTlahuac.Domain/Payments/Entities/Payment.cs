using LaboratorioTlahuac.Domain.WorkOrders.Entities;

namespace LaboratorioTlahuac.Domain.Payments.Entities;

public sealed class Payment
{
    public const int MethodMaxLength = 40;
    public const int ReferenceMaxLength = 100;
    public const int NotesMaxLength = 1000;
    public const int CancellationReasonMaxLength = 1000;

    private Payment()
    {
    }

    private Payment(
        Guid id,
        Guid workOrderId,
        DateOnly paymentDate,
        decimal amount,
        PaymentMethod method,
        string? reference,
        string? notes,
        Guid? createdByUserId,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        WorkOrderId = workOrderId;
        CreatedAtUtc = createdAtUtc;
        CreatedByUserId = createdByUserId;

        SetDetails(paymentDate, amount, method, reference, notes);
    }

    public Guid Id { get; private set; }

    public Guid WorkOrderId { get; private set; }

    public DateOnly PaymentDate { get; private set; }

    public decimal Amount { get; private set; }

    public PaymentMethod Method { get; private set; }

    public string? Reference { get; private set; }

    public string? Notes { get; private set; }

    public bool IsCancelled { get; private set; }

    public DateTimeOffset? CancelledAtUtc { get; private set; }

    public Guid? CancelledByUserId { get; private set; }

    public string? CancellationReason { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public Guid? CreatedByUserId { get; private set; }

    public WorkOrder? WorkOrder { get; private set; }

    public static Payment Create(
        Guid workOrderId,
        DateOnly paymentDate,
        decimal amount,
        PaymentMethod method,
        string? reference,
        string? notes,
        Guid? createdByUserId,
        DateTimeOffset createdAtUtc)
    {
        return new Payment(
            Guid.NewGuid(),
            workOrderId,
            paymentDate,
            amount,
            method,
            reference,
            notes,
            createdByUserId,
            createdAtUtc);
    }

    public void Cancel(string reason, Guid? cancelledByUserId, DateTimeOffset cancelledAtUtc)
    {
        if (IsCancelled)
        {
            throw new InvalidOperationException("Payment is already cancelled.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        IsCancelled = true;
        CancelledAtUtc = cancelledAtUtc;
        CancelledByUserId = cancelledByUserId;
        CancellationReason = reason.Trim();
    }

    private void SetDetails(
        DateOnly paymentDate,
        decimal amount,
        PaymentMethod method,
        string? reference,
        string? notes)
    {
        if (paymentDate == default)
        {
            throw new ArgumentOutOfRangeException(nameof(paymentDate), "Payment date is required.");
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Payment amount must be greater than 0.");
        }

        if (!Enum.IsDefined(method))
        {
            throw new ArgumentOutOfRangeException(nameof(method), "Payment method is invalid.");
        }

        PaymentDate = paymentDate;
        Amount = amount;
        Method = method;
        Reference = TrimOptional(reference);
        Notes = TrimOptional(notes);
    }

    private static string? TrimOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
