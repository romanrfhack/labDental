using LaboratorioTlahuac.Application.Abstractions.Security;

namespace LaboratorioTlahuac.Application.Tests;

public class PermissionCheckerContractTests
{
    [Fact]
    public void PermissionCheckerContractIsAvailableToApplicationLayer()
    {
        var method = typeof(IPermissionChecker).GetMethod(nameof(IPermissionChecker.HasPermissionAsync));

        Assert.NotNull(method);
    }
}
