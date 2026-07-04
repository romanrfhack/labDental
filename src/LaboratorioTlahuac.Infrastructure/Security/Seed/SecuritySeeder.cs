using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using LaboratorioTlahuac.Domain.Security;
using LaboratorioTlahuac.Domain.Security.Entities;
using LaboratorioTlahuac.Infrastructure.Persistence;

namespace LaboratorioTlahuac.Infrastructure.Security.Seed;

public sealed class SecuritySeeder(
    LaboratorioTlahuacDbContext dbContext,
    IPasswordHasher<User> passwordHasher,
    IConfiguration configuration,
    SecuritySeedRuntimeOptions runtimeOptions,
    ILogger<SecuritySeeder> logger)
    : ISecuritySeeder
{
    private const string DriverRoleName = "Repartidor";
    private const string DriverRoleDescription = "Rol operativo para futuras entregas; sin permisos activos en Fase 3.3.";
    private const string LimitedQaRoleName = "Limited QA";
    private const string LimitedQaRoleDescription = "Usuario QA limitado local de Development.";
    private static readonly char[] PermissionSeparators = [',', ';', ' ', '\n', '\r', '\t'];
    private static readonly Action<ILogger, Exception?> LogLimitedQaSeedSkippedOutsideDevelopment =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(1, nameof(LogLimitedQaSeedSkippedOutsideDevelopment)),
            "Limited QA user seed skipped because the current environment is not Development.");
    private static readonly Action<ILogger, Exception?> LogLimitedQaSeedSkippedMissingConfiguration =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(2, nameof(LogLimitedQaSeedSkippedMissingConfiguration)),
            "Limited QA user seed skipped because email, password or full name configuration is missing.");
    private static readonly Action<ILogger, string, Exception?> LogLimitedQaSeedIgnoredUnknownPermissions =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(3, nameof(LogLimitedQaSeedIgnoredUnknownPermissions)),
            "Limited QA user seed ignored unknown permission key(s): {PermissionKeys}.");
    private static readonly Action<ILogger, Exception?> LogLimitedQaSeedSkippedAdminEmail =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(4, nameof(LogLimitedQaSeedSkippedAdminEmail)),
            "Limited QA user seed skipped because the configured email belongs to an Admin user.");

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var adminSeedEnabled = configuration.GetValue<bool>($"{SecuritySeedOptions.SectionName}:RunOnStartup");
        var limitedQaSeedEnabled = IsLimitedQaSeedEnabled();
        var baselineSeedEnabled = configuration.GetValue<bool>(
            $"{SecuritySeedOptions.SectionName}:EnsureBaselineOnStartup");

        if (!baselineSeedEnabled && !adminSeedEnabled && !limitedQaSeedEnabled)
        {
            return;
        }

        var options = adminSeedEnabled
            ? configuration
                .GetSection(SecuritySeedOptions.SectionName)
                .Get<SecuritySeedOptions>() ?? new SecuritySeedOptions()
            : new SecuritySeedOptions();

        var now = DateTimeOffset.UtcNow;
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var permissionsByKey = await EnsurePermissionsAsync(now, cancellationToken);
        await EnsureDriverRoleAsync(now, cancellationToken);

        if (adminSeedEnabled)
        {
            var adminRole = await EnsureAdminRoleAsync(options, now, cancellationToken);
            await EnsureAdminPermissionsAsync(adminRole, permissionsByKey, cancellationToken);
            await EnsureAdminUserAsync(options, adminRole, now, cancellationToken);
        }

        if (limitedQaSeedEnabled)
        {
            await EnsureLimitedQaUserAsync(permissionsByKey, now, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private bool IsLimitedQaSeedEnabled()
    {
        var runOnStartup = configuration.GetValue<bool>($"{LimitedQaUserSeedOptions.SectionName}:RunOnStartup");

        if (!runOnStartup)
        {
            return false;
        }

        if (runtimeOptions.IsDevelopment)
        {
            return true;
        }

        LogLimitedQaSeedSkippedOutsideDevelopment(logger, null);

        return false;
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

    private async Task<Role> EnsureDriverRoleAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var normalizedRoleName = SecurityTextNormalizer.NormalizeName(DriverRoleName);
        var role = await dbContext.Roles
            .FirstOrDefaultAsync(
                currentRole => currentRole.NormalizedName == normalizedRoleName,
                cancellationToken);

        if (role is not null)
        {
            return role;
        }

        role = Role.Create(DriverRoleName, DriverRoleDescription, isSystem: true, now);
        dbContext.Roles.Add(role);

        return role;
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

    private async Task EnsureLimitedQaUserAsync(
        IReadOnlyDictionary<string, Permission> permissionsByKey,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var options = configuration
            .GetSection(LimitedQaUserSeedOptions.SectionName)
            .Get<LimitedQaUserSeedOptions>() ?? new LimitedQaUserSeedOptions();

        var email = FirstNonWhiteSpace(
            configuration["LT_QA_LIMITED_EMAIL"],
            options.Email);
        var password = FirstNonWhiteSpace(
            configuration["LT_QA_LIMITED_PASSWORD"],
            options.Password);
        var fullName = FirstNonWhiteSpace(
            configuration["LT_QA_LIMITED_FULL_NAME"],
            options.FullName);

        if (string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(fullName))
        {
            LogLimitedQaSeedSkippedMissingConfiguration(logger, null);
            return;
        }

        var limitedQaRole = await EnsureLimitedQaRoleAsync(now, cancellationToken);
        var desiredPermissionKeys = ParsePermissionKeys(options.Permissions);
        var desiredPermissions = GetKnownPermissions(desiredPermissionKeys, permissionsByKey);

        await SynchronizeRolePermissionsAsync(limitedQaRole, desiredPermissions, cancellationToken);
        await EnsureLimitedQaUserAsync(email, password, fullName, limitedQaRole, now, cancellationToken);
    }

    private async Task<Role> EnsureLimitedQaRoleAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var normalizedRoleName = SecurityTextNormalizer.NormalizeName(LimitedQaRoleName);
        var role = await dbContext.Roles
            .FirstOrDefaultAsync(
                currentRole => currentRole.NormalizedName == normalizedRoleName,
                cancellationToken);

        if (role is not null)
        {
            return role;
        }

        role = Role.Create(LimitedQaRoleName, LimitedQaRoleDescription, isSystem: true, now);
        dbContext.Roles.Add(role);

        return role;
    }

    private List<Permission> GetKnownPermissions(
        HashSet<string> desiredPermissionKeys,
        IReadOnlyDictionary<string, Permission> permissionsByKey)
    {
        var selectedPermissions = new List<Permission>();
        var unknownPermissionKeys = new List<string>();

        foreach (var permissionKey in desiredPermissionKeys)
        {
            if (permissionsByKey.TryGetValue(permissionKey, out var permission))
            {
                selectedPermissions.Add(permission);
                continue;
            }

            unknownPermissionKeys.Add(permissionKey);
        }

        if (unknownPermissionKeys.Count > 0)
        {
            LogLimitedQaSeedIgnoredUnknownPermissions(logger, string.Join(", ", unknownPermissionKeys), null);
        }

        return selectedPermissions;
    }

    private async Task SynchronizeRolePermissionsAsync(
        Role role,
        IReadOnlyCollection<Permission> desiredPermissions,
        CancellationToken cancellationToken)
    {
        var desiredPermissionIds = desiredPermissions
            .Select(permission => permission.Id)
            .ToHashSet();
        var existingRolePermissions = await dbContext.RolePermissions
            .Where(rolePermission => rolePermission.RoleId == role.Id)
            .ToListAsync(cancellationToken);
        var existingPermissionIds = existingRolePermissions
            .Select(rolePermission => rolePermission.PermissionId)
            .ToHashSet();

        dbContext.RolePermissions.RemoveRange(
            existingRolePermissions.Where(rolePermission => !desiredPermissionIds.Contains(rolePermission.PermissionId)));

        foreach (var desiredPermissionId in desiredPermissionIds)
        {
            if (existingPermissionIds.Contains(desiredPermissionId))
            {
                continue;
            }

            dbContext.RolePermissions.Add(new RolePermission(role.Id, desiredPermissionId));
        }
    }

    private async Task EnsureLimitedQaUserAsync(
        string email,
        string password,
        string fullName,
        Role limitedQaRole,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = SecurityTextNormalizer.NormalizeEmail(email);
        var user = await dbContext.Users
            .FirstOrDefaultAsync(
                currentUser => currentUser.NormalizedEmail == normalizedEmail,
                cancellationToken);

        if (user is not null && await HasAdminRoleAsync(user.Id, cancellationToken))
        {
            LogLimitedQaSeedSkippedAdminEmail(logger, null);
            return;
        }

        if (user is null)
        {
            user = User.Create(email, fullName, "pending-password-hash", now);
            dbContext.Users.Add(user);
        }
        else
        {
            user.Rename(fullName, now);
            user.Activate(now);
            user.ClearLockout(now);
        }

        user.SetPasswordHash(passwordHasher.HashPassword(user, password));

        await SynchronizeUserRolesAsync(user, limitedQaRole, cancellationToken);
    }

    private async Task<bool> HasAdminRoleAsync(Guid userId, CancellationToken cancellationToken)
    {
        var adminRoleName = FirstNonWhiteSpace(
            configuration[$"{SecuritySeedOptions.SectionName}:AdminRoleName"],
            "Admin") ?? "Admin";
        var normalizedAdminRoleName = SecurityTextNormalizer.NormalizeName(adminRoleName);

        return await dbContext.UserRoles
            .Where(userRole => userRole.UserId == userId)
            .Join(
                dbContext.Roles,
                userRole => userRole.RoleId,
                role => role.Id,
                (_, role) => role.NormalizedName)
            .AnyAsync(
                normalizedRoleName => normalizedRoleName == normalizedAdminRoleName,
                cancellationToken);
    }

    private async Task SynchronizeUserRolesAsync(
        User user,
        Role limitedQaRole,
        CancellationToken cancellationToken)
    {
        var existingUserRoles = await dbContext.UserRoles
            .Where(userRole => userRole.UserId == user.Id)
            .ToListAsync(cancellationToken);
        var hasLimitedQaRole = false;

        foreach (var userRole in existingUserRoles)
        {
            if (userRole.RoleId == limitedQaRole.Id)
            {
                hasLimitedQaRole = true;
                continue;
            }

            dbContext.UserRoles.Remove(userRole);
        }

        if (!hasLimitedQaRole)
        {
            dbContext.UserRoles.Add(new UserRole(user.Id, limitedQaRole.Id));
        }
    }

    private static HashSet<string> ParsePermissionKeys(string? permissions)
    {
        return (permissions ?? string.Empty)
            .Split(PermissionSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string? FirstNonWhiteSpace(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();
    }
}
