namespace LaboratorioTlahuac.Domain.Payments;

public enum PaymentStatus
{
    TotalNotSet = 1,
    Unpaid = 2,
    Partial = 3,
    Paid = 4,
    Overpaid = 5
}
