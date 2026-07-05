using System.Data;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using LaboratorioTlahuac.Domain.Catalog.Entities;
using LaboratorioTlahuac.Infrastructure.Persistence;

namespace LaboratorioTlahuac.Infrastructure.Catalog;

public sealed class CatalogSeeder(LaboratorioTlahuacDbContext dbContext)
    : ICatalogSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!await CatalogTablesExistAsync(cancellationToken))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var sectionsByKey = await dbContext.CatalogSections
            .ToDictionaryAsync(section => section.Key, StringComparer.Ordinal, cancellationToken);

        var sectionSortOrder = 0;

        foreach (var sectionSeed in CatalogSeedData.Sections)
        {
            var sectionImagePath = ValidateSeedImagePath(sectionSeed.ImagePath);
            var sectionAltText = sectionImagePath is null
                ? null
                : $"Imagen representativa de {sectionSeed.Name}";

            if (!sectionsByKey.TryGetValue(sectionSeed.Key, out var section))
            {
                section = CatalogSection.Create(
                    sectionSeed.Key,
                    sectionSeed.Name,
                    description: null,
                    sectionImagePath,
                    sectionAltText,
                    sectionSortOrder,
                    isActive: true,
                    now);

                dbContext.CatalogSections.Add(section);
                sectionsByKey.Add(section.Key, section);
            }
            else
            {
                section.BackfillSeedData(sectionImagePath, sectionAltText, now);
            }

            sectionSortOrder++;
        }

        var productsByKey = await dbContext.CatalogProducts
            .ToDictionaryAsync(product => product.Key, StringComparer.Ordinal, cancellationToken);

        foreach (var sectionSeed in CatalogSeedData.Sections)
        {
            var section = sectionsByKey[sectionSeed.Key];
            var productSortOrder = 0;

            foreach (var productSeed in sectionSeed.Products)
            {
                var productImagePath = ValidateSeedImagePath(productSeed.ImagePath);
                var productAltText = productImagePath is null
                    ? null
                    : $"{productSeed.Name} - {sectionSeed.Name}";

                if (!productsByKey.TryGetValue(productSeed.Key, out var product))
                {
                    product = CatalogProduct.Create(
                        section.Id,
                        productSeed.Key,
                        productSeed.Name,
                        description: null,
                        productSeed.PriceAmount,
                        CatalogProduct.DefaultCurrency,
                        productImagePath,
                        productAltText,
                        productSortOrder,
                        isActive: true,
                        now);

                    dbContext.CatalogProducts.Add(product);
                    productsByKey.Add(product.Key, product);
                }
                else
                {
                    product.BackfillSeedData(productImagePath, productAltText, now);
                }

                productSortOrder++;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<bool> CatalogTablesExistAsync(CancellationToken cancellationToken)
    {
        var providerName = dbContext.Database.ProviderName ?? string.Empty;
        var connection = dbContext.Database.GetDbConnection();
        var shouldClose = connection.State == ConnectionState.Closed;

        if (shouldClose)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();

            if (providerName.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                command.CommandText = """
                    SELECT COUNT(1)
                    FROM sqlite_master
                    WHERE type = 'table'
                        AND name IN ('CatalogSections', 'CatalogProducts')
                    """;
            }
            else
            {
                command.CommandText = """
                    SELECT COUNT(1)
                    FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_NAME IN ('CatalogSections', 'CatalogProducts')
                    """;
            }

            var result = await command.ExecuteScalarAsync(cancellationToken);

            return Convert.ToInt32(result, CultureInfo.InvariantCulture) == 2;
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static string? ValidateSeedImagePath(string? imagePath)
    {
        if (imagePath is null)
        {
            return null;
        }

        if (!CatalogImagePathValidator.IsSafeCatalogAssetPath(imagePath))
        {
            throw new InvalidOperationException($"Catalog seed contains an unsafe image path: {imagePath}");
        }

        return imagePath;
    }
}
