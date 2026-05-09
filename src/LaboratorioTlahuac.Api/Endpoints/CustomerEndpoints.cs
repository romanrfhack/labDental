using LaboratorioTlahuac.Application.Customers;
using LaboratorioTlahuac.Domain.Security;

namespace LaboratorioTlahuac.Api.Endpoints;

public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/customers")
            .WithTags("Customers");

        group.MapGet(
                "",
                async (
                    string? search,
                    string? type,
                    bool? isActive,
                    int? page,
                    int? pageSize,
                    ICustomerService customerService,
                    CancellationToken cancellationToken) =>
                    ToResult(await customerService.ListAsync(
                        new CustomerListQuery(search, type, isActive, page, pageSize),
                        cancellationToken)))
            .RequireAuthorization(Permissions.CustomersView)
            .WithName("CustomersList");

        group.MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    ICustomerService customerService,
                    CancellationToken cancellationToken) =>
                    ToResult(await customerService.GetByIdAsync(id, cancellationToken)))
            .RequireAuthorization(Permissions.CustomersView)
            .WithName("CustomersGetById");

        group.MapPost(
                "",
                async (
                    CustomerUpsertRequest request,
                    ICustomerService customerService,
                    CancellationToken cancellationToken) =>
                    ToCreatedResult(await customerService.CreateAsync(request, cancellationToken)))
            .RequireAuthorization(Permissions.CustomersCreate)
            .WithName("CustomersCreate");

        group.MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    CustomerUpsertRequest request,
                    ICustomerService customerService,
                    CancellationToken cancellationToken) =>
                    ToResult(await customerService.UpdateAsync(id, request, cancellationToken)))
            .RequireAuthorization(Permissions.CustomersEdit)
            .WithName("CustomersUpdate");

        group.MapPatch(
                "/{id:guid}/status",
                async (
                    Guid id,
                    CustomerStatusRequest request,
                    ICustomerService customerService,
                    CancellationToken cancellationToken) =>
                    ToResult(await customerService.UpdateStatusAsync(id, request, cancellationToken)))
            .RequireAuthorization(Permissions.CustomersEdit)
            .WithName("CustomersUpdateStatus");

        group.MapGet(
                "/{customerId:guid}/internal-doctors",
                async (
                    Guid customerId,
                    bool? isActive,
                    ICustomerService customerService,
                    CancellationToken cancellationToken) =>
                    ToResult(await customerService.ListInternalDoctorsAsync(
                        customerId,
                        new InternalDoctorListQuery(isActive),
                        cancellationToken)))
            .RequireAuthorization(Permissions.CustomersView)
            .WithName("InternalDoctorsList");

        group.MapPost(
                "/{customerId:guid}/internal-doctors",
                async (
                    Guid customerId,
                    InternalDoctorUpsertRequest request,
                    ICustomerService customerService,
                    CancellationToken cancellationToken) =>
                    ToResult(await customerService.CreateInternalDoctorAsync(customerId, request, cancellationToken)))
            .RequireAuthorization(Permissions.CustomersCreate)
            .WithName("InternalDoctorsCreate");

        group.MapPut(
                "/{customerId:guid}/internal-doctors/{doctorId:guid}",
                async (
                    Guid customerId,
                    Guid doctorId,
                    InternalDoctorUpsertRequest request,
                    ICustomerService customerService,
                    CancellationToken cancellationToken) =>
                    ToResult(await customerService.UpdateInternalDoctorAsync(
                        customerId,
                        doctorId,
                        request,
                        cancellationToken)))
            .RequireAuthorization(Permissions.CustomersEdit)
            .WithName("InternalDoctorsUpdate");

        group.MapPatch(
                "/{customerId:guid}/internal-doctors/{doctorId:guid}/status",
                async (
                    Guid customerId,
                    Guid doctorId,
                    InternalDoctorStatusRequest request,
                    ICustomerService customerService,
                    CancellationToken cancellationToken) =>
                    ToResult(await customerService.UpdateInternalDoctorStatusAsync(
                        customerId,
                        doctorId,
                        request,
                        cancellationToken)))
            .RequireAuthorization(Permissions.CustomersEdit)
            .WithName("InternalDoctorsUpdateStatus");

        return endpoints;
    }

    private static IResult ToCreatedResult(CustomerServiceResult<CustomerDetailResponse> result)
    {
        return result.Status == CustomerServiceStatus.Success && result.Value is not null
            ? Results.Created($"/api/customers/{result.Value.Id}", result.Value)
            : ToResult(result);
    }

    private static IResult ToResult<T>(CustomerServiceResult<T> result)
    {
        return result.Status switch
        {
            CustomerServiceStatus.Success when result.Value is not null => Results.Ok(result.Value),
            CustomerServiceStatus.ValidationError => Results.ValidationProblem(result.Errors),
            CustomerServiceStatus.NotFound => Results.Problem(
                title: result.Message ?? "Resource was not found.",
                statusCode: StatusCodes.Status404NotFound),
            CustomerServiceStatus.Conflict => Results.Problem(
                title: result.Message ?? "The request conflicts with the current state.",
                statusCode: StatusCodes.Status409Conflict),
            _ => Results.Problem(
                title: "Unexpected customer service result.",
                statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}
