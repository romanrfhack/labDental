namespace LaboratorioTlahuac.Application.Catalog;

public sealed record CatalogPublicResponse(
    IReadOnlyCollection<CatalogPublicSectionResponse> Sections);

public sealed record CatalogPublicSectionResponse(
    string Key,
    string Name,
    string? Description,
    string? ImagePath,
    string? AltText,
    IReadOnlyCollection<CatalogPublicProductResponse> Products);

public sealed record CatalogPublicProductResponse(
    string Key,
    string Name,
    string? Description,
    decimal PriceAmount,
    string Currency,
    string? ImagePath,
    string? AltText);

public sealed record CatalogProductListQuery(Guid? SectionId);

public sealed record CatalogSectionUpsertRequest(
    string? Key,
    string? Name,
    string? Description,
    string? ImagePath,
    string? AltText,
    int SortOrder,
    bool? IsActive);

public sealed record CatalogProductUpsertRequest(
    Guid CatalogSectionId,
    string? Key,
    string? Name,
    string? Description,
    decimal PriceAmount,
    string? Currency,
    string? ImagePath,
    string? AltText,
    int SortOrder,
    bool? IsActive);

public sealed record CatalogStatusRequest(bool IsActive);

public sealed record CatalogPriceRequest(decimal PriceAmount, string? Currency);

public sealed record CatalogAdminSectionResponse(
    Guid Id,
    string Key,
    string Name,
    string? Description,
    string? ImagePath,
    string? AltText,
    int SortOrder,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record CatalogAdminProductResponse(
    Guid Id,
    Guid CatalogSectionId,
    string CatalogSectionKey,
    string CatalogSectionName,
    string Key,
    string Name,
    string? Description,
    decimal PriceAmount,
    string Currency,
    string? ImagePath,
    string? AltText,
    int SortOrder,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
