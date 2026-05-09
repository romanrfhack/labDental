namespace LaboratorioTlahuac.Application.Authentication;

public sealed record AuthenticatedUser(
    Guid Id,
    string Email,
    string FullName,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);
