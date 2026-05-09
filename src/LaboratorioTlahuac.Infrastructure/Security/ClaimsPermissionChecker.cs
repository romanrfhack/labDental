using System.Security.Claims;
using LaboratorioTlahuac.Application.Abstractions.Security;
using LaboratorioTlahuac.Domain.Security;

namespace LaboratorioTlahuac.Infrastructure.Security;

public sealed class ClaimsPermissionChecker : IPermissionChecker
{
    public ValueTask<bool> HasPermissionAsync(
        ClaimsPrincipal user,
        string permission,
        CancellationToken cancellationToken = default)
    {
        var hasPermission = user.Identity?.IsAuthenticated == true
            && user.HasClaim(PermissionClaimTypes.Permission, permission);

        return ValueTask.FromResult(hasPermission);
    }
}
