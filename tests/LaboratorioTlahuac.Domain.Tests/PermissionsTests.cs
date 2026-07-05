using LaboratorioTlahuac.Domain.Security;

namespace LaboratorioTlahuac.Domain.Tests;

public class PermissionsTests
{
    [Fact]
    public void AllContainsInitialPermissionCatalog()
    {
        Assert.Contains(Permissions.OrdersView, Permissions.All);
        Assert.Contains(Permissions.PaymentsCreate, Permissions.All);
        Assert.Contains(Permissions.CustomersEdit, Permissions.All);
        Assert.Contains(Permissions.RolesManage, Permissions.All);
        Assert.Contains(Permissions.DeliveriesView, Permissions.All);
        Assert.Contains(Permissions.DeliveriesAssign, Permissions.All);
        Assert.Contains(Permissions.DeliveriesUpdate, Permissions.All);
        Assert.Contains(Permissions.DeliveriesComplete, Permissions.All);
        Assert.Contains(Permissions.CatalogView, Permissions.All);
        Assert.Contains(Permissions.CatalogManage, Permissions.All);

        Assert.Equal(25, Permissions.All.Count);
    }
}
