namespace LaboratorioTlahuac.Infrastructure.Security.Seed;

public interface ISecuritySeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
