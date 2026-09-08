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
    public async Task UserAllowAndDenyOverridesApplyToExistingSessionAndAuthMe()
    {
        var adminClient = factory.CreateClientWithoutRedirects();
        var adminXsrf = await adminClient.LoginAsAdminAsync();
        var userClient = factory.CreateClientWithoutRedirects();
        await userClient.LoginAsNoPermissionsUserAsync();

        var before = await userClient.GetAsync("/api/customers");
        Assert.Equal(HttpStatusCode.Forbidden, before.StatusCode);

        var users = await adminClient.GetFromJsonAsync<JsonElement>("/api/admin/users?pageSize=100");
        var user = users.GetProperty("items").EnumerateArray()
            .Single(item => item.GetProperty("email").GetString() == "no-permissions@tests.local");
        var userId = user.GetProperty("id").GetGuid();

        var permissions = await adminClient.GetFromJsonAsync<JsonElement>("/api/admin/permissions");
        var customersViewId = permissions.EnumerateArray()
            .Single(item => item.GetProperty("key").GetString() == Permissions.CustomersView)
            .GetProperty("id")
            .GetGuid();

        var allow = await adminClient.PatchAsJsonWithXsrfAsync(
            $"/api/admin/users/{userId}/permissions",
            adminXsrf,
            new
            {
                overrides = new[]
                {
                    new { permissionId = customersViewId, effect = "Allow" }
                }
            });

        Assert.Equal(HttpStatusCode.OK, allow.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await userClient.GetAsync("/api/customers")).StatusCode);

        var meWithAllow = await userClient.GetFromJsonAsync<JsonElement>("/api/auth/me");
        Assert.Contains(
            meWithAllow.GetProperty("permissions").EnumerateArray(),
            item => item.GetString() == Permissions.CustomersView);

        var deny = await adminClient.PatchAsJsonWithXsrfAsync(
            $"/api/admin/users/{userId}/permissions",
            adminXsrf,
            new
            {
                overrides = new[]
                {
                    new { permissionId = customersViewId, effect = "Deny" }
                }
            });

        Assert.Equal(HttpStatusCode.OK, deny.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await userClient.GetAsync("/api/customers")).StatusCode);

        var meWithDeny = await userClient.GetFromJsonAsync<JsonElement>("/api/auth/me");
        Assert.DoesNotContain(
            meWithDeny.GetProperty("permissions").EnumerateArray(),
            item => item.GetString() == Permissions.CustomersView);
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
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/admin/users")).StatusCode);
    }

    [Fact]
    public async Task AdminUserPermissionOverridesAreProtected()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrf = await client.LoginAsAdminAsync();
        var users = await client.GetFromJsonAsync<JsonElement>("/api/admin/users?pageSize=100");
        var admin = users.GetProperty("items").EnumerateArray()
            .Single(item => item.GetProperty("email").GetString() == "admin@tests.local");
        var adminUserId = admin.GetProperty("id").GetGuid();

        var response = await client.PatchAsJsonWithXsrfAsync(
            $"/api/admin/users/{adminUserId}/permissions",
            xsrf,
            new { overrides = Array.Empty<object>() });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UserWithoutAdminPermissionsCannotManageRoleOrUserPermissions()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrf = await client.LoginAsLimitedUserAsync();

        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/admin/permissions")).StatusCode);

        var roleEdit = await client.PatchAsJsonWithXsrfAsync(
            $"/api/admin/roles/{Guid.NewGuid()}/permissions",
            xsrf,
            new { permissionIds = Array.Empty<Guid>() });
        Assert.Equal(HttpStatusCode.Forbidden, roleEdit.StatusCode);

        var userEdit = await client.PatchAsJsonWithXsrfAsync(
            $"/api/admin/users/{Guid.NewGuid()}/permissions",
            xsrf,
            new { overrides = Array.Empty<object>() });
        Assert.Equal(HttpStatusCode.Forbidden, userEdit.StatusCode);
    }
}
