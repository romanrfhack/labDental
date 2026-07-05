namespace LaboratorioTlahuac.Domain.Catalog.Entities;

public sealed class CatalogProduct
{
    public const int KeyMaxLength = 120;
    public const int NameMaxLength = 150;
    public const int DescriptionMaxLength = 1000;
    public const int CurrencyMaxLength = 3;
    public const int ImagePathMaxLength = 300;
    public const int AltTextMaxLength = 200;
    public const string DefaultCurrency = "MXN";

    private CatalogProduct()
    {
    }

    private CatalogProduct(
        Guid id,
        Guid catalogSectionId,
        string key,
        string name,
        string? description,
        decimal priceAmount,
        string? currency,
        string? imagePath,
        string? altText,
        int sortOrder,
        bool isActive,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        CatalogSectionId = catalogSectionId;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;

        SetDetails(
            catalogSectionId,
            key,
            name,
            description,
            priceAmount,
            currency,
            imagePath,
            altText,
            sortOrder,
            isActive);
    }

    public Guid Id { get; private set; }

    public Guid CatalogSectionId { get; private set; }

    public string Key { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public decimal PriceAmount { get; private set; }

    public string Currency { get; private set; } = DefaultCurrency;

    public string? ImagePath { get; private set; }

    public string? AltText { get; private set; }

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public CatalogSection? CatalogSection { get; private set; }

    public static CatalogProduct Create(
        Guid catalogSectionId,
        string key,
        string name,
        string? description,
        decimal priceAmount,
        string? currency,
        string? imagePath,
        string? altText,
        int sortOrder,
        bool isActive,
        DateTimeOffset createdAtUtc)
    {
        return new CatalogProduct(
            Guid.NewGuid(),
            catalogSectionId,
            key,
            name,
            description,
            priceAmount,
            currency,
            imagePath,
            altText,
            sortOrder,
            isActive,
            createdAtUtc);
    }

    public void Update(
        Guid catalogSectionId,
        string key,
        string name,
        string? description,
        decimal priceAmount,
        string? currency,
        string? imagePath,
        string? altText,
        int sortOrder,
        bool isActive,
        DateTimeOffset updatedAtUtc)
    {
        SetDetails(
            catalogSectionId,
            key,
            name,
            description,
            priceAmount,
            currency,
            imagePath,
            altText,
            sortOrder,
            isActive);
        Touch(updatedAtUtc);
    }

    public void SetStatus(bool isActive, DateTimeOffset updatedAtUtc)
    {
        IsActive = isActive;
        Touch(updatedAtUtc);
    }

    public void UpdatePrice(decimal priceAmount, string? currency, DateTimeOffset updatedAtUtc)
    {
        SetPrice(priceAmount, currency);
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
        Guid catalogSectionId,
        string key,
        string name,
        string? description,
        decimal priceAmount,
        string? currency,
        string? imagePath,
        string? altText,
        int sortOrder,
        bool isActive)
    {
        if (catalogSectionId == Guid.Empty)
        {
            throw new ArgumentException("Catalog section id is required.", nameof(catalogSectionId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        CatalogSectionId = catalogSectionId;
        Key = key.Trim();
        Name = name.Trim();
        Description = TrimOptional(description);
        SetPrice(priceAmount, currency);
        ImagePath = TrimOptional(imagePath);
        AltText = TrimOptional(altText);
        SortOrder = sortOrder;
        IsActive = isActive;
    }

    private void SetPrice(decimal priceAmount, string? currency)
    {
        if (priceAmount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(priceAmount), "Price cannot be negative.");
        }

        PriceAmount = priceAmount;
        Currency = string.IsNullOrWhiteSpace(currency) ? DefaultCurrency : currency.Trim().ToUpperInvariant();
    }

    private void Touch(DateTimeOffset updatedAtUtc)
    {
        UpdatedAtUtc = updatedAtUtc;
    }

    private static string? TrimOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
