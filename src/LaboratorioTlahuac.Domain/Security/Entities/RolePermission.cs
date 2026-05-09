#pragma warning disable CA1711
namespace LaboratorioTlahuac.Domain.Security.Entities;

public sealed class RolePermission
{
    private RolePermission()
    {
    }

    public RolePermission(Guid roleId, Guid permissionId)
    {
        RoleId = roleId;
        PermissionId = permissionId;
    }

    public Guid RoleId { get; private set; }

    public Guid PermissionId { get; private set; }

    public Role? Role { get; private set; }

    public Permission? Permission { get; private set; }
}
#pragma warning restore CA1711
