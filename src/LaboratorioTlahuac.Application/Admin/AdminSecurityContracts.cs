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
    DateTimeOffset? LastLoginAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record AdminRoleListItemResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsSystem,
    int UserCount,
    int PermissionCount,
    IReadOnlyCollection<AdminPermissionResponse> Permissions);

public sealed record AdminRoleDetailResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsSystem,
    int UserCount,
    int ActiveUserCount,
    IReadOnlyCollection<AdminPermissionResponse> Permissions);
