using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using LaboratorioTlahuac.Application.Abstractions.Security;
using LaboratorioTlahuac.Infrastructure.Persistence;

namespace LaboratorioTlahuac.Infrastructure.Security;

public sealed class ClaimsPermissionChecker(LaboratorioTlahuacDbContext dbContext) : IPermissionChecker
{
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

        return await dbContext.Users
            .Where(currentUser =>
                currentUser.Id == userId
                && currentUser.IsActive
                && (currentUser.LockoutEndUtc == null || currentUser.LockoutEndUtc <= now))
            .SelectMany(currentUser => currentUser.UserRoles)
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
