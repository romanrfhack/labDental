using System.Security.Claims;
using LaboratorioTlahuac.Application.Authentication;

namespace LaboratorioTlahuac.Application.Abstractions.Authentication;

public interface IAuthSessionService
{
    Task<LoginResult> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<AuthenticatedUser?> GetCurrentUserAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default);
}
