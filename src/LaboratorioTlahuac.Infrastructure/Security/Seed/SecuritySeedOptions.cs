namespace LaboratorioTlahuac.Infrastructure.Security.Seed;

public sealed class SecuritySeedOptions
{
    public const string SectionName = "SecuritySeed";

    public bool RunOnStartup { get; init; }

    public string AdminRoleName { get; init; } = "Admin";

    public string AdminRoleDescription { get; init; } = "Administrador del sistema.";

    public AdminSeedOptions Admin { get; init; } = new();
}

public sealed class AdminSeedOptions
{
    public string? Email { get; init; }

    public string? Password { get; init; }

    public string? FullName { get; init; }
}
