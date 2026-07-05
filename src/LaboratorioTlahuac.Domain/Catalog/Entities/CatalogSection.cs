namespace LaboratorioTlahuac.Domain.Catalog.Entities;

public sealed class CatalogSection
{
    public const int KeyMaxLength = 120;
    public const int NameMaxLength = 150;
    public const int DescriptionMaxLength = 1000;
    public const int ImagePathMaxLength = 300;
    public const int AltTextMaxLength = 200;

    private CatalogSection()
    {
    }

    private CatalogSection(
        Guid id,
        string key,
        string name,
        string? description,
        string? imagePath,
        string? altText,
        int sortOrder,
        bool isActive,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;

        SetDetails(key, name, description, imagePath, altText, sortOrder, isActive);
    }

    public Guid Id { get; private set; }

    public string Key { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? ImagePath { get; private set; }

    public string? AltText { get; private set; }

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public ICollection<CatalogProduct> Products { get; private set; } = new List<CatalogProduct>();

    public static CatalogSection Create(
        string key,
        string name,
        string? description,
        string? imagePath,
        string? altText,
        int sortOrder,
        bool isActive,
        DateTimeOffset createdAtUtc)
    {
        return new CatalogSection(
            Guid.NewGuid(),
            key,
            name,
            description,
            imagePath,
            altText,
            sortOrder,
            isActive,
            createdAtUtc);
    }

    public void Update(
        string key,
        string name,
        string? description,
        string? imagePath,
        string? altText,
        int sortOrder,
        bool isActive,
        DateTimeOffset updatedAtUtc)
    {
        SetDetails(key, name, description, imagePath, altText, sortOrder, isActive);
        Touch(updatedAtUtc);
    }

    public void SetStatus(bool isActive, DateTimeOffset updatedAtUtc)
    {
        IsActive = isActive;
        Touch(updatedAtUtc);
    }

    public void BackfillSeedData(
        string? imagePath,
        string? altText,
        DateTimeOffset updatedAtUtc)
    {
        var changed = false;

        if (ImagePath is null && imagePath is not null)
        {
            ImagePath = imagePath;
            changed = true;
        }

        if (AltText is null && altText is not null)
        {
            AltText = altText;
            changed = true;
        }

        if (changed)
        {
            Touch(updatedAtUtc);
        }
    }

    private void SetDetails(
        string key,
        string name,
        string? description,
        string? imagePath,
        string? altText,
        int sortOrder,
        bool isActive)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Key = TrimRequired(key);
        Name = TrimRequired(name);
        Description = TrimOptional(description);
        ImagePath = TrimOptional(imagePath);
        AltText = TrimOptional(altText);
        SortOrder = sortOrder;
        IsActive = isActive;
    }

    private void Touch(DateTimeOffset updatedAtUtc)
    {
        UpdatedAtUtc = updatedAtUtc;
    }

    private static string TrimRequired(string value)
    {
        return value.Trim();
    }

    private static string? TrimOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
