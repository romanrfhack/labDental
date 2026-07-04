using LaboratorioTlahuac.Application.Deliveries;
using LaboratorioTlahuac.Domain.Security;

namespace LaboratorioTlahuac.Api.Endpoints;

public static class DeliveryEndpoints
{
    public static IEndpointRouteBuilder MapDeliveryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var deliveriesGroup = endpoints
            .MapGroup("/api/deliveries")
            .WithTags("Deliveries");

        deliveriesGroup.MapGet(
                "",
                async (
                    string? status,
                    bool? assignedToMe,
                    int? page,
                    int? pageSize,
                    IDeliveryService deliveryService,
                    CancellationToken cancellationToken) =>
                    ToResult(await deliveryService.ListAsync(
                        new DeliveryListQuery(status, assignedToMe, page, pageSize),
                        cancellationToken)))
            .RequireAuthorization(Permissions.DeliveriesView)
            .WithName("DeliveriesList");

        deliveriesGroup.MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IDeliveryService deliveryService,
                    CancellationToken cancellationToken) =>
                    ToResult(await deliveryService.GetByIdAsync(id, cancellationToken)))
            .RequireAuthorization(Permissions.DeliveriesView)
            .WithName("DeliveriesGetById");

        deliveriesGroup.MapPatch(
                "/{id:guid}/assign",
                async (
                    Guid id,
                    DeliveryAssignRequest request,
                    IDeliveryService deliveryService,
                    CancellationToken cancellationToken) =>
                    ToResult(await deliveryService.AssignAsync(id, request, cancellationToken)))
            .RequireAuthorization(Permissions.DeliveriesAssign)
            .WithName("DeliveriesAssign");

        deliveriesGroup.MapPatch(
                "/{id:guid}/out-for-delivery",
                async (
                    Guid id,
                    DeliveryOutForDeliveryRequest request,
                    IDeliveryService deliveryService,
                    CancellationToken cancellationToken) =>
                    ToResult(await deliveryService.MarkOutForDeliveryAsync(id, request, cancellationToken)))
            .RequireAuthorization(Permissions.DeliveriesUpdate)
            .WithName("DeliveriesOutForDelivery");

        deliveriesGroup.MapPatch(
                "/{id:guid}/complete",
                async (
                    Guid id,
                    DeliveryCompleteRequest request,
                    IDeliveryService deliveryService,
                    CancellationToken cancellationToken) =>
                    ToResult(await deliveryService.CompleteAsync(id, request, cancellationToken)))
            .RequireAuthorization(Permissions.DeliveriesComplete)
            .WithName("DeliveriesComplete");

        deliveriesGroup.MapPatch(
                "/{id:guid}/failed",
                async (
                    Guid id,
                    DeliveryFailedRequest request,
                    IDeliveryService deliveryService,
                    CancellationToken cancellationToken) =>
                    ToResult(await deliveryService.MarkFailedAsync(id, request, cancellationToken)))
            .RequireAuthorization(Permissions.DeliveriesComplete)
            .WithName("DeliveriesFailed");

        var workOrdersGroup = endpoints
            .MapGroup("/api/work-orders")
            .WithTags("Deliveries");

        workOrdersGroup.MapGet(
                "/{workOrderId:guid}/delivery",
                async (
                    Guid workOrderId,
                    IDeliveryService deliveryService,
                    CancellationToken cancellationToken) =>
                    ToResult(await deliveryService.GetByWorkOrderIdAsync(workOrderId, cancellationToken)))
            .RequireAuthorization(Permissions.DeliveriesView)
            .WithName("WorkOrderDeliveryGet");

        workOrdersGroup.MapPost(
                "/{workOrderId:guid}/delivery",
                async (
                    Guid workOrderId,
                    DeliveryCreateRequest request,
                    IDeliveryService deliveryService,
                    CancellationToken cancellationToken) =>
                    ToCreatedResult(await deliveryService.CreateForWorkOrderAsync(
                        workOrderId,
                        request,
                        cancellationToken)))
            .RequireAuthorization(Permissions.DeliveriesAssign)
            .WithName("WorkOrderDeliveryCreate");

        return endpoints;
    }

    private static IResult ToCreatedResult(DeliveryServiceResult<DeliveryResponse> result)
    {
        return result.Status == DeliveryServiceStatus.Success && result.Value is not null
            ? Results.Created($"/api/deliveries/{result.Value.Id}", result.Value)
            : ToResult(result);
    }

    private static IResult ToResult<T>(DeliveryServiceResult<T> result)
    {
        return result.Status switch
        {
            DeliveryServiceStatus.Success when result.Value is not null => Results.Ok(result.Value),
            DeliveryServiceStatus.ValidationError => Results.ValidationProblem(result.Errors),
            DeliveryServiceStatus.NotFound => Results.Problem(
                title: result.Message ?? "Resource was not found.",
                statusCode: StatusCodes.Status404NotFound),
            DeliveryServiceStatus.Conflict => Results.Problem(
                title: result.Message ?? "The request conflicts with the current state.",
                statusCode: StatusCodes.Status409Conflict),
            DeliveryServiceStatus.Forbidden => Results.Problem(
                title: result.Message ?? "The current user cannot access this delivery.",
                statusCode: StatusCodes.Status403Forbidden),
            _ => Results.Problem(
                title: "Unexpected delivery service result.",
                statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}
