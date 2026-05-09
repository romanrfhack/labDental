using LaboratorioTlahuac.Domain.Security;

namespace LaboratorioTlahuac.Domain.Security.Entities;

public sealed class Role
{
    private Role()
    {
    }

    private Role(
        Guid id,
        string name,
        string description,
        bool isSystem,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        SetName(name);
        Description = description.Trim();
        IsSystem = isSystem;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string NormalizedName { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public bool IsSystem { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    public static Role Create(
        string name,
        string description,
        bool isSystem,
        DateTimeOffset createdAtUtc)
    {
        return new Role(Guid.NewGuid(), name, description, isSystem, createdAtUtc);
    }

    private void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        NormalizedName = SecurityTextNormalizer.NormalizeName(Name);
    }
}
