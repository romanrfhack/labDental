using System.Security.Claims;

namespace LaboratorioTlahuac.Application.Abstractions.Security;

public interface IPermissionChecker
{
    ValueTask<bool> HasPermissionAsync(
        ClaimsPrincipal user,
        string permission,
        CancellationToken cancellationToken = default);
}
