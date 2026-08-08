using LaboratorioTlahuac.Domain.Catalog.Entities;

namespace LaboratorioTlahuac.Infrastructure.Catalog;

internal static class CatalogImagePathValidator
{
    private const string CatalogProductsPrefix = "assets/catalog/products/";
    private const string CatalogImagesApiPrefix = "/api/catalog/images/";
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".webp", ".jpg", ".jpeg", ".png" };

    public static string? NormalizeAndValidate(
        IDictionary<string, string[]> errors,
        string fieldName,
        string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        if (normalized is null)
        {
            return null;
        }

        if (normalized.Length > CatalogProduct.ImagePathMaxLength)
        {
            AddError(errors, fieldName, $"{fieldName} must be {CatalogProduct.ImagePathMaxLength} characters or fewer.");
            return normalized;
        }

        if (!IsSafeCatalogImagePath(normalized))
        {
            AddError(errors, fieldName, $"{fieldName} must be a safe catalog image path.");
        }

        return normalized;
    }

    public static bool IsSafeCatalogAssetPath(string path)
    {
        return IsSafeCatalogImagePath(path);
    }

    public static bool IsSafeCatalogImagePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var trimmed = path.Trim();

        if (!string.Equals(trimmed, path, StringComparison.Ordinal)
            || trimmed.Contains('\\')
            || trimmed.Contains('?')
            || trimmed.Contains('#')
            || trimmed.StartsWith("//", StringComparison.Ordinal))
        {
            return false;
        }

        if (trimmed.StartsWith(CatalogImagesApiPrefix, StringComparison.Ordinal))
        {
            return CatalogImageFileName.IsGeneratedName(trimmed[CatalogImagesApiPrefix.Length..]);
        }

        if (trimmed[0] == '/'
            || trimmed[0] == '\\'
            || Uri.TryCreate(trimmed, UriKind.Absolute, out _)
            || !trimmed.StartsWith(CatalogProductsPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        var segments = trimmed.Split('/', StringSplitOptions.None);

        if (segments.Any(segment => segment.Length == 0 || segment is "." or ".."))
        {
            return false;
        }

        var fileName = segments[^1];
        var extensionStart = fileName.LastIndexOf('.');

        if (extensionStart < 0)
        {
            return false;
        }

        var extension = fileName[extensionStart..];

        return AllowedExtensions.Contains(extension);
    }

    private static void AddError(IDictionary<string, string[]> errors, string fieldName, string error)
    {
        errors[fieldName] = errors.TryGetValue(fieldName, out var existingErrors)
            ? [.. existingErrors, error]
            : [error];
    }
}
