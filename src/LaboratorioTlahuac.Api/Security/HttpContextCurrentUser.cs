using System.Security.Claims;
using LaboratorioTlahuac.Application.Abstractions.Security;
using LaboratorioTlahuac.Domain.Security;

namespace LaboratorioTlahuac.Api.Security;

public sealed class HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
        }
    }

    public string? Email => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);

    public IReadOnlyCollection<string> Permissions =>
        httpContextAccessor.HttpContext?.User
            .FindAll(PermissionClaimTypes.Permission)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray()
        ?? [];
}
