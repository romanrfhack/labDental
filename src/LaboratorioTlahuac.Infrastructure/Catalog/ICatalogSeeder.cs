namespace LaboratorioTlahuac.Infrastructure.Catalog;

public interface ICatalogSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
