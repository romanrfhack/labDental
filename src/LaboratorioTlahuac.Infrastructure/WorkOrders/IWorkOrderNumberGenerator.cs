namespace LaboratorioTlahuac.Infrastructure.WorkOrders;

public interface IWorkOrderNumberGenerator
{
    string Generate(DateTimeOffset nowUtc);
}
