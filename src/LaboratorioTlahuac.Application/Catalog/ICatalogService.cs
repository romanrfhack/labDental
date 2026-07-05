namespace LaboratorioTlahuac.Application.Catalog;

public interface ICatalogService
{
    Task<CatalogServiceResult<CatalogPublicResponse>> GetPublicCatalogAsync(
        CancellationToken cancellationToken = default);

    Task<CatalogServiceResult<IReadOnlyCollection<CatalogAdminSectionResponse>>> ListSectionsAsync(
        CancellationToken cancellationToken = default);

    Task<CatalogServiceResult<CatalogAdminSectionResponse>> CreateSectionAsync(
        CatalogSectionUpsertRequest request,
        CancellationToken cancellationToken = default);

    Task<CatalogServiceResult<CatalogAdminSectionResponse>> UpdateSectionAsync(
        Guid id,
        CatalogSectionUpsertRequest request,
        CancellationToken cancellationToken = default);

    Task<CatalogServiceResult<CatalogAdminSectionResponse>> UpdateSectionStatusAsync(
        Guid id,
        CatalogStatusRequest request,
        CancellationToken cancellationToken = default);

    Task<CatalogServiceResult<IReadOnlyCollection<CatalogAdminProductResponse>>> ListProductsAsync(
        CatalogProductListQuery query,
        CancellationToken cancellationToken = default);

    Task<CatalogServiceResult<CatalogAdminProductResponse>> CreateProductAsync(
        CatalogProductUpsertRequest request,
        CancellationToken cancellationToken = default);

    Task<CatalogServiceResult<CatalogAdminProductResponse>> UpdateProductAsync(
        Guid id,
        CatalogProductUpsertRequest request,
        CancellationToken cancellationToken = default);

    Task<CatalogServiceResult<CatalogAdminProductResponse>> UpdateProductStatusAsync(
        Guid id,
        CatalogStatusRequest request,
        CancellationToken cancellationToken = default);

    Task<CatalogServiceResult<CatalogAdminProductResponse>> UpdateProductPriceAsync(
        Guid id,
        CatalogPriceRequest request,
        CancellationToken cancellationToken = default);
}
