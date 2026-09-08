namespace LaboratorioTlahuac.Application.Admin;

public interface IAdminPermissionManagementService
{
    Task<AdminSecurityServiceResult<IReadOnlyCollection<AdminPermissionResponse>>> ListPermissionsAsync(
        CancellationToken cancellationToken = default);

    Task<AdminSecurityServiceResult<AdminRoleDetailResponse>> UpdateRolePermissionsAsync(
        Guid roleId,
        AdminRolePermissionsRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record AdminRolePermissionsRequest(IReadOnlyCollection<Guid>? PermissionIds);
