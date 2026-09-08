using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LaboratorioTlahuac.Application.Abstractions.Authentication;
using LaboratorioTlahuac.Application.Authentication;
using LaboratorioTlahuac.Domain.Security;
using LaboratorioTlahuac.Domain.Security.Entities;
using LaboratorioTlahuac.Infrastructure.Persistence;

namespace LaboratorioTlahuac.Infrastructure.Security.Authentication;

public sealed class AuthSessionService(
    LaboratorioTlahuacDbContext dbContext,
    IPasswordHasher<User> passwordHasher)
    : IAuthSessionService
{
    private const int MaxFailedAccessAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private static readonly string AdminRoleName = SecurityTextNormalizer.NormalizeName("Admin");

    public async Task<LoginResult> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return LoginResult.Failure(LoginFailureReason.InvalidCredentials);
        }

        var normalizedEmail = SecurityTextNormalizer.NormalizeEmail(email);
        var now = DateTimeOffset.UtcNow;
        var user = await FindUserWithSecurityGraphAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            return LoginResult.Failure(LoginFailureReason.InvalidCredentials);
        }

        if (!user.IsActive)
        {
            return LoginResult.Failure(LoginFailureReason.Inactive);
        }

        if (user.IsLockedOut(now))
        {
            return LoginResult.Failure(LoginFailureReason.LockedOut);
        }

        var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            user.RecordFailedLogin(now, MaxFailedAccessAttempts, LockoutDuration);
            await dbContext.SaveChangesAsync(cancellationToken);

            return user.IsLockedOut(now)
                ? LoginResult.Failure(LoginFailureReason.LockedOut)
                : LoginResult.Failure(LoginFailureReason.InvalidCredentials);
        }

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.SetPasswordHash(passwordHasher.HashPassword(user, password));
        }

        user.RecordSuccessfulLogin(now);
        await dbContext.SaveChangesAsync(cancellationToken);

        return LoginResult.Success(await MapToAuthenticatedUserAsync(user, cancellationToken));
    }

    public async Task<AuthenticatedUser?> GetCurrentUserAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var user = await dbContext.Users
            .Include(currentUser => currentUser.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                    .ThenInclude(role => role!.RolePermissions)
                        .ThenInclude(rolePermission => rolePermission.Permission)
            .AsNoTracking()
            .FirstOrDefaultAsync(currentUser => currentUser.Id == userId, cancellationToken);

        if (user is null || !user.IsActive || user.IsLockedOut(now))
        {
            return null;
        }

        return await MapToAuthenticatedUserAsync(user, cancellationToken);
    }

    private Task<User?> FindUserWithSecurityGraphAsync(
        string normalizedEmail,
        CancellationToken cancellationToken)
    {
        return dbContext.Users
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                    .ThenInclude(role => role!.RolePermissions)
                        .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    private async Task<AuthenticatedUser> MapToAuthenticatedUserAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var roleEntities = user.UserRoles
            .Select(userRole => userRole.Role)
            .Where(role => role is not null)
            .Select(role => role!)
            .ToArray();

        var roles = roleEntities
            .Select(role => role.Name)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(role => role, StringComparer.Ordinal)
            .ToArray();

        var permissionSet = roleEntities
            .SelectMany(role => role.RolePermissions)
            .Select(rolePermission => rolePermission.Permission)
            .Where(permission => permission is not null)
            .Select(permission => permission!.Key)
            .ToHashSet(StringComparer.Ordinal);

        var isAdmin = roleEntities.Any(role => role.NormalizedName == AdminRoleName);

        if (!isAdmin)
        {
            var overrides = await dbContext.UserPermissionOverrides
                .Where(userPermission => userPermission.UserId == user.Id)
                .Join(
                    dbContext.Permissions,
                    userPermission => userPermission.PermissionId,
                    permission => permission.Id,
                    (userPermission, permission) => new
                    {
                        permission.Key,
                        userPermission.Effect
                    })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var directOverride in overrides)
            {
                if (directOverride.Effect == UserPermissionEffect.Allow)
                {
                    permissionSet.Add(directOverride.Key);
                }
                else if (directOverride.Effect == UserPermissionEffect.Deny)
                {
                    permissionSet.Remove(directOverride.Key);
                }
            }
        }

        return new AuthenticatedUser(
            user.Id,
            user.Email,
            user.FullName,
            roles,
            permissionSet.OrderBy(permission => permission, StringComparer.Ordinal).ToArray());
    }
}
