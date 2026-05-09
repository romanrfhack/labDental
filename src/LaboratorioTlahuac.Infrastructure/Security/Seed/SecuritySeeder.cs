using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using LaboratorioTlahuac.Domain.Security;
using LaboratorioTlahuac.Domain.Security.Entities;
using LaboratorioTlahuac.Infrastructure.Persistence;

namespace LaboratorioTlahuac.Infrastructure.Security.Seed;

public sealed class SecuritySeeder(
    LaboratorioTlahuacDbContext dbContext,
    IPasswordHasher<User> passwordHasher,
    IConfiguration configuration)
    : ISecuritySeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var options = configuration
            .GetSection(SecuritySeedOptions.SectionName)
            .Get<SecuritySeedOptions>() ?? new SecuritySeedOptions();

        var now = DateTimeOffset.UtcNow;
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var permissionsByKey = await EnsurePermissionsAsync(now, cancellationToken);
        var adminRole = await EnsureAdminRoleAsync(options, now, cancellationToken);
        await EnsureAdminPermissionsAsync(adminRole, permissionsByKey, cancellationToken);
        await EnsureAdminUserAsync(options, adminRole, now, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<Dictionary<string, Permission>> EnsurePermissionsAsync(
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var existingPermissions = await dbContext.Permissions
            .ToDictionaryAsync(permission => permission.Key, StringComparer.Ordinal, cancellationToken);

        foreach (var permissionKey in Permissions.All)
        {
            if (existingPermissions.ContainsKey(permissionKey))
            {
                continue;
            }

            var description = Permissions.Descriptions.TryGetValue(permissionKey, out var knownDescription)
                ? knownDescription
                : permissionKey;

            var permission = Permission.Create(permissionKey, description, now);
            dbContext.Permissions.Add(permission);
            existingPermissions.Add(permissionKey, permission);
        }

        return existingPermissions;
    }

    private async Task<Role> EnsureAdminRoleAsync(
        SecuritySeedOptions options,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var normalizedAdminRoleName = SecurityTextNormalizer.NormalizeName(options.AdminRoleName);
        var adminRole = await dbContext.Roles
            .FirstOrDefaultAsync(
                role => role.NormalizedName == normalizedAdminRoleName,
                cancellationToken);

        if (adminRole is not null)
        {
            return adminRole;
        }

        adminRole = Role.Create(
            options.AdminRoleName,
            options.AdminRoleDescription,
            isSystem: true,
            now);

        dbContext.Roles.Add(adminRole);

        return adminRole;
    }

    private async Task EnsureAdminPermissionsAsync(
        Role adminRole,
        IReadOnlyDictionary<string, Permission> permissionsByKey,
        CancellationToken cancellationToken)
    {
        var existingPermissionIds = await dbContext.RolePermissions
            .Where(rolePermission => rolePermission.RoleId == adminRole.Id)
            .Select(rolePermission => rolePermission.PermissionId)
            .ToListAsync(cancellationToken);

        var existingPermissionIdSet = existingPermissionIds.ToHashSet();

        foreach (var permission in permissionsByKey.Values)
        {
            if (existingPermissionIdSet.Contains(permission.Id))
            {
                continue;
            }

            dbContext.RolePermissions.Add(new RolePermission(adminRole.Id, permission.Id));
        }
    }

    private async Task EnsureAdminUserAsync(
        SecuritySeedOptions options,
        Role adminRole,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var adminEmail = FirstNonWhiteSpace(
            configuration["LT_ADMIN_EMAIL"],
            options.Admin.Email);
        var adminPassword = FirstNonWhiteSpace(
            configuration["LT_ADMIN_PASSWORD"],
            options.Admin.Password);
        var adminFullName = FirstNonWhiteSpace(
            configuration["LT_ADMIN_FULL_NAME"],
            options.Admin.FullName,
            "Administrador") ?? "Administrador";

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            return;
        }

        var normalizedAdminEmail = SecurityTextNormalizer.NormalizeEmail(adminEmail);
        var adminUser = await dbContext.Users
            .FirstOrDefaultAsync(
                user => user.NormalizedEmail == normalizedAdminEmail,
                cancellationToken);

        if (adminUser is null)
        {
            adminUser = User.Create(adminEmail, adminFullName, "pending-password-hash", now);
            adminUser.SetPasswordHash(passwordHasher.HashPassword(adminUser, adminPassword));
            dbContext.Users.Add(adminUser);
        }

        var hasAdminRole = await dbContext.UserRoles
            .AnyAsync(
                userRole => userRole.UserId == adminUser.Id && userRole.RoleId == adminRole.Id,
                cancellationToken);

        if (!hasAdminRole)
        {
            dbContext.UserRoles.Add(new UserRole(adminUser.Id, adminRole.Id));
        }
    }

    private static string? FirstNonWhiteSpace(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();
    }
}
