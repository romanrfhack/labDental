namespace LaboratorioTlahuac.Infrastructure.Catalog;

public sealed class CatalogImagesOptions
{
    public const string SectionName = "CatalogImages";
    public const long MaximumFileSizeBytes = 2_097_152;

    public string StoragePath { get; init; } = string.Empty;
}
