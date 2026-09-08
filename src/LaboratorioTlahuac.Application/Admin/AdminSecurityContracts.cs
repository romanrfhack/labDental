namespace LaboratorioTlahuac.Application.Admin;

public sealed record AdminUserListQuery(
    string? Search,
    bool? IsActive,
    Guid? RoleId,
    int? Page,
    int? PageSize);

public sealed record AdminUserCreateRequest(
    string? Email,
    string? FullName,
    string? TemporaryPassword,
    IReadOnlyCollection<Guid>? RoleIds);

public sealed record AdminUserUpdateRequest(
    string? Email,
    string? FullName);

public sealed record AdminUserStatusRequest(bool IsActive);

public sealed record AdminUserRolesRequest(IReadOnlyCollection<Guid>? RoleIds);

public sealed record AdminUserTemporaryPasswordRequest(string? TemporaryPassword);

public sealed record AdminUserPermissionOverrideRequest(Guid PermissionId, string? Effect);

public sealed record AdminUserPermissionsRequest(
    IReadOnlyCollection<AdminUserPermissionOverrideRequest>? Overrides);

public sealed record AdminRolePermissionsRequest(IReadOnlyCollection<Guid>? PermissionIds);

public sealed record AdminPagedResponse<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record AdminRoleSummaryResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsSystem);

public sealed record AdminPermissionResponse(
    Guid Id,
    string Key,
    string Description);

public sealed record AdminUserPermissionResponse(
    Guid Id,
    string Key,
    string Description,
    bool Inherited,
    bool Effective,
    string? OverrideEffect,
    IReadOnlyCollection<string> SourceRoles);

public sealed record AdminUserListItemResponse(
    Guid Id,
    string Email,
    string FullName,
    bool IsActive,
    IReadOnlyCollection<AdminRoleSummaryResponse> Roles,
    DateTimeOffset? LastLoginAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record AdminUserDetailResponse(
    Guid Id,
    string Email,
    string FullName,
    bool IsActive,
    IReadOnlyCollection<AdminRoleSummaryResponse> Roles,
    bool IsPermissionOverrideEditingLocked,
    IReadOnlyCollection<AdminUserPermissionResponse> Permissions,
    DateTimeOffset? LastLoginAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record AdminRoleListItemResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsSystem,
    bool IsPermissionEditingLocked,
    int UserCount,
    int PermissionCount,
    IReadOnlyCollection<AdminPermissionResponse> Permissions);

public sealed record AdminRoleDetailResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsSystem,
    bool IsPermissionEditingLocked,
    int UserCount,
    int ActiveUserCount,
    IReadOnlyCollection<AdminPermissionResponse> Permissions,
    IReadOnlyCollection<AdminPermissionResponse> AvailablePermissions);
