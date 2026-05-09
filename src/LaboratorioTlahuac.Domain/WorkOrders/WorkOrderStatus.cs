namespace LaboratorioTlahuac.Domain.WorkOrders;

public enum WorkOrderStatus
{
    Received = 1,
    InProcess = 2,
    FirstTrial = 3,
    SecondTrial = 4,
    ReadyForDelivery = 5,
    Delivered = 6,
    Cancelled = 7
}
