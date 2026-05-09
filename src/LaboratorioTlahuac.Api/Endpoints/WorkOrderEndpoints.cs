using LaboratorioTlahuac.Application.WorkOrders;
using LaboratorioTlahuac.Domain.Security;

namespace LaboratorioTlahuac.Api.Endpoints;

public static class WorkOrderEndpoints
{
    public static IEndpointRouteBuilder MapWorkOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/work-orders")
            .WithTags("Work orders");

        group.MapGet(
                "",
                async (
                    string? search,
                    Guid? customerId,
                    Guid? internalDoctorId,
                    string? status,
                    DateOnly? receivedDateFrom,
                    DateOnly? receivedDateTo,
                    DateOnly? deliveryDateFrom,
                    DateOnly? deliveryDateTo,
                    bool? includeCancelled,
                    int? page,
                    int? pageSize,
                    IWorkOrderService workOrderService,
                    CancellationToken cancellationToken) =>
                    ToResult(await workOrderService.ListAsync(
                        new WorkOrderListQuery(
                            search,
                            customerId,
                            internalDoctorId,
                            status,
                            receivedDateFrom,
                            receivedDateTo,
                            deliveryDateFrom,
                            deliveryDateTo,
                            includeCancelled,
                            page,
                            pageSize),
                        cancellationToken)))
            .RequireAuthorization(Permissions.OrdersView)
            .WithName("WorkOrdersList");

        group.MapGet(
                "/statuses",
                (IWorkOrderService workOrderService) => Results.Ok(workOrderService.GetStatuses()))
            .RequireAuthorization(Permissions.OrdersView)
            .WithName("WorkOrdersStatuses");

        group.MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IWorkOrderService workOrderService,
                    CancellationToken cancellationToken) =>
                    ToResult(await workOrderService.GetByIdAsync(id, cancellationToken)))
            .RequireAuthorization(Permissions.OrdersView)
            .WithName("WorkOrdersGetById");

        group.MapPost(
                "",
                async (
                    WorkOrderUpsertRequest request,
                    IWorkOrderService workOrderService,
                    CancellationToken cancellationToken) =>
                    ToCreatedResult(await workOrderService.CreateAsync(request, cancellationToken)))
            .RequireAuthorization(Permissions.OrdersCreate)
            .WithName("WorkOrdersCreate");

        group.MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    WorkOrderUpsertRequest request,
                    IWorkOrderService workOrderService,
                    CancellationToken cancellationToken) =>
                    ToResult(await workOrderService.UpdateAsync(id, request, cancellationToken)))
            .RequireAuthorization(Permissions.OrdersEdit)
            .WithName("WorkOrdersUpdate");

        group.MapPatch(
                "/{id:guid}/status",
                async (
                    Guid id,
                    WorkOrderChangeStatusRequest request,
                    IWorkOrderService workOrderService,
                    CancellationToken cancellationToken) =>
                    ToResult(await workOrderService.ChangeStatusAsync(id, request, cancellationToken)))
            .RequireAuthorization(Permissions.OrdersChangeStatus)
            .WithName("WorkOrdersChangeStatus");

        return endpoints;
    }

    private static IResult ToCreatedResult(WorkOrderServiceResult<WorkOrderDetailResponse> result)
    {
        return result.Status == WorkOrderServiceStatus.Success && result.Value is not null
            ? Results.Created($"/api/work-orders/{result.Value.Id}", result.Value)
            : ToResult(result);
    }

    private static IResult ToResult<T>(WorkOrderServiceResult<T> result)
    {
        return result.Status switch
        {
            WorkOrderServiceStatus.Success when result.Value is not null => Results.Ok(result.Value),
            WorkOrderServiceStatus.ValidationError => Results.ValidationProblem(result.Errors),
            WorkOrderServiceStatus.NotFound => Results.Problem(
                title: result.Message ?? "Resource was not found.",
                statusCode: StatusCodes.Status404NotFound),
            WorkOrderServiceStatus.Conflict => Results.Problem(
                title: result.Message ?? "The request conflicts with the current state.",
                statusCode: StatusCodes.Status409Conflict),
            _ => Results.Problem(
                title: "Unexpected work order service result.",
                statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}
