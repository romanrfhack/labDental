namespace LaboratorioTlahuac.Application.Admin;

public interface IAdminSecurityService
{
    Task<AdminSecurityServiceResult<AdminPagedResponse<AdminUserListItemResponse>>> ListUsersAsync(
        AdminUserListQuery query,
        CancellationToken cancellationToken = default);

    Task<AdminSecurityServiceResult<AdminUserDetailResponse>> GetUserByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<AdminSecurityServiceResult<AdminUserDetailResponse>> CreateUserAsync(
        AdminUserCreateRequest request,
        CancellationToken cancellationToken = default);

    Task<AdminSecurityServiceResult<AdminUserDetailResponse>> UpdateUserAsync(
        Guid id,
        AdminUserUpdateRequest request,
        CancellationToken cancellationToken = default);

    Task<AdminSecurityServiceResult<AdminUserDetailResponse>> UpdateUserStatusAsync(
        Guid id,
        AdminUserStatusRequest request,
        CancellationToken cancellationToken = default);

    Task<AdminSecurityServiceResult<AdminUserDetailResponse>> AssignUserRolesAsync(
        Guid id,
        AdminUserRolesRequest request,
        CancellationToken cancellationToken = default);

    Task<AdminSecurityServiceResult<AdminUserDetailResponse>> SetTemporaryPasswordAsync(
        Guid id,
        AdminUserTemporaryPasswordRequest request,
        CancellationToken cancellationToken = default);

    Task<AdminSecurityServiceResult<IReadOnlyCollection<AdminRoleListItemResponse>>> ListRolesAsync(
        CancellationToken cancellationToken = default);

    Task<AdminSecurityServiceResult<AdminRoleDetailResponse>> GetRoleByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
