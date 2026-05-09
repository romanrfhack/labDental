using LaboratorioTlahuac.Api.Endpoints;
using LaboratorioTlahuac.Api.Security;
using LaboratorioTlahuac.Domain.Security;

namespace LaboratorioTlahuac.Api.Tests;

public class ApiBootstrapTests
{
    [Fact]
    public void HealthResponseExposesExpectedContract()
    {
        var response = new HealthResponse("Healthy", "LaboratorioTlahuac.Api");

        Assert.Equal("Healthy", response.Status);
        Assert.Equal("LaboratorioTlahuac.Api", response.Application);
    }

    [Fact]
    public void RequirePermissionAttributeUsesPermissionAsPolicyName()
    {
        var attribute = new RequirePermissionAttribute(Permissions.OrdersEdit);

        Assert.Equal(Permissions.OrdersEdit, attribute.Policy);
        Assert.Equal(Permissions.OrdersEdit, attribute.Permission);
    }
}
