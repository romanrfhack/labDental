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

        return LoginResult.Success(MapToAuthenticatedUser(user));
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

        return MapToAuthenticatedUser(user);
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

    private static AuthenticatedUser MapToAuthenticatedUser(User user)
    {
        var roles = user.UserRoles
            .Select(userRole => userRole.Role)
            .Where(role => role is not null)
            .Select(role => role!.Name)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(role => role, StringComparer.Ordinal)
            .ToArray();

        var permissions = user.UserRoles
            .Select(userRole => userRole.Role)
            .Where(role => role is not null)
            .SelectMany(role => role!.RolePermissions)
            .Select(rolePermission => rolePermission.Permission)
            .Where(permission => permission is not null)
            .Select(permission => permission!.Key)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(permission => permission, StringComparer.Ordinal)
            .ToArray();

        return new AuthenticatedUser(user.Id, user.Email, user.FullName, roles, permissions);
    }
}
