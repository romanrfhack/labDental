using Microsoft.AspNetCore.Authorization;
using LaboratorioTlahuac.Application.Catalog;
using LaboratorioTlahuac.Domain.Security;

namespace LaboratorioTlahuac.Api.Endpoints;

public static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var publicGroup = endpoints
            .MapGroup("/api/catalog")
            .WithTags("Catalog");

        publicGroup.MapGet(
                "/public",
                async (
                    ICatalogService catalogService,
                    CancellationToken cancellationToken) =>
                    ToResult(await catalogService.GetPublicCatalogAsync(cancellationToken)))
            .WithName("CatalogPublic");

        publicGroup.MapGet(
                "/images/{fileName}",
                async (
                    string fileName,
                    HttpRequest request,
                    HttpResponse response,
                    ICatalogService catalogService,
                    CancellationToken cancellationToken) =>
                {
                    if (request.QueryString.HasValue)
                    {
                        return Results.NotFound();
                    }

                    var result = await catalogService.GetCatalogImageAsync(fileName, cancellationToken);

                    if (result.Status != CatalogServiceStatus.Success || result.Value is null)
                    {
                        return ToResult(result);
                    }

                    response.Headers.CacheControl = "public, max-age=31536000, immutable";
                    response.Headers.XContentTypeOptions = "nosniff";

                    return Results.Stream(result.Value.Content, result.Value.ContentType);
                })
            .WithName("CatalogImagePublic");

        var adminGroup = endpoints
            .MapGroup("/api/admin/catalog")
            .WithTags("AdminCatalog");

        adminGroup.MapGet(
                "/sections",
                async (
                    ICatalogService catalogService,
                    CancellationToken cancellationToken) =>
                    ToResult(await catalogService.ListSectionsAsync(cancellationToken)))
            .RequireAuthorization(RequireCatalogViewOrManage)
            .WithName("AdminCatalogSectionsList");

        adminGroup.MapPost(
                "/sections",
                async (
                    CatalogSectionUpsertRequest request,
                    ICatalogService catalogService,
                    CancellationToken cancellationToken) =>
                    ToCreatedSectionResult(await catalogService.CreateSectionAsync(request, cancellationToken)))
            .RequireAuthorization(Permissions.CatalogManage)
            .WithName("AdminCatalogSectionsCreate");

        adminGroup.MapPut(
                "/sections/{id:guid}",
                async (
                    Guid id,
                    CatalogSectionUpsertRequest request,
                    ICatalogService catalogService,
                    CancellationToken cancellationToken) =>
                    ToResult(await catalogService.UpdateSectionAsync(id, request, cancellationToken)))
            .RequireAuthorization(Permissions.CatalogManage)
            .WithName("AdminCatalogSectionsUpdate");

        adminGroup.MapPatch(
                "/sections/{id:guid}/status",
                async (
                    Guid id,
                    CatalogStatusRequest request,
                    ICatalogService catalogService,
                    CancellationToken cancellationToken) =>
                    ToResult(await catalogService.UpdateSectionStatusAsync(id, request, cancellationToken)))
            .RequireAuthorization(Permissions.CatalogManage)
            .WithName("AdminCatalogSectionsUpdateStatus");

        adminGroup.MapGet(
                "/products",
                async (
                    Guid? sectionId,
                    ICatalogService catalogService,
                    CancellationToken cancellationToken) =>
                    ToResult(await catalogService.ListProductsAsync(
                        new CatalogProductListQuery(sectionId),
                        cancellationToken)))
            .RequireAuthorization(RequireCatalogViewOrManage)
            .WithName("AdminCatalogProductsList");

        adminGroup.MapPost(
                "/products",
                async (
                    CatalogProductUpsertRequest request,
                    ICatalogService catalogService,
                    CancellationToken cancellationToken) =>
                    ToCreatedProductResult(await catalogService.CreateProductAsync(request, cancellationToken)))
            .RequireAuthorization(Permissions.CatalogManage)
            .WithName("AdminCatalogProductsCreate");

        adminGroup.MapPut(
                "/products/{id:guid}",
                async (
                    Guid id,
                    CatalogProductUpsertRequest request,
                    ICatalogService catalogService,
                    CancellationToken cancellationToken) =>
                    ToResult(await catalogService.UpdateProductAsync(id, request, cancellationToken)))
            .RequireAuthorization(Permissions.CatalogManage)
            .WithName("AdminCatalogProductsUpdate");

        adminGroup.MapPatch(
                "/products/{id:guid}/status",
                async (
                    Guid id,
                    CatalogStatusRequest request,
                    ICatalogService catalogService,
                    CancellationToken cancellationToken) =>
                    ToResult(await catalogService.UpdateProductStatusAsync(id, request, cancellationToken)))
            .RequireAuthorization(Permissions.CatalogManage)
            .WithName("AdminCatalogProductsUpdateStatus");

        adminGroup.MapPatch(
                "/products/{id:guid}/price",
                async (
                    Guid id,
                    CatalogPriceRequest request,
                    ICatalogService catalogService,
                    CancellationToken cancellationToken) =>
                    ToResult(await catalogService.UpdateProductPriceAsync(id, request, cancellationToken)))
            .RequireAuthorization(Permissions.CatalogManage)
            .WithName("AdminCatalogProductsUpdatePrice");

        adminGroup.MapPost(
                "/products/{id:guid}/image",
                UploadProductImageAsync)
            .Accepts<IFormFile>("multipart/form-data")
            .RequireAuthorization(Permissions.CatalogManage)
            .WithName("AdminCatalogProductsUploadImage");

        adminGroup.MapDelete(
                "/products/{id:guid}/image",
                async (
                    Guid id,
                    ICatalogService catalogService,
                    CancellationToken cancellationToken) =>
                    ToResult(await catalogService.ClearProductImageAsync(id, cancellationToken)))
            .RequireAuthorization(Permissions.CatalogManage)
            .WithName("AdminCatalogProductsClearImage");

        return endpoints;
    }

    private static void RequireCatalogViewOrManage(AuthorizationPolicyBuilder policy)
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.HasClaim(PermissionClaimTypes.Permission, Permissions.CatalogView)
            || context.User.HasClaim(PermissionClaimTypes.Permission, Permissions.CatalogManage));
    }

    private static IResult ToCreatedSectionResult(CatalogServiceResult<CatalogAdminSectionResponse> result)
    {
        return result.Status == CatalogServiceStatus.Success && result.Value is not null
            ? Results.Created($"/api/admin/catalog/sections/{result.Value.Id}", result.Value)
            : ToResult(result);
    }

    private static async Task<IResult> UploadProductImageAsync(
        Guid id,
        HttpRequest request,
        ICatalogService catalogService,
        CancellationToken cancellationToken)
    {
        if (!request.HasFormContentType)
        {
            return MissingFileResult();
        }

        IFormCollection form;

        try
        {
            form = await request.ReadFormAsync(cancellationToken);
        }
        catch (Exception exception) when (
            exception is InvalidDataException or IOException or BadHttpRequestException)
        {
            return MissingFileResult();
        }

        var files = form.Files.GetFiles("file");

        if (files.Count == 0)
        {
            return MissingFileResult();
        }

        if (files.Count != 1 || form.Files.Count != 1)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["file"] = ["Exactly one file part named 'file' is required."]
            });
        }

        var file = files[0];
        await using var content = file.OpenReadStream();
        var uploadRequest = new CatalogImageUploadRequest(
            file.FileName,
            file.ContentType,
            file.Length,
            content);

        return ToResult(await catalogService.UploadProductImageAsync(id, uploadRequest, cancellationToken));
    }

    private static IResult MissingFileResult()
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["file"] = ["A multipart file part named 'file' is required."]
        });
    }

    private static IResult ToCreatedProductResult(CatalogServiceResult<CatalogAdminProductResponse> result)
    {
        return result.Status == CatalogServiceStatus.Success && result.Value is not null
            ? Results.Created($"/api/admin/catalog/products/{result.Value.Id}", result.Value)
            : ToResult(result);
    }

    private static IResult ToResult<T>(CatalogServiceResult<T> result)
    {
        return result.Status switch
        {
            CatalogServiceStatus.Success when result.Value is not null => Results.Ok(result.Value),
            CatalogServiceStatus.ValidationError => Results.ValidationProblem(result.Errors),
            CatalogServiceStatus.NotFound => Results.Problem(
                title: result.Message ?? "Resource was not found.",
                statusCode: StatusCodes.Status404NotFound),
            CatalogServiceStatus.Conflict => Results.Problem(
                title: result.Message ?? "The request conflicts with the current state.",
                statusCode: StatusCodes.Status409Conflict),
            CatalogServiceStatus.PayloadTooLarge => Results.Problem(
                title: result.Message ?? "The request body is too large.",
                statusCode: StatusCodes.Status413PayloadTooLarge),
            CatalogServiceStatus.ServiceUnavailable => Results.Problem(
                title: result.Message ?? "Catalog image storage is unavailable.",
                statusCode: StatusCodes.Status503ServiceUnavailable),
            _ => Results.Problem(
                title: "Unexpected catalog service result.",
                statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}
