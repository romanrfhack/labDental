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

        var existingPermissionIds = await dbContext.Permissions
            .Where(permission => desiredPermissionIds.Contains(permission.Id))
            .Select(permission => permission.Id)
            .ToListAsync(cancellationToken);

        if (existingPermissionIds.Count != desiredPermissionIds.Count)
        {
            return AdminSecurityServiceResult.Validation<AdminRoleDetailResponse>(
                new Dictionary<string, string[]>(StringComparer.Ordinal)
                {
                    [nameof(request.PermissionIds)] = ["One or more permissions do not exist."]
                });
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
