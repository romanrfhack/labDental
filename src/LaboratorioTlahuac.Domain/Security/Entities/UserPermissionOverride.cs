#pragma warning disable CA1711
namespace LaboratorioTlahuac.Domain.Security.Entities;

public enum UserPermissionEffect
{
    Allow = 1,
    Deny = 2
}

public sealed class UserPermissionOverride
{
    private UserPermissionOverride()
    {
    }

    public UserPermissionOverride(Guid userId, Guid permissionId, UserPermissionEffect effect)
    {
        if (!Enum.IsDefined(effect))
        {
            throw new ArgumentOutOfRangeException(nameof(effect));
        }

        UserId = userId;
        PermissionId = permissionId;
        Effect = effect;
    }

    public Guid UserId { get; private set; }

    public Guid PermissionId { get; private set; }

    public UserPermissionEffect Effect { get; private set; }

    public User? User { get; private set; }

    public Permission? Permission { get; private set; }

    public void SetEffect(UserPermissionEffect effect)
    {
        if (!Enum.IsDefined(effect))
        {
            throw new ArgumentOutOfRangeException(nameof(effect));
        }

        Effect = effect;
    }
}
#pragma warning restore CA1711
