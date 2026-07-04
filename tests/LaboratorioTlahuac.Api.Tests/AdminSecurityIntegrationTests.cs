using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LaboratorioTlahuac.Domain.Security;

namespace LaboratorioTlahuac.Api.Tests;

public sealed class AdminSecurityIntegrationTests(TestApplicationFactory factory)
    : IClassFixture<TestApplicationFactory>
{
    [Fact]
    public async Task AdminCanListUsers()
    {
        var client = factory.CreateClientWithoutRedirects();
        await client.LoginAsAdminAsync();

        var response = await client.GetAsync("/api/admin/users");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(payload.GetProperty("totalCount").GetInt32() >= 1);
        Assert.Contains(
            payload.GetProperty("items").EnumerateArray(),
            user => user.GetProperty("email").GetString() == "admin@tests.local");
    }

    [Fact]
    public async Task AdminCanCreateUserWithoutPasswordLeak()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var driverRole = await GetRoleByNameAsync(client, "Repartidor");
        var email = UniqueEmail("new-user");
        const string temporaryPassword = "TempUserPass123!";

        var response = await client.PostAsJsonWithXsrfAsync(
            "/api/admin/users",
            xsrfToken,
            new
            {
                email,
                fullName = "Nuevo Usuario QA",
                temporaryPassword,
                roleIds = new[] { driverRole.GetProperty("id").GetGuid() }
            });
        var json = await response.Content.ReadAsStringAsync();
        var payload = JsonSerializer.Deserialize<JsonElement>(json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(email, payload.GetProperty("email").GetString());
        Assert.False(payload.TryGetProperty("passwordHash", out _));
        Assert.DoesNotContain("passwordHash", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(temporaryPassword, json, StringComparison.Ordinal);
        Assert.Contains(
            payload.GetProperty("roles").EnumerateArray(),
            role => role.GetProperty("name").GetString() == "Repartidor");
    }

    [Fact]
    public async Task AdminCanAssignRole()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var driverRole = await GetRoleByNameAsync(client, "Repartidor");
        var limitedRole = await GetRoleByNameAsync(client, "Limited");
        var created = await CreateAdminUserAsync(client, xsrfToken, driverRole.GetProperty("id").GetGuid());
        var userId = created.GetProperty("id").GetGuid();

        var response = await client.PatchAsJsonWithXsrfAsync(
            $"/api/admin/users/{userId}/roles",
            xsrfToken,
            new { roleIds = new[] { limitedRole.GetProperty("id").GetGuid() } });
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(
            payload.GetProperty("roles").EnumerateArray(),
            role => role.GetProperty("name").GetString() == "Limited");
        Assert.DoesNotContain(
            payload.GetProperty("roles").EnumerateArray(),
            role => role.GetProperty("name").GetString() == "Repartidor");
    }

    [Fact]
    public async Task AdminCanActivateAndDeactivateUser()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var driverRole = await GetRoleByNameAsync(client, "Repartidor");
        var created = await CreateAdminUserAsync(client, xsrfToken, driverRole.GetProperty("id").GetGuid());
        var userId = created.GetProperty("id").GetGuid();

        var deactivateResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/admin/users/{userId}/status",
            xsrfToken,
            new { isActive = false });
        var deactivated = await deactivateResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);
        Assert.False(deactivated.GetProperty("isActive").GetBoolean());

        var activateResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/admin/users/{userId}/status",
            xsrfToken,
            new { isActive = true });
        var activated = await activateResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, activateResponse.StatusCode);
        Assert.True(activated.GetProperty("isActive").GetBoolean());
    }

    [Fact]
    public async Task UserWithoutAdminPermissionReceivesForbidden()
    {
        var client = factory.CreateClientWithoutRedirects();
        await client.LoginAsLimitedUserAsync();

        var usersResponse = await client.GetAsync("/api/admin/users");
        var rolesResponse = await client.GetAsync("/api/admin/roles");

        Assert.Equal(HttpStatusCode.Forbidden, usersResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, rolesResponse.StatusCode);
    }

    [Fact]
    public async Task UserWithoutSessionReceivesUnauthorized()
    {
        var client = factory.CreateClientWithoutRedirects();

        var usersResponse = await client.GetAsync("/api/admin/users");
        var rolesResponse = await client.GetAsync("/api/admin/roles");

        Assert.Equal(HttpStatusCode.Unauthorized, usersResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, rolesResponse.StatusCode);
    }

    [Fact]
    public async Task RolesListAndDetailWork()
    {
        var client = factory.CreateClientWithoutRedirects();
        await client.LoginAsAdminAsync();

        var rolesResponse = await client.GetAsync("/api/admin/roles");
        var roles = await rolesResponse.Content.ReadFromJsonAsync<JsonElement>();
        var driverRole = roles.EnumerateArray()
            .Single(role => role.GetProperty("name").GetString() == "Repartidor");

        Assert.Equal(HttpStatusCode.OK, rolesResponse.StatusCode);
        Assert.Equal(2, driverRole.GetProperty("permissionCount").GetInt32());
        Assert.Equal(
            new[] { Permissions.DeliveriesComplete, Permissions.DeliveriesView },
            driverRole.GetProperty("permissions")
                .EnumerateArray()
                .Select(permission => permission.GetProperty("key").GetString())
                .OrderBy(permission => permission, StringComparer.Ordinal)
                .ToArray());

        var detailResponse = await client.GetAsync($"/api/admin/roles/{driverRole.GetProperty("id").GetGuid()}");
        var detail = await detailResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        Assert.Equal("Repartidor", detail.GetProperty("name").GetString());
        Assert.Equal(
            new[] { Permissions.DeliveriesComplete, Permissions.DeliveriesView },
            detail.GetProperty("permissions")
                .EnumerateArray()
                .Select(permission => permission.GetProperty("key").GetString())
                .OrderBy(permission => permission, StringComparer.Ordinal)
                .ToArray());
    }

    [Fact]
    public async Task AdminExistingUserKeepsAdministrativePermissions()
    {
        var client = factory.CreateClientWithoutRedirects();
        await client.LoginAsAdminAsync();

        var meResponse = await client.GetAsync("/api/auth/me");
        var me = await meResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        Assert.Contains(
            me.GetProperty("permissions").EnumerateArray(),
            permission => permission.GetString() == Permissions.UsersManage);
        Assert.Contains(
            me.GetProperty("permissions").EnumerateArray(),
            permission => permission.GetString() == Permissions.RolesManage);
        Assert.Contains(
            me.GetProperty("permissions").EnumerateArray(),
            permission => permission.GetString() == Permissions.DeliveriesAssign);
        Assert.Contains(
            me.GetProperty("permissions").EnumerateArray(),
            permission => permission.GetString() == Permissions.DeliveriesUpdate);
        Assert.Contains(
            me.GetProperty("permissions").EnumerateArray(),
            permission => permission.GetString() == Permissions.DeliveriesComplete);
        Assert.Contains(
            me.GetProperty("roles").EnumerateArray(),
            role => role.GetString() == "Admin");
    }

    [Fact]
    public async Task AdminUserResponsesDoNotExposePasswordHash()
    {
        var client = factory.CreateClientWithoutRedirects();
        await client.LoginAsAdminAsync();

        var listResponse = await client.GetAsync("/api/admin/users");
        var listJson = await listResponse.Content.ReadAsStringAsync();
        var firstUserId = JsonSerializer.Deserialize<JsonElement>(listJson)
            .GetProperty("items")
            .EnumerateArray()
            .First()
            .GetProperty("id")
            .GetGuid();
        var detailResponse = await client.GetAsync($"/api/admin/users/{firstUserId}");
        var detailJson = await detailResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        Assert.DoesNotContain("passwordHash", listJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("passwordHash", detailJson, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<JsonElement> GetRoleByNameAsync(HttpClient client, string roleName)
    {
        var response = await client.GetAsync("/api/admin/roles");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return payload.EnumerateArray()
            .Single(role => role.GetProperty("name").GetString() == roleName);
    }

    private static async Task<JsonElement> CreateAdminUserAsync(
        HttpClient client,
        string xsrfToken,
        Guid roleId)
    {
        var response = await client.PostAsJsonWithXsrfAsync(
            "/api/admin/users",
            xsrfToken,
            new
            {
                email = UniqueEmail("created-admin-user"),
                fullName = "Usuario Admin QA",
                temporaryPassword = "TempUserPass123!",
                roleIds = new[] { roleId }
            });
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return payload;
    }

    private static string UniqueEmail(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid():N}@tests.local";
    }
}
