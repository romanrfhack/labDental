#pragma warning disable CA1711
namespace LaboratorioTlahuac.Domain.Security.Entities;

public sealed class Permission
{
    private Permission()
    {
    }

    private Permission(Guid id, string key, string description, DateTimeOffset createdAtUtc)
    {
        Id = id;
        SetKey(key);
        Description = description.Trim();
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public string Key { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    public ICollection<UserPermissionOverride> UserPermissionOverrides { get; private set; } = new List<UserPermissionOverride>();

    public static Permission Create(string key, string description, DateTimeOffset createdAtUtc)
    {
        return new Permission(Guid.NewGuid(), key, description, createdAtUtc);
    }

    private void SetKey(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        Key = key.Trim();
    }
}
#pragma warning restore CA1711
