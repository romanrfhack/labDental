using LaboratorioTlahuac.Application.Admin;
using LaboratorioTlahuac.Domain.Security;

namespace LaboratorioTlahuac.Api.Endpoints;

public static class AdminSecurityEndpoints
{
    public static IEndpointRouteBuilder MapAdminSecurityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/admin")
            .WithTags("Admin");

        group.MapGet(
                "/users",
                async (
                    string? search,
                    bool? isActive,
                    Guid? roleId,
                    int? page,
                    int? pageSize,
                    IAdminSecurityService adminSecurityService,
                    CancellationToken cancellationToken) =>
                    ToResult(await adminSecurityService.ListUsersAsync(
                        new AdminUserListQuery(search, isActive, roleId, page, pageSize),
                        cancellationToken)))
            .RequireAuthorization(Permissions.UsersManage)
            .WithName("AdminUsersList");

        group.MapGet(
                "/users/{id:guid}",
                async (
                    Guid id,
                    IAdminSecurityService adminSecurityService,
                    CancellationToken cancellationToken) =>
                    ToResult(await adminSecurityService.GetUserByIdAsync(id, cancellationToken)))
            .RequireAuthorization(Permissions.UsersManage)
            .WithName("AdminUsersGetById");

        group.MapPost(
                "/users",
                async (
                    AdminUserCreateRequest request,
                    IAdminSecurityService adminSecurityService,
                    CancellationToken cancellationToken) =>
                    ToCreatedResult(await adminSecurityService.CreateUserAsync(request, cancellationToken)))
            .RequireAuthorization(Permissions.UsersManage)
            .WithName("AdminUsersCreate");

        group.MapPut(
                "/users/{id:guid}",
                async (
                    Guid id,
                    AdminUserUpdateRequest request,
                    IAdminSecurityService adminSecurityService,
                    CancellationToken cancellationToken) =>
                    ToResult(await adminSecurityService.UpdateUserAsync(id, request, cancellationToken)))
            .RequireAuthorization(Permissions.UsersManage)
            .WithName("AdminUsersUpdate");

        group.MapPatch(
                "/users/{id:guid}/status",
                async (
                    Guid id,
                    AdminUserStatusRequest request,
                    IAdminSecurityService adminSecurityService,
                    CancellationToken cancellationToken) =>
                    ToResult(await adminSecurityService.UpdateUserStatusAsync(id, request, cancellationToken)))
            .RequireAuthorization(Permissions.UsersManage)
            .WithName("AdminUsersUpdateStatus");

        group.MapPatch(
                "/users/{id:guid}/roles",
                async (
                    Guid id,
                    AdminUserRolesRequest request,
                    IAdminSecurityService adminSecurityService,
                    CancellationToken cancellationToken) =>
                    ToResult(await adminSecurityService.AssignUserRolesAsync(id, request, cancellationToken)))
            .RequireAuthorization(Permissions.UsersManage)
            .WithName("AdminUsersAssignRoles");

        group.MapGet(
                "/users/{id:guid}/permissions",
                async (
                    Guid id,
                    IAdminPermissionManagementService permissionManagementService,
                    CancellationToken cancellationToken) =>
                    ToResult(await permissionManagementService.GetUserPermissionsAsync(id, cancellationToken)))
            .RequireAuthorization(Permissions.UsersManage)
            .WithName("AdminUsersGetPermissions");

        group.MapPatch(
                "/users/{id:guid}/permissions",
                async (
                    Guid id,
                    AdminUserPermissionOverridesRequest request,
                    IAdminPermissionManagementService permissionManagementService,
                    CancellationToken cancellationToken) =>
                    ToResult(await permissionManagementService.UpdateUserPermissionOverridesAsync(
                        id,
                        request,
                        cancellationToken)))
            .RequireAuthorization(Permissions.UsersManage)
            .WithName("AdminUsersUpdatePermissionOverrides");

        group.MapPost(
                "/users/{id:guid}/temporary-password",
                async (
                    Guid id,
                    AdminUserTemporaryPasswordRequest request,
                    IAdminSecurityService adminSecurityService,
                    CancellationToken cancellationToken) =>
                    ToResult(await adminSecurityService.SetTemporaryPasswordAsync(id, request, cancellationToken)))
            .RequireAuthorization(Permissions.UsersManage)
            .WithName("AdminUsersSetTemporaryPassword");

        group.MapGet(
                "/roles",
                async (
                    IAdminSecurityService adminSecurityService,
                    CancellationToken cancellationToken) =>
                    ToResult(await adminSecurityService.ListRolesAsync(cancellationToken)))
            .RequireAuthorization(Permissions.RolesManage)
            .WithName("AdminRolesList");

        group.MapGet(
                "/roles/{id:guid}",
                async (
                    Guid id,
                    IAdminSecurityService adminSecurityService,
                    CancellationToken cancellationToken) =>
                    ToResult(await adminSecurityService.GetRoleByIdAsync(id, cancellationToken)))
            .RequireAuthorization(Permissions.RolesManage)
            .WithName("AdminRolesGetById");

        group.MapGet(
                "/permissions",
                async (
                    IAdminPermissionManagementService permissionManagementService,
                    CancellationToken cancellationToken) =>
                    ToResult(await permissionManagementService.ListPermissionsAsync(cancellationToken)))
            .RequireAuthorization(Permissions.RolesManage)
            .WithName("AdminPermissionsList");

        group.MapPatch(
                "/roles/{id:guid}/permissions",
                async (
                    Guid id,
                    AdminRolePermissionsRequest request,
                    IAdminPermissionManagementService permissionManagementService,
                    CancellationToken cancellationToken) =>
                    ToResult(await permissionManagementService.UpdateRolePermissionsAsync(
                        id,
                        request,
                        cancellationToken)))
            .RequireAuthorization(Permissions.RolesManage)
            .WithName("AdminRolesUpdatePermissions");

        return endpoints;
    }

    private static IResult ToCreatedResult(AdminSecurityServiceResult<AdminUserDetailResponse> result)
    {
        return result.Status == AdminSecurityServiceStatus.Success && result.Value is not null
            ? Results.Created($"/api/admin/users/{result.Value.Id}", result.Value)
            : ToResult(result);
    }

    private static IResult ToResult<T>(AdminSecurityServiceResult<T> result)
    {
        return result.Status switch
        {
            AdminSecurityServiceStatus.Success when result.Value is not null => Results.Ok(result.Value),
            AdminSecurityServiceStatus.ValidationError => Results.ValidationProblem(result.Errors),
            AdminSecurityServiceStatus.NotFound => Results.Problem(
                title: result.Message ?? "Resource was not found.",
                statusCode: StatusCodes.Status404NotFound),
            AdminSecurityServiceStatus.Conflict => Results.Problem(
                title: result.Message ?? "The request conflicts with the current state.",
                statusCode: StatusCodes.Status409Conflict),
            _ => Results.Problem(
                title: "Unexpected admin security service result.",
                statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}
