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

        Assert.Equal(19, Permissions.All.Count);
    }
}
