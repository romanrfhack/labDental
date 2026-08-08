namespace LaboratorioTlahuac.Infrastructure.Catalog;

internal static class CatalogImageFileName
{
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.Ordinal) { ".webp", ".jpg", ".jpeg", ".png" };

    public static bool IsGeneratedName(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName)
            || !string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal)
            || fileName.Contains('/')
            || fileName.Contains('\\')
            || fileName.Contains('?')
            || fileName.Contains('#'))
        {
            return false;
        }

        var extension = Path.GetExtension(fileName);

        if (!AllowedExtensions.Contains(extension))
        {
            return false;
        }

        var stem = fileName[..^extension.Length];

        return stem.Length == 32
            && stem.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');
    }

    public static string Create(string extension)
    {
        return $"{Guid.NewGuid():N}{extension}";
    }

    public static string? GetContentType(string fileName)
    {
        return Path.GetExtension(fileName) switch
        {
            ".webp" => "image/webp",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => null
        };
    }
}
