using Microsoft.EntityFrameworkCore;
using LaboratorioTlahuac.Application.Abstractions.Time;
using LaboratorioTlahuac.Application.Catalog;
using LaboratorioTlahuac.Domain.Catalog.Entities;
using LaboratorioTlahuac.Infrastructure.Persistence;

namespace LaboratorioTlahuac.Infrastructure.Catalog;

public sealed class CatalogService(
    LaboratorioTlahuacDbContext dbContext,
    IClock clock,
    ICatalogImageStorage catalogImageStorage)
    : ICatalogService
{
    public async Task<CatalogServiceResult<CatalogPublicResponse>> GetPublicCatalogAsync(
        CancellationToken cancellationToken = default)
    {
        var sections = await dbContext.CatalogSections
            .Include(section => section.Products)
            .AsNoTracking()
            .Where(section => section.IsActive)
            .OrderBy(section => section.SortOrder)
            .ThenBy(section => section.Name)
            .ToListAsync(cancellationToken);

        var response = new CatalogPublicResponse(
            sections
                .Select(MapPublicSection)
                .ToArray());

        return CatalogServiceResult.Success(response);
    }

    public async Task<CatalogServiceResult<IReadOnlyCollection<CatalogAdminSectionResponse>>> ListSectionsAsync(
        CancellationToken cancellationToken = default)
    {
        var sections = await dbContext.CatalogSections
            .AsNoTracking()
            .OrderBy(section => section.SortOrder)
            .ThenBy(section => section.Name)
            .ToListAsync(cancellationToken);

        return CatalogServiceResult.Success<IReadOnlyCollection<CatalogAdminSectionResponse>>(
            sections.Select(MapAdminSection).ToArray());
    }

    public async Task<CatalogServiceResult<CatalogAdminSectionResponse>> CreateSectionAsync(
        CatalogSectionUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        var input = ValidateSection(request);

        if (input.Errors.Count > 0 || input.Value is null)
        {
            return CatalogServiceResult.Validation<CatalogAdminSectionResponse>(input.Errors);
        }

        if (await SectionKeyExistsAsync(input.Value.Key, exceptId: null, cancellationToken))
        {
            return CatalogServiceResult.Conflict<CatalogAdminSectionResponse>("Catalog section key already exists.");
        }

        var section = CatalogSection.Create(
            input.Value.Key,
            input.Value.Name,
            input.Value.Description,
            input.Value.ImagePath,
            input.Value.AltText,
            input.Value.SortOrder,
            input.Value.IsActive,
            clock.UtcNow);

        dbContext.CatalogSections.Add(section);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CatalogServiceResult.Success(MapAdminSection(section));
    }

    public async Task<CatalogServiceResult<CatalogAdminSectionResponse>> UpdateSectionAsync(
        Guid id,
        CatalogSectionUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        var input = ValidateSection(request);

        if (input.Errors.Count > 0 || input.Value is null)
        {
            return CatalogServiceResult.Validation<CatalogAdminSectionResponse>(input.Errors);
        }

        var section = await dbContext.CatalogSections
            .FirstOrDefaultAsync(currentSection => currentSection.Id == id, cancellationToken);

        if (section is null)
        {
            return CatalogServiceResult.NotFound<CatalogAdminSectionResponse>("Catalog section was not found.");
        }

        if (await SectionKeyExistsAsync(input.Value.Key, id, cancellationToken))
        {
            return CatalogServiceResult.Conflict<CatalogAdminSectionResponse>("Catalog section key already exists.");
        }

        section.Update(
            input.Value.Key,
            input.Value.Name,
            input.Value.Description,
            input.Value.ImagePath,
            input.Value.AltText,
            input.Value.SortOrder,
            input.Value.IsActive,
            clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CatalogServiceResult.Success(MapAdminSection(section));
    }

    public async Task<CatalogServiceResult<CatalogAdminSectionResponse>> UpdateSectionStatusAsync(
        Guid id,
        CatalogStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var section = await dbContext.CatalogSections
            .FirstOrDefaultAsync(currentSection => currentSection.Id == id, cancellationToken);

        if (section is null)
        {
            return CatalogServiceResult.NotFound<CatalogAdminSectionResponse>("Catalog section was not found.");
        }

        section.SetStatus(request.IsActive, clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CatalogServiceResult.Success(MapAdminSection(section));
    }

    public async Task<CatalogServiceResult<IReadOnlyCollection<CatalogAdminProductResponse>>> ListProductsAsync(
        CatalogProductListQuery query,
        CancellationToken cancellationToken = default)
    {
        var productsQuery = dbContext.CatalogProducts
            .Include(product => product.CatalogSection)
            .AsNoTracking();

        if (query.SectionId.HasValue)
        {
            productsQuery = productsQuery.Where(product => product.CatalogSectionId == query.SectionId.Value);
        }

        var products = await productsQuery
            .OrderBy(product => product.CatalogSection!.SortOrder)
            .ThenBy(product => product.SortOrder)
            .ThenBy(product => product.Name)
            .ToListAsync(cancellationToken);

        return CatalogServiceResult.Success<IReadOnlyCollection<CatalogAdminProductResponse>>(
            products.Select(MapAdminProduct).ToArray());
    }

    public async Task<CatalogServiceResult<CatalogAdminProductResponse>> CreateProductAsync(
        CatalogProductUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        var input = ValidateProduct(request);

        if (input.Errors.Count > 0 || input.Value is null)
        {
            return CatalogServiceResult.Validation<CatalogAdminProductResponse>(input.Errors);
        }

        if (!await SectionExistsAsync(input.Value.CatalogSectionId, cancellationToken))
        {
            return CatalogServiceResult.NotFound<CatalogAdminProductResponse>("Catalog section was not found.");
        }

        if (await ProductKeyExistsAsync(input.Value.Key, exceptId: null, cancellationToken))
        {
            return CatalogServiceResult.Conflict<CatalogAdminProductResponse>("Catalog product key already exists.");
        }

        var product = CatalogProduct.Create(
            input.Value.CatalogSectionId,
            input.Value.Key,
            input.Value.Name,
            input.Value.Description,
            input.Value.PriceAmount,
            input.Value.Currency,
            input.Value.ImagePath,
            input.Value.AltText,
            input.Value.SortOrder,
            input.Value.IsActive,
            clock.UtcNow);

        dbContext.CatalogProducts.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await LoadSavedProductAsync(product.Id, cancellationToken);
    }

    public async Task<CatalogServiceResult<CatalogAdminProductResponse>> UpdateProductAsync(
        Guid id,
        CatalogProductUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        var input = ValidateProduct(request);

        if (input.Errors.Count > 0 || input.Value is null)
        {
            return CatalogServiceResult.Validation<CatalogAdminProductResponse>(input.Errors);
        }

        var product = await dbContext.CatalogProducts
            .FirstOrDefaultAsync(currentProduct => currentProduct.Id == id, cancellationToken);

        if (product is null)
        {
            return CatalogServiceResult.NotFound<CatalogAdminProductResponse>("Catalog product was not found.");
        }

        if (!await SectionExistsAsync(input.Value.CatalogSectionId, cancellationToken))
        {
            return CatalogServiceResult.NotFound<CatalogAdminProductResponse>("Catalog section was not found.");
        }

        if (await ProductKeyExistsAsync(input.Value.Key, id, cancellationToken))
        {
            return CatalogServiceResult.Conflict<CatalogAdminProductResponse>("Catalog product key already exists.");
        }

        product.Update(
            input.Value.CatalogSectionId,
            input.Value.Key,
            input.Value.Name,
            input.Value.Description,
            input.Value.PriceAmount,
            input.Value.Currency,
            input.Value.ImagePath,
            input.Value.AltText,
            input.Value.SortOrder,
            input.Value.IsActive,
            clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await LoadSavedProductAsync(product.Id, cancellationToken);
    }

    public async Task<CatalogServiceResult<CatalogAdminProductResponse>> UpdateProductStatusAsync(
        Guid id,
        CatalogStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await dbContext.CatalogProducts
            .FirstOrDefaultAsync(currentProduct => currentProduct.Id == id, cancellationToken);

        if (product is null)
        {
            return CatalogServiceResult.NotFound<CatalogAdminProductResponse>("Catalog product was not found.");
        }

        product.SetStatus(request.IsActive, clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await LoadSavedProductAsync(product.Id, cancellationToken);
    }

    public async Task<CatalogServiceResult<CatalogAdminProductResponse>> UpdateProductPriceAsync(
        Guid id,
        CatalogPriceRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var currency = NormalizeCurrency(errors, nameof(request.Currency), request.Currency);

        if (request.PriceAmount < 0)
        {
            AddError(errors, nameof(request.PriceAmount), "PriceAmount cannot be negative.");
        }

        if (errors.Count > 0)
        {
            return CatalogServiceResult.Validation<CatalogAdminProductResponse>(errors);
        }

        var product = await dbContext.CatalogProducts
            .FirstOrDefaultAsync(currentProduct => currentProduct.Id == id, cancellationToken);

        if (product is null)
        {
            return CatalogServiceResult.NotFound<CatalogAdminProductResponse>("Catalog product was not found.");
        }

        product.UpdatePrice(request.PriceAmount, currency, clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await LoadSavedProductAsync(product.Id, cancellationToken);
    }

    public async Task<CatalogServiceResult<CatalogAdminProductResponse>> UploadProductImageAsync(
        Guid id,
        CatalogImageUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await dbContext.CatalogProducts
            .FirstOrDefaultAsync(currentProduct => currentProduct.Id == id, cancellationToken);

        if (product is null)
        {
            return CatalogServiceResult.NotFound<CatalogAdminProductResponse>("Catalog product was not found.");
        }

        var storedImage = await catalogImageStorage.StoreAsync(request, cancellationToken);

        if (storedImage.Status == CatalogImageStoreStatus.ValidationError)
        {
            return CatalogServiceResult.Validation<CatalogAdminProductResponse>(storedImage.Errors);
        }

        if (storedImage.Status == CatalogImageStoreStatus.PayloadTooLarge)
        {
            return CatalogServiceResult.PayloadTooLarge<CatalogAdminProductResponse>(
                "Catalog images cannot exceed 2 MB.");
        }

        if (storedImage.Status == CatalogImageStoreStatus.ServiceUnavailable
            || string.IsNullOrEmpty(storedImage.FileName))
        {
            return CatalogServiceResult.ServiceUnavailable<CatalogAdminProductResponse>(
                "Catalog image storage is unavailable.");
        }

        var imagePath = $"/api/catalog/images/{storedImage.FileName}";
        product.UpdateImage(imagePath, clock.UtcNow);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await catalogImageStorage.TryDeleteAsync(storedImage.FileName, CancellationToken.None);
            throw;
        }

        return await LoadSavedProductAsync(product.Id, cancellationToken);
    }

    public async Task<CatalogServiceResult<CatalogAdminProductResponse>> ClearProductImageAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var product = await dbContext.CatalogProducts
            .FirstOrDefaultAsync(currentProduct => currentProduct.Id == id, cancellationToken);

        if (product is null)
        {
            return CatalogServiceResult.NotFound<CatalogAdminProductResponse>("Catalog product was not found.");
        }

        product.UpdateImage(null, clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await LoadSavedProductAsync(product.Id, cancellationToken);
    }

    public async Task<CatalogServiceResult<CatalogImageContent>> GetCatalogImageAsync(
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var image = await catalogImageStorage.OpenReadAsync(fileName, cancellationToken);

        return image is null
            ? CatalogServiceResult.NotFound<CatalogImageContent>("Catalog image was not found.")
            : CatalogServiceResult.Success(image);
    }

    private async Task<CatalogServiceResult<CatalogAdminProductResponse>> LoadSavedProductAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var product = await dbContext.CatalogProducts
            .Include(currentProduct => currentProduct.CatalogSection)
            .AsNoTracking()
            .FirstAsync(currentProduct => currentProduct.Id == id, cancellationToken);

        return CatalogServiceResult.Success(MapAdminProduct(product));
    }

    private async Task<bool> SectionKeyExistsAsync(
        string key,
        Guid? exceptId,
        CancellationToken cancellationToken)
    {
        return await dbContext.CatalogSections
            .AsNoTracking()
            .AnyAsync(
                section => section.Key == key && (!exceptId.HasValue || section.Id != exceptId.Value),
                cancellationToken);
    }

    private async Task<bool> ProductKeyExistsAsync(
        string key,
        Guid? exceptId,
        CancellationToken cancellationToken)
    {
        return await dbContext.CatalogProducts
            .AsNoTracking()
            .AnyAsync(
                product => product.Key == key && (!exceptId.HasValue || product.Id != exceptId.Value),
                cancellationToken);
    }

    private async Task<bool> SectionExistsAsync(Guid sectionId, CancellationToken cancellationToken)
    {
        return await dbContext.CatalogSections
            .AsNoTracking()
            .AnyAsync(section => section.Id == sectionId, cancellationToken);
    }

    private static ValidationResult<CatalogSectionInput> ValidateSection(CatalogSectionUpsertRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var key = NormalizeAndValidateKey(errors, nameof(request.Key), request.Key);
        var name = NormalizeAndValidateRequiredText(
            errors,
            nameof(request.Name),
            request.Name,
            CatalogSection.NameMaxLength);
        var description = NormalizeAndValidateMax(
            errors,
            nameof(request.Description),
            request.Description,
            CatalogSection.DescriptionMaxLength);
        var imagePath = CatalogImagePathValidator.NormalizeAndValidate(errors, nameof(request.ImagePath), request.ImagePath);
        var altText = NormalizeAndValidateMax(
            errors,
            nameof(request.AltText),
            request.AltText,
            CatalogSection.AltTextMaxLength);

        return errors.Count > 0 || key is null || name is null
            ? new ValidationResult<CatalogSectionInput>(errors, null)
            : new ValidationResult<CatalogSectionInput>(
                errors,
                new CatalogSectionInput(
                    key,
                    name,
                    description,
                    imagePath,
                    altText,
                    request.SortOrder,
                    request.IsActive ?? true));
    }

    private static ValidationResult<CatalogProductInput> ValidateProduct(CatalogProductUpsertRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var key = NormalizeAndValidateKey(errors, nameof(request.Key), request.Key);
        var name = NormalizeAndValidateRequiredText(
            errors,
            nameof(request.Name),
            request.Name,
            CatalogProduct.NameMaxLength);
        var description = NormalizeAndValidateMax(
            errors,
            nameof(request.Description),
            request.Description,
            CatalogProduct.DescriptionMaxLength);
        var currency = NormalizeCurrency(errors, nameof(request.Currency), request.Currency);
        var imagePath = CatalogImagePathValidator.NormalizeAndValidate(errors, nameof(request.ImagePath), request.ImagePath);
        var altText = NormalizeAndValidateMax(
            errors,
            nameof(request.AltText),
            request.AltText,
            CatalogProduct.AltTextMaxLength);

        if (request.CatalogSectionId == Guid.Empty)
        {
            AddError(errors, nameof(request.CatalogSectionId), "CatalogSectionId is required.");
        }

        if (request.PriceAmount < 0)
        {
            AddError(errors, nameof(request.PriceAmount), "PriceAmount cannot be negative.");
        }

        return errors.Count > 0 || key is null || name is null
            ? new ValidationResult<CatalogProductInput>(errors, null)
            : new ValidationResult<CatalogProductInput>(
                errors,
                new CatalogProductInput(
                    request.CatalogSectionId,
                    key,
                    name,
                    description,
                    request.PriceAmount,
                    currency,
                    imagePath,
                    altText,
                    request.SortOrder,
                    request.IsActive ?? true));
    }

    private static string? NormalizeAndValidateKey(
        IDictionary<string, string[]> errors,
        string fieldName,
        string? value)
    {
        var normalized = NormalizeOptional(value);

        if (normalized is null)
        {
            AddError(errors, fieldName, $"{fieldName} is required.");
            return null;
        }

        ValidateMaxLength(errors, fieldName, normalized, CatalogSection.KeyMaxLength);

        if (!IsValidKey(normalized))
        {
            AddError(errors, fieldName, $"{fieldName} must use lowercase letters, numbers, and hyphens.");
        }

        return normalized;
    }

    private static bool IsValidKey(string value)
    {
        return value.Length > 0
            && value[0] != '-'
            && value[^1] != '-'
            && value.All(character =>
                character is >= 'a' and <= 'z'
                || character is >= '0' and <= '9'
                || character == '-');
    }

    private static string? NormalizeAndValidateRequiredText(
        IDictionary<string, string[]> errors,
        string fieldName,
        string? value,
        int maxLength)
    {
        var normalized = NormalizeOptional(value);

        if (normalized is null)
        {
            AddError(errors, fieldName, $"{fieldName} is required.");
            return null;
        }

        ValidateMaxLength(errors, fieldName, normalized, maxLength);

        return normalized;
    }

    private static string? NormalizeAndValidateMax(
        IDictionary<string, string[]> errors,
        string fieldName,
        string? value,
        int maxLength)
    {
        var normalized = NormalizeOptional(value);

        if (normalized is not null)
        {
            ValidateMaxLength(errors, fieldName, normalized, maxLength);
        }

        return normalized;
    }

    private static string NormalizeCurrency(
        IDictionary<string, string[]> errors,
        string fieldName,
        string? value)
    {
        var currency = string.IsNullOrWhiteSpace(value)
            ? CatalogProduct.DefaultCurrency
            : value.Trim().ToUpperInvariant();

        if (!string.Equals(currency, CatalogProduct.DefaultCurrency, StringComparison.Ordinal))
        {
            AddError(errors, fieldName, "Currency must be MXN.");
        }

        return currency;
    }

    private static void ValidateMaxLength(
        IDictionary<string, string[]> errors,
        string fieldName,
        string value,
        int maxLength)
    {
        if (value.Length > maxLength)
        {
            AddError(errors, fieldName, $"{fieldName} must be {maxLength} characters or fewer.");
        }
    }

    private static void AddError(IDictionary<string, string[]> errors, string fieldName, string error)
    {
        errors[fieldName] = errors.TryGetValue(fieldName, out var existingErrors)
            ? [.. existingErrors, error]
            : [error];
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static CatalogPublicSectionResponse MapPublicSection(CatalogSection section)
    {
        return new CatalogPublicSectionResponse(
            section.Key,
            section.Name,
            section.Description,
            section.ImagePath,
            section.AltText,
            section.Products
                .Where(product => product.IsActive)
                .OrderBy(product => product.SortOrder)
                .ThenBy(product => product.Name)
                .Select(product => new CatalogPublicProductResponse(
                    product.Key,
                    product.Name,
                    product.Description,
                    product.PriceAmount,
                    product.Currency,
                    product.ImagePath,
                    product.AltText))
                .ToArray());
    }

    private static CatalogAdminSectionResponse MapAdminSection(CatalogSection section)
    {
        return new CatalogAdminSectionResponse(
            section.Id,
            section.Key,
            section.Name,
            section.Description,
            section.ImagePath,
            section.AltText,
            section.SortOrder,
            section.IsActive,
            section.CreatedAtUtc,
            section.UpdatedAtUtc);
    }

    private static CatalogAdminProductResponse MapAdminProduct(CatalogProduct product)
    {
        return new CatalogAdminProductResponse(
            product.Id,
            product.CatalogSectionId,
            product.CatalogSection?.Key ?? string.Empty,
            product.CatalogSection?.Name ?? string.Empty,
            product.Key,
            product.Name,
            product.Description,
            product.PriceAmount,
            product.Currency,
            product.ImagePath,
            product.AltText,
            product.SortOrder,
            product.IsActive,
            product.CreatedAtUtc,
            product.UpdatedAtUtc);
    }

    private sealed record CatalogSectionInput(
        string Key,
        string Name,
        string? Description,
        string? ImagePath,
        string? AltText,
        int SortOrder,
        bool IsActive);

    private sealed record CatalogProductInput(
        Guid CatalogSectionId,
        string Key,
        string Name,
        string? Description,
        decimal PriceAmount,
        string Currency,
        string? ImagePath,
        string? AltText,
        int SortOrder,
        bool IsActive);

    private sealed record ValidationResult<T>(
        IReadOnlyDictionary<string, string[]> Errors,
        T? Value);
}
