namespace LaboratorioTlahuac.Infrastructure.WorkOrders;

public sealed class GuidWorkOrderNumberGenerator : IWorkOrderNumberGenerator
{
    public string Generate(DateTimeOffset nowUtc)
    {
        var dateSegment = nowUtc.UtcDateTime.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
        var uniqueSegment = Guid.NewGuid().ToString("N")
            .Substring(0, 6)
            .ToUpperInvariant();

        return $"OT-{dateSegment}-{uniqueSegment}";
    }
}
