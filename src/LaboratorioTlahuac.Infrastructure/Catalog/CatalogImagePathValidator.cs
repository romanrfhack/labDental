using LaboratorioTlahuac.Domain.Catalog.Entities;

namespace LaboratorioTlahuac.Infrastructure.Catalog;

internal static class CatalogImagePathValidator
{
    private const string CatalogProductsPrefix = "assets/catalog/products/";
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

        if (!IsSafeCatalogAssetPath(normalized))
        {
            AddError(errors, fieldName, $"{fieldName} must be a safe relative catalog asset path.");
        }

        return normalized;
    }

    public static bool IsSafeCatalogAssetPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var trimmed = path.Trim();

        if (!string.Equals(trimmed, path, StringComparison.Ordinal)
            || trimmed[0] == '/'
            || trimmed[0] == '\\'
            || trimmed.Contains('\\')
            || trimmed.Contains('?')
            || trimmed.Contains('#')
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
