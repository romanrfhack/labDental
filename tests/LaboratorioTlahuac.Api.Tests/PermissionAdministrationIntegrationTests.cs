using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LaboratorioTlahuac.Domain.Security;

namespace LaboratorioTlahuac.Api.Tests;

public sealed class PermissionAdministrationIntegrationTests(TestApplicationFactory factory)
    : IClassFixture<TestApplicationFactory>
{
    [Fact]
    public async Task RolePermissionChangesApplyToExistingSessionWithoutRelogin()
    {
        var adminClient = factory.CreateClientWithoutRedirects();
        var adminXsrf = await adminClient.LoginAsAdminAsync();
        var limitedRole = await GetRoleByNameAsync(adminClient, "Limited");
        var originalPermissionIds = limitedRole
            .GetProperty("permissions")
            .EnumerateArray()
            .Select(permission => permission.GetProperty("id").GetGuid())
            .ToArray();
        var customersViewId = GetPermissionId(limitedRole, Permissions.CustomersView);
        var email = UniqueEmail("role-refresh");
        const string password = "RoleRefreshPass123!";
        await CreateUserAsync(
            adminClient,
            adminXsrf,
            email,
            password,
            limitedRole.GetProperty("id").GetGuid());

        var limitedClient = factory.CreateClientWithoutRedirects();
        await LoginAsync(limitedClient, email, password);

        Assert.Equal(HttpStatusCode.Forbidden, (await limitedClient.GetAsync("/api/customers")).StatusCode);

        var grantResponse = await adminClient.PutAsJsonWithXsrfAsync(
            $"/api/admin/roles/{limitedRole.GetProperty("id").GetGuid()}/permissions",
            adminXsrf,
            new { permissionIds = originalPermissionIds.Append(customersViewId).Distinct().ToArray() });

        Assert.Equal(HttpStatusCode.OK, grantResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await limitedClient.GetAsync("/api/customers")).StatusCode);

        var meAfterGrant = await limitedClient.GetFromJsonAsync<JsonElement>("/api/auth/me");
        Assert.Contains(
            meAfterGrant.GetProperty("permissions").EnumerateArray(),
            permission => permission.GetString() == Permissions.CustomersView);

        var revokeResponse = await adminClient.PutAsJsonWithXsrfAsync(
            $"/api/admin/roles/{limitedRole.GetProperty("id").GetGuid()}/permissions",
            adminXsrf,
            new { permissionIds = originalPermissionIds });

        Assert.Equal(HttpStatusCode.OK, revokeResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await limitedClient.GetAsync("/api/customers")).StatusCode);
    }

    [Fact]
    public async Task UserAllowAndDenyOverridesApplyToExistingSessionWithoutRelogin()
    {
        var adminClient = factory.CreateClientWithoutRedirects();
        var adminXsrf = await adminClient.LoginAsAdminAsync();
        var limitedRole = await GetRoleByNameAsync(adminClient, "Limited");
        var email = UniqueEmail("override-refresh");
        const string password = "OverrideRefreshPass123!";
        var createdUser = await CreateUserAsync(
            adminClient,
            adminXsrf,
            email,
            password,
            limitedRole.GetProperty("id").GetGuid());
        var userId = createdUser.GetProperty("id").GetGuid();
        var detail = await adminClient.GetFromJsonAsync<JsonElement>($"/api/admin/users/{userId}");
        var customersViewId = GetUserPermissionId(detail, Permissions.CustomersView);
        var reportsViewId = GetUserPermissionId(detail, Permissions.ReportsView);

        var userClient = factory.CreateClientWithoutRedirects();
        await LoginAsync(userClient, email, password);

        Assert.Equal(HttpStatusCode.Forbidden, (await userClient.GetAsync("/api/customers")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await userClient.GetAsync("/api/dashboard/summary")).StatusCode);

        var updateResponse = await adminClient.PutAsJsonWithXsrfAsync(
            $"/api/admin/users/{userId}/permissions",
            adminXsrf,
            new
            {
                overrides = new object[]
                {
                    new { permissionId = customersViewId, effect = "Allow" },
                    new { permissionId = reportsViewId, effect = "Deny" }
                }
            });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await userClient.GetAsync("/api/customers")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await userClient.GetAsync("/api/dashboard/summary")).StatusCode);

        var me = await userClient.GetFromJsonAsync<JsonElement>("/api/auth/me");
        var effectivePermissions = me.GetProperty("permissions")
            .EnumerateArray()
            .Select(permission => permission.GetString())
            .ToArray();

        Assert.Contains(Permissions.CustomersView, effectivePermissions);
        Assert.DoesNotContain(Permissions.ReportsView, effectivePermissions);
    }

    [Fact]
    public async Task AdminRoleAndAdminUserPermissionSetsAreProtected()
    {
        var adminClient = factory.CreateClientWithoutRedirects();
        var adminXsrf = await adminClient.LoginAsAdminAsync();
        var adminRole = await GetRoleByNameAsync(adminClient, "Admin");

        var roleResponse = await adminClient.PutAsJsonWithXsrfAsync(
            $"/api/admin/roles/{adminRole.GetProperty("id").GetGuid()}/permissions",
            adminXsrf,
            new { permissionIds = Array.Empty<Guid>() });

        Assert.Equal(HttpStatusCode.Conflict, roleResponse.StatusCode);

        var users = await adminClient.GetFromJsonAsync<JsonElement>("/api/admin/users?search=admin%40tests.local");
        var adminUserId = users.GetProperty("items")
            .EnumerateArray()
            .Single(user => user.GetProperty("email").GetString() == "admin@tests.local")
            .GetProperty("id")
            .GetGuid();
        var adminDetail = await adminClient.GetFromJsonAsync<JsonElement>($"/api/admin/users/{adminUserId}");
        var customersViewId = GetUserPermissionId(adminDetail, Permissions.CustomersView);

        Assert.True(adminDetail.GetProperty("isPermissionOverrideEditingLocked").GetBoolean());

        var userResponse = await adminClient.PutAsJsonWithXsrfAsync(
            $"/api/admin/users/{adminUserId}/permissions",
            adminXsrf,
            new
            {
                overrides = new[]
                {
                    new { permissionId = customersViewId, effect = "Deny" }
                }
            });

        Assert.Equal(HttpStatusCode.Conflict, userResponse.StatusCode);
    }

    private static async Task<JsonElement> GetRoleByNameAsync(HttpClient client, string roleName)
    {
        var roles = await client.GetFromJsonAsync<JsonElement>("/api/admin/roles");
        var roleSummary = roles.EnumerateArray()
            .Single(role => role.GetProperty("name").GetString() == roleName);

        return await client.GetFromJsonAsync<JsonElement>(
            $"/api/admin/roles/{roleSummary.GetProperty("id").GetGuid()}");
    }

    private static Guid GetPermissionId(JsonElement roleDetail, string permissionKey)
    {
        return roleDetail.GetProperty("availablePermissions")
            .EnumerateArray()
            .Single(permission => permission.GetProperty("key").GetString() == permissionKey)
            .GetProperty("id")
            .GetGuid();
    }

    private static Guid GetUserPermissionId(JsonElement userDetail, string permissionKey)
    {
        return userDetail.GetProperty("permissions")
            .EnumerateArray()
            .Single(permission => permission.GetProperty("key").GetString() == permissionKey)
            .GetProperty("id")
            .GetGuid();
    }

    private static async Task<JsonElement> CreateUserAsync(
        HttpClient client,
        string xsrfToken,
        string email,
        string password,
        Guid roleId)
    {
        var response = await client.PostAsJsonWithXsrfAsync(
            "/api/admin/users",
            xsrfToken,
            new
            {
                email,
                fullName = "Permission Administration QA",
                temporaryPassword = password,
                roleIds = new[] { roleId }
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static async Task LoginAsync(HttpClient client, string email, string password)
    {
        var xsrfToken = await client.GetXsrfTokenAsync();
        var response = await client.PostAsJsonWithXsrfAsync(
            "/api/auth/login",
            xsrfToken,
            new { email, password });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static string UniqueEmail(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid():N}@tests.local";
    }
}
