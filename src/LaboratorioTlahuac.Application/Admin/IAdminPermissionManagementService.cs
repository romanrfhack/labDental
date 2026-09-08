namespace LaboratorioTlahuac.Application.Admin;

public interface IAdminPermissionManagementService
{
    Task<AdminSecurityServiceResult<IReadOnlyCollection<AdminPermissionResponse>>> ListPermissionsAsync(
        CancellationToken cancellationToken = default);

    Task<AdminSecurityServiceResult<AdminRoleDetailResponse>> UpdateRolePermissionsAsync(
        Guid roleId,
        AdminRolePermissionsRequest request,
        CancellationToken cancellationToken = default);

    Task<AdminSecurityServiceResult<AdminUserPermissionsResponse>> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<AdminSecurityServiceResult<AdminUserPermissionsResponse>> UpdateUserPermissionOverridesAsync(
        Guid userId,
        AdminUserPermissionOverridesRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record AdminRolePermissionsRequest(IReadOnlyCollection<Guid>? PermissionIds);

public sealed record AdminUserPermissionOverrideRequest(Guid PermissionId, string? Effect);

public sealed record AdminUserPermissionOverridesRequest(
    IReadOnlyCollection<AdminUserPermissionOverrideRequest>? Overrides);

public sealed record AdminUserPermissionStateResponse(
    AdminPermissionResponse Permission,
    bool Inherited,
    bool EffectiveAllowed,
    string? OverrideEffect,
    IReadOnlyCollection<string> SourceRoles);

public sealed record AdminUserPermissionsResponse(
    Guid UserId,
    string Email,
    string FullName,
    IReadOnlyCollection<AdminRoleSummaryResponse> Roles,
    IReadOnlyCollection<AdminUserPermissionStateResponse> Permissions);
