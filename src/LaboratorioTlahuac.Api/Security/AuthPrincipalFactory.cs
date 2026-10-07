using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using LaboratorioTlahuac.Application.Authentication;
using LaboratorioTlahuac.Domain.Security;

namespace LaboratorioTlahuac.Api.Security;

public static class AuthPrincipalFactory
{
    public static ClaimsPrincipal Create(AuthenticatedUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName)
        };

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(user.Permissions.Select(permission => new Claim(PermissionClaimTypes.Permission, permission)));

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme,
            ClaimTypes.Name,
            ClaimTypes.Role);

        return new ClaimsPrincipal(identity);
    }

    public static bool Matches(ClaimsPrincipal? principal, AuthenticatedUser user)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        if (principal.FindFirstValue(ClaimTypes.NameIdentifier) != user.Id.ToString()
            || principal.FindFirstValue(ClaimTypes.Email) != user.Email
            || principal.FindFirstValue(ClaimTypes.Name) != user.FullName)
        {
            return false;
        }

        var roles = principal.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal);
        var permissions = principal.FindAll(PermissionClaimTypes.Permission)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal);

        return roles.SequenceEqual(user.Roles.OrderBy(value => value, StringComparer.Ordinal), StringComparer.Ordinal)
            && permissions.SequenceEqual(
                user.Permissions.OrderBy(value => value, StringComparer.Ordinal),
                StringComparer.Ordinal);
    }
}
