using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using LaboratorioTlahuac.Application.Abstractions.Security;
using LaboratorioTlahuac.Domain.Security;
using LaboratorioTlahuac.Domain.Security.Entities;
using LaboratorioTlahuac.Infrastructure.Persistence;

namespace LaboratorioTlahuac.Infrastructure.Security;

public sealed class ClaimsPermissionChecker(LaboratorioTlahuacDbContext dbContext) : IPermissionChecker
{
    private static readonly string AdminRoleName = SecurityTextNormalizer.NormalizeName("Admin");

    public async ValueTask<bool> HasPermissionAsync(
        ClaimsPrincipal user,
        string permission,
        CancellationToken cancellationToken = default)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        var isActive = await dbContext.Users.AnyAsync(
            currentUser =>
                currentUser.Id == userId
                && currentUser.IsActive
                && (currentUser.LockoutEndUtc == null || currentUser.LockoutEndUtc <= now),
            cancellationToken);

        if (!isActive)
        {
            return false;
        }

        var isAdmin = await dbContext.UserRoles
            .Where(userRole => userRole.UserId == userId)
            .Join(
                dbContext.Roles,
                userRole => userRole.RoleId,
                role => role.Id,
                (_, role) => role.NormalizedName)
            .AnyAsync(roleName => roleName == AdminRoleName, cancellationToken);

        if (!isAdmin)
        {
            var directOverride = await dbContext.UserPermissionOverrides
                .Where(userPermission => userPermission.UserId == userId)
                .Join(
                    dbContext.Permissions,
                    userPermission => userPermission.PermissionId,
                    currentPermission => currentPermission.Id,
                    (userPermission, currentPermission) => new
                    {
                        currentPermission.Key,
                        userPermission.Effect
                    })
                .Where(item => item.Key == permission)
                .Select(item => (UserPermissionEffect?)item.Effect)
                .FirstOrDefaultAsync(cancellationToken);

            if (directOverride == UserPermissionEffect.Deny)
            {
                return false;
            }

            if (directOverride == UserPermissionEffect.Allow)
            {
                return true;
            }
        }

        return await dbContext.UserRoles
            .Where(userRole => userRole.UserId == userId)
            .Join(
                dbContext.RolePermissions,
                userRole => userRole.RoleId,
                rolePermission => rolePermission.RoleId,
                (_, rolePermission) => rolePermission.PermissionId)
            .Join(
                dbContext.Permissions,
                permissionId => permissionId,
                currentPermission => currentPermission.Id,
                (_, currentPermission) => currentPermission.Key)
            .AnyAsync(permissionKey => permissionKey == permission, cancellationToken);
    }
}
