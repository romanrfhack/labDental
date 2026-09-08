using Microsoft.EntityFrameworkCore;
using LaboratorioTlahuac.Application.Admin;
using LaboratorioTlahuac.Domain.Security;
using LaboratorioTlahuac.Domain.Security.Entities;
using LaboratorioTlahuac.Infrastructure.Persistence;

namespace LaboratorioTlahuac.Infrastructure.Admin;

public sealed class AdminPermissionManagementService(LaboratorioTlahuacDbContext dbContext)
    : IAdminPermissionManagementService
{
    private static readonly HashSet<string> ProtectedRoleNames =
    [
        SecurityTextNormalizer.NormalizeName("Admin"),
        SecurityTextNormalizer.NormalizeName("Limited QA")
    ];

    public async Task<AdminSecurityServiceResult<IReadOnlyCollection<AdminPermissionResponse>>> ListPermissionsAsync(
        CancellationToken cancellationToken = default)
    {
        var permissions = await dbContext.Permissions
            .AsNoTracking()
            .OrderBy(permission => permission.Key)
            .Select(permission => new AdminPermissionResponse(
                permission.Id,
                permission.Key,
                permission.Description))
            .ToListAsync(cancellationToken);

        return AdminSecurityServiceResult.Success<IReadOnlyCollection<AdminPermissionResponse>>(permissions);
    }

    public async Task<AdminSecurityServiceResult<AdminRoleDetailResponse>> UpdateRolePermissionsAsync(
        Guid roleId,
        AdminRolePermissionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var role = await dbContext.Roles
            .Include(currentRole => currentRole.RolePermissions)
                .ThenInclude(rolePermission => rolePermission.Permission)
            .Include(currentRole => currentRole.UserRoles)
                .ThenInclude(userRole => userRole.User)
            .FirstOrDefaultAsync(currentRole => currentRole.Id == roleId, cancellationToken);

        if (role is null)
        {
            return AdminSecurityServiceResult.NotFound<AdminRoleDetailResponse>("Role was not found.");
        }

        if (ProtectedRoleNames.Contains(role.NormalizedName))
        {
            return AdminSecurityServiceResult.Conflict<AdminRoleDetailResponse>(
                "Permissions for this protected system role cannot be changed.");
        }

        var desiredPermissionIds = (request.PermissionIds ?? Array.Empty<Guid>())
            .Where(id => id != Guid.Empty)
            .ToHashSet();

        var permissionValidation = await ValidatePermissionIdsAsync(desiredPermissionIds, cancellationToken);

        if (permissionValidation is not null)
        {
            return AdminSecurityServiceResult.Validation<AdminRoleDetailResponse>(permissionValidation);
        }

        var currentPermissionIds = role.RolePermissions
            .Select(rolePermission => rolePermission.PermissionId)
            .ToHashSet();

        dbContext.RolePermissions.RemoveRange(
            role.RolePermissions.Where(rolePermission => !desiredPermissionIds.Contains(rolePermission.PermissionId)));

        foreach (var permissionId in desiredPermissionIds)
        {
            if (!currentPermissionIds.Contains(permissionId))
            {
                dbContext.RolePermissions.Add(new RolePermission(role.Id, permissionId));
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var updatedRole = await dbContext.Roles
            .Include(currentRole => currentRole.RolePermissions)
                .ThenInclude(rolePermission => rolePermission.Permission)
            .Include(currentRole => currentRole.UserRoles)
                .ThenInclude(userRole => userRole.User)
            .AsNoTracking()
            .FirstAsync(currentRole => currentRole.Id == roleId, cancellationToken);

        return AdminSecurityServiceResult.Success(MapRoleDetail(updatedRole));
    }

    public async Task<AdminSecurityServiceResult<AdminUserPermissionsResponse>> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .Include(currentUser => currentUser.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                    .ThenInclude(role => role!.RolePermissions)
                        .ThenInclude(rolePermission => rolePermission.Permission)
            .AsNoTracking()
            .FirstOrDefaultAsync(currentUser => currentUser.Id == userId, cancellationToken);

        if (user is null)
        {
            return AdminSecurityServiceResult.NotFound<AdminUserPermissionsResponse>("User was not found.");
        }

        var permissions = await dbContext.Permissions
            .AsNoTracking()
            .OrderBy(permission => permission.Key)
            .ToListAsync(cancellationToken);
        var overrides = await dbContext.UserPermissionOverrides
            .Where(userPermission => userPermission.UserId == userId)
            .AsNoTracking()
            .ToDictionaryAsync(
                userPermission => userPermission.PermissionId,
                userPermission => userPermission.Effect,
                cancellationToken);

        return AdminSecurityServiceResult.Success(MapUserPermissions(user, permissions, overrides));
    }

    public async Task<AdminSecurityServiceResult<AdminUserPermissionsResponse>> UpdateUserPermissionOverridesAsync(
        Guid userId,
        AdminUserPermissionOverridesRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .Include(currentUser => currentUser.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(currentUser => currentUser.Id == userId, cancellationToken);

        if (user is null)
        {
            return AdminSecurityServiceResult.NotFound<AdminUserPermissionsResponse>("User was not found.");
        }

        if (user.UserRoles.Any(userRole =>
                userRole.Role is not null
                && SecurityTextNormalizer.NormalizeName(userRole.Role.Name)
                    == SecurityTextNormalizer.NormalizeName("Admin")))
        {
            return AdminSecurityServiceResult.Conflict<AdminUserPermissionsResponse>(
                "Direct permission overrides cannot be assigned to an Admin user.");
        }

        var requestedOverrides = request.Overrides ?? Array.Empty<AdminUserPermissionOverrideRequest>();
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var desiredOverrides = new Dictionary<Guid, UserPermissionEffect>();

        foreach (var item in requestedOverrides)
        {
            if (item.PermissionId == Guid.Empty)
            {
                AddError(errors, nameof(request.Overrides), "PermissionId cannot be empty.");
                continue;
            }

            if (desiredOverrides.ContainsKey(item.PermissionId))
            {
                AddError(errors, nameof(request.Overrides), "A permission can only appear once.");
                continue;
            }

            if (!TryParseEffect(item.Effect, out var effect))
            {
                AddError(errors, nameof(request.Overrides), "Effect must be Allow or Deny.");
                continue;
            }

            desiredOverrides.Add(item.PermissionId, effect);
        }

        var permissionValidation = await ValidatePermissionIdsAsync(desiredOverrides.Keys.ToHashSet(), cancellationToken);

        if (permissionValidation is not null)
        {
            foreach (var (key, messages) in permissionValidation)
            {
                foreach (var message in messages)
                {
                    AddError(errors, key, message);
                }
            }
        }

        if (errors.Count > 0)
        {
            return AdminSecurityServiceResult.Validation<AdminUserPermissionsResponse>(errors);
        }

        var existingOverrides = await dbContext.UserPermissionOverrides
            .Where(userPermission => userPermission.UserId == userId)
            .ToListAsync(cancellationToken);
        var existingByPermissionId = existingOverrides
            .ToDictionary(userPermission => userPermission.PermissionId);

        dbContext.UserPermissionOverrides.RemoveRange(
            existingOverrides.Where(userPermission => !desiredOverrides.ContainsKey(userPermission.PermissionId)));

        foreach (var (permissionId, effect) in desiredOverrides)
        {
            if (existingByPermissionId.TryGetValue(permissionId, out var existingOverride))
            {
                existingOverride.SetEffect(effect);
            }
            else
            {
                dbContext.UserPermissionOverrides.Add(
                    new UserPermissionOverride(userId, permissionId, effect));
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetUserPermissionsAsync(userId, cancellationToken);
    }

    private async Task<Dictionary<string, string[]>?> ValidatePermissionIdsAsync(
        HashSet<Guid> desiredPermissionIds,
        CancellationToken cancellationToken)
    {
        if (desiredPermissionIds.Count == 0)
        {
            return null;
        }

        var existingPermissionIds = await dbContext.Permissions
            .Where(permission => desiredPermissionIds.Contains(permission.Id))
            .Select(permission => permission.Id)
            .ToListAsync(cancellationToken);

        if (existingPermissionIds.Count == desiredPermissionIds.Count)
        {
            return null;
        }

        return new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["PermissionIds"] = ["One or more permissions do not exist."]
        };
    }

    private static bool TryParseEffect(string? value, out UserPermissionEffect effect)
    {
        if (string.Equals(value?.Trim(), "Allow", StringComparison.OrdinalIgnoreCase))
        {
            effect = UserPermissionEffect.Allow;
            return true;
        }

        if (string.Equals(value?.Trim(), "Deny", StringComparison.OrdinalIgnoreCase))
        {
            effect = UserPermissionEffect.Deny;
            return true;
        }

        effect = default;
        return false;
    }

    private static void AddError(Dictionary<string, string[]> errors, string key, string message)
    {
        if (errors.TryGetValue(key, out var existing))
        {
            errors[key] = [.. existing, message];
            return;
        }

        errors[key] = [message];
    }

    private static AdminUserPermissionsResponse MapUserPermissions(
        User user,
        IReadOnlyCollection<Permission> permissions,
        IReadOnlyDictionary<Guid, UserPermissionEffect> overrides)
    {
        var roleSummaries = user.UserRoles
            .Select(userRole => userRole.Role)
            .Where(role => role is not null)
            .Select(role => new AdminRoleSummaryResponse(
                role!.Id,
                role.Name,
                role.Description,
                role.IsSystem))
            .OrderBy(role => role.Name, StringComparer.Ordinal)
            .ToArray();

        var sourceRolesByPermissionId = user.UserRoles
            .Select(userRole => userRole.Role)
            .Where(role => role is not null)
            .SelectMany(role => role!.RolePermissions.Select(rolePermission => new
            {
                rolePermission.PermissionId,
                RoleName = role.Name
            }))
            .GroupBy(item => item.PermissionId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyCollection<string>)group
                    .Select(item => item.RoleName)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(roleName => roleName, StringComparer.Ordinal)
                    .ToArray());

        var states = permissions
            .Select(permission =>
            {
                var sourceRoles = sourceRolesByPermissionId.TryGetValue(permission.Id, out var roles)
                    ? roles
                    : Array.Empty<string>();
                var inherited = sourceRoles.Count > 0;
                var hasOverride = overrides.TryGetValue(permission.Id, out var overrideEffect);
                var effectiveAllowed = hasOverride
                    ? overrideEffect == UserPermissionEffect.Allow
                    : inherited;

                return new AdminUserPermissionStateResponse(
                    new AdminPermissionResponse(permission.Id, permission.Key, permission.Description),
                    inherited,
                    effectiveAllowed,
                    hasOverride ? overrideEffect.ToString() : null,
                    sourceRoles);
            })
            .OrderBy(state => state.Permission.Key, StringComparer.Ordinal)
            .ToArray();

        return new AdminUserPermissionsResponse(
            user.Id,
            user.Email,
            user.FullName,
            roleSummaries,
            states);
    }

    private static AdminRoleDetailResponse MapRoleDetail(Role role)
    {
        var permissions = role.RolePermissions
            .Select(rolePermission => rolePermission.Permission)
            .Where(permission => permission is not null)
            .Select(permission => new AdminPermissionResponse(
                permission!.Id,
                permission.Key,
                permission.Description))
            .OrderBy(permission => permission.Key, StringComparer.Ordinal)
            .ToArray();

        var activeUserCount = role.UserRoles.Count(userRole => userRole.User?.IsActive == true);

        return new AdminRoleDetailResponse(
            role.Id,
            role.Name,
            role.Description,
            role.IsSystem,
            role.UserRoles.Count,
            activeUserCount,
            permissions);
    }
}
