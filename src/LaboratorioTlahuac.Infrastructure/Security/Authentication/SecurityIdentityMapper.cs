using LaboratorioTlahuac.Application.Authentication;
using LaboratorioTlahuac.Domain.Security;
using LaboratorioTlahuac.Domain.Security.Entities;

namespace LaboratorioTlahuac.Infrastructure.Security.Authentication;

public static class SecurityIdentityMapper
{
    private static readonly string NormalizedAdminRoleName = SecurityTextNormalizer.NormalizeName("Admin");

    public static AuthenticatedUser Map(User user)
    {
        return new AuthenticatedUser(
            user.Id,
            user.Email,
            user.FullName,
            GetRoleNames(user),
            GetPermissionKeys(user));
    }

    public static string[] GetRoleNames(User user)
    {
        return user.UserRoles
            .Select(userRole => userRole.Role)
            .Where(role => role is not null)
            .Select(role => role!.Name)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(role => role, StringComparer.Ordinal)
            .ToArray();
    }

    public static string[] GetPermissionKeys(User user)
    {
        var permissions = user.UserRoles
            .Select(userRole => userRole.Role)
            .Where(role => role is not null)
            .SelectMany(role => role!.RolePermissions)
            .Select(rolePermission => rolePermission.Permission)
            .Where(permission => permission is not null)
            .Select(permission => permission!.Key)
            .ToHashSet(StringComparer.Ordinal);

        if (!IsAdmin(user))
        {
            foreach (var permissionOverride in user.PermissionOverrides)
            {
                var permissionKey = permissionOverride.Permission?.Key;

                if (string.IsNullOrWhiteSpace(permissionKey))
                {
                    continue;
                }

                if (permissionOverride.Effect == UserPermissionOverrideEffect.Deny)
                {
                    permissions.Remove(permissionKey);
                }
                else
                {
                    permissions.Add(permissionKey);
                }
            }
        }

        return permissions
            .OrderBy(permission => permission, StringComparer.Ordinal)
            .ToArray();
    }

    public static bool IsAdmin(User user)
    {
        return user.UserRoles
            .Select(userRole => userRole.Role)
            .Any(role => role?.NormalizedName == NormalizedAdminRoleName);
    }
}
