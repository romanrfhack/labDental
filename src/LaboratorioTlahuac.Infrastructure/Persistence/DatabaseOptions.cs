namespace LaboratorioTlahuac.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public string Provider { get; init; } = "SqlServer";

    public string ConnectionStringName { get; init; } = "DefaultConnection";
}
