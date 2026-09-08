namespace LaboratorioTlahuac.Domain.Security.Entities;

public sealed class UserPermissionOverride
{
    private UserPermissionOverride()
    {
    }

    public UserPermissionOverride(
        Guid userId,
        Guid permissionId,
        UserPermissionOverrideEffect effect)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        if (permissionId == Guid.Empty)
        {
            throw new ArgumentException("Permission id is required.", nameof(permissionId));
        }

        UserId = userId;
        PermissionId = permissionId;
        Effect = effect;
    }

    public Guid UserId { get; private set; }

    public Guid PermissionId { get; private set; }

    public UserPermissionOverrideEffect Effect { get; private set; }

    public User? User { get; private set; }

    public Permission? Permission { get; private set; }

    public void SetEffect(UserPermissionOverrideEffect effect)
    {
        Effect = effect;
    }
}
