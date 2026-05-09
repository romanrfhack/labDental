namespace LaboratorioTlahuac.Application.Abstractions.Security;

public interface ICurrentUser
{
    Guid? UserId { get; }

    string? Email { get; }

    IReadOnlyCollection<string> Permissions { get; }
}
