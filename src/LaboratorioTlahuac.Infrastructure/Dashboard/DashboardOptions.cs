namespace LaboratorioTlahuac.Infrastructure.Dashboard;

public sealed class DashboardOptions
{
    public const string SectionName = "Dashboard";
    public const string DefaultBusinessTimeZone = "America/Mexico_City";

    public string BusinessTimeZone { get; init; } = DefaultBusinessTimeZone;
}
