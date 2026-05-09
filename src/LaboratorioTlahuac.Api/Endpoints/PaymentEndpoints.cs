using LaboratorioTlahuac.Application.Payments;
using LaboratorioTlahuac.Domain.Security;

namespace LaboratorioTlahuac.Api.Endpoints;

public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var workOrderPayments = endpoints
            .MapGroup("/api/work-orders/{workOrderId:guid}/payments")
            .WithTags("Payments");

        workOrderPayments.MapGet(
                "",
                async (
                    Guid workOrderId,
                    bool? includeCancelled,
                    IPaymentService paymentService,
                    CancellationToken cancellationToken) =>
                    ToResult(await paymentService.ListForWorkOrderAsync(
                        workOrderId,
                        new WorkOrderPaymentListQuery(includeCancelled),
                        cancellationToken)))
            .RequireAuthorization(Permissions.PaymentsView)
            .WithName("WorkOrderPaymentsList");

        workOrderPayments.MapGet(
                "/summary",
                async (
                    Guid workOrderId,
                    IPaymentService paymentService,
                    CancellationToken cancellationToken) =>
                    ToResult(await paymentService.GetSummaryAsync(workOrderId, cancellationToken)))
            .RequireAuthorization(Permissions.PaymentsView)
            .WithName("WorkOrderPaymentsSummary");

        workOrderPayments.MapPost(
                "",
                async (
                    Guid workOrderId,
                    PaymentCreateRequest request,
                    IPaymentService paymentService,
                    CancellationToken cancellationToken) =>
                    ToCreatedResult(workOrderId, await paymentService.CreateAsync(
                        workOrderId,
                        request,
                        cancellationToken)))
            .RequireAuthorization(Permissions.PaymentsCreate)
            .WithName("WorkOrderPaymentsCreate");

        workOrderPayments.MapPatch(
                "/{paymentId:guid}/cancel",
                async (
                    Guid workOrderId,
                    Guid paymentId,
                    PaymentCancelRequest request,
                    IPaymentService paymentService,
                    CancellationToken cancellationToken) =>
                    ToResult(await paymentService.CancelAsync(
                        workOrderId,
                        paymentId,
                        request,
                        cancellationToken)))
            .RequireAuthorization(Permissions.PaymentsCancel)
            .WithName("WorkOrderPaymentsCancel");

        var payments = endpoints
            .MapGroup("/api/payments")
            .WithTags("Payments");

        payments.MapGet(
                "",
                async (
                    string? search,
                    Guid? customerId,
                    Guid? workOrderId,
                    string? method,
                    DateOnly? paymentDateFrom,
                    DateOnly? paymentDateTo,
                    bool? includeCancelled,
                    int? page,
                    int? pageSize,
                    IPaymentService paymentService,
                    CancellationToken cancellationToken) =>
                    ToResult(await paymentService.ListAsync(
                        new PaymentListQuery(
                            search,
                            customerId,
                            workOrderId,
                            method,
                            paymentDateFrom,
                            paymentDateTo,
                            includeCancelled,
                            page,
                            pageSize),
                        cancellationToken)))
            .RequireAuthorization(Permissions.PaymentsView)
            .WithName("PaymentsList");

        payments.MapGet(
                "/methods",
                (IPaymentService paymentService) => Results.Ok(paymentService.GetMethods()))
            .RequireAuthorization(Permissions.PaymentsView)
            .WithName("PaymentsMethods");

        payments.MapGet(
                "/statuses",
                (IPaymentService paymentService) => Results.Ok(paymentService.GetStatuses()))
            .RequireAuthorization(Permissions.PaymentsView)
            .WithName("PaymentsStatuses");

        return endpoints;
    }

    private static IResult ToCreatedResult(
        Guid workOrderId,
        PaymentServiceResult<PaymentMutationResponse> result)
    {
        return result.Status == PaymentServiceStatus.Success && result.Value is not null
            ? Results.Created(
                $"/api/work-orders/{workOrderId}/payments/{result.Value.Payment.Id}",
                result.Value)
            : ToResult(result);
    }

    private static IResult ToResult<T>(PaymentServiceResult<T> result)
    {
        return result.Status switch
        {
            PaymentServiceStatus.Success when result.Value is not null => Results.Ok(result.Value),
            PaymentServiceStatus.ValidationError => Results.ValidationProblem(result.Errors),
            PaymentServiceStatus.NotFound => Results.Problem(
                title: result.Message ?? "Resource was not found.",
                statusCode: StatusCodes.Status404NotFound),
            PaymentServiceStatus.Conflict => Results.Problem(
                title: result.Message ?? "The request conflicts with the current state.",
                statusCode: StatusCodes.Status409Conflict),
            _ => Results.Problem(
                title: "Unexpected payment service result.",
                statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}
