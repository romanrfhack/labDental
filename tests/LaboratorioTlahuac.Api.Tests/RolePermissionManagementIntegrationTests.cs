using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LaboratorioTlahuac.Domain.Security;

namespace LaboratorioTlahuac.Api.Tests;

public sealed class RolePermissionManagementIntegrationTests(TestApplicationFactory factory)
    : IClassFixture<TestApplicationFactory>
{
    [Fact]
    public async Task AdminCanUpdateRolePermissionsAndExistingSessionLosesPermissionImmediately()
    {
        var adminClient = factory.CreateClientWithoutRedirects();
        var adminXsrf = await adminClient.LoginAsAdminAsync();
        var userClient = factory.CreateClientWithoutRedirects();
        await userClient.LoginAsDashboardOrdersUserAsync();

        var before = await userClient.GetAsync("/api/work-orders");
        Assert.Equal(HttpStatusCode.OK, before.StatusCode);

        var roles = await adminClient.GetFromJsonAsync<JsonElement>("/api/admin/roles");
        var role = roles.EnumerateArray()
            .Single(item => item.GetProperty("name").GetString() == "DashboardOrders");
        var roleId = role.GetProperty("id").GetGuid();

        var permissions = await adminClient.GetFromJsonAsync<JsonElement>("/api/admin/permissions");
        var reportsViewId = permissions.EnumerateArray()
            .Single(item => item.GetProperty("key").GetString() == Permissions.ReportsView)
            .GetProperty("id")
            .GetGuid();

        var update = await adminClient.PatchAsJsonWithXsrfAsync(
            $"/api/admin/roles/{roleId}/permissions",
            adminXsrf,
            new { permissionIds = new[] { reportsViewId } });

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var after = await userClient.GetAsync("/api/work-orders");
        Assert.Equal(HttpStatusCode.Forbidden, after.StatusCode);

        var dashboard = await userClient.GetAsync("/api/dashboard/summary");
        Assert.Equal(HttpStatusCode.OK, dashboard.StatusCode);
    }

    [Fact]
    public async Task AdminRolePermissionsAreProtected()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrf = await client.LoginAsAdminAsync();
        var roles = await client.GetFromJsonAsync<JsonElement>("/api/admin/roles");
        var adminRole = roles.EnumerateArray()
            .Single(item => item.GetProperty("name").GetString() == "Admin");
        var adminRoleId = adminRole.GetProperty("id").GetGuid();

        var response = await client.PatchAsJsonWithXsrfAsync(
            $"/api/admin/roles/{adminRoleId}/permissions",
            xsrf,
            new { permissionIds = Array.Empty<Guid>() });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var users = await client.GetAsync("/api/admin/users");
        Assert.Equal(HttpStatusCode.OK, users.StatusCode);
    }

    [Fact]
    public async Task UserWithoutRolesManageCannotListOrEditPermissions()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrf = await client.LoginAsLimitedUserAsync();

        var listResponse = await client.GetAsync("/api/admin/permissions");
        Assert.Equal(HttpStatusCode.Forbidden, listResponse.StatusCode);

        var editResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/admin/roles/{Guid.NewGuid()}/permissions",
            xsrf,
            new { permissionIds = Array.Empty<Guid>() });

        Assert.Equal(HttpStatusCode.Forbidden, editResponse.StatusCode);
    }
}
