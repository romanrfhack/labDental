using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using LaboratorioTlahuac.Domain.Security;
using LaboratorioTlahuac.Infrastructure.Security.Seed;

namespace LaboratorioTlahuac.Api.Tests;

public sealed class SecuritySeederPermissionPreservationTests
{
    [Fact]
    public async Task BaselineSeedDoesNotOverwriteExistingDriverRolePermissions()
    {
        using var factory = new TestApplicationFactory(
            new DateTimeOffset(2026, 9, 8, 2, 0, 0, TimeSpan.Zero),
            extraSettings: new Dictionary<string, string?>
            {
                ["SecuritySeed:EnsureBaselineOnStartup"] = "true",
                ["SecuritySeed:RunOnStartup"] = "false",
                ["SecuritySeed:LimitedQaUser:RunOnStartup"] = "false"
            });
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var driverRole = await GetRoleByNameAsync(client, "Repartidor");
        var customersViewId = driverRole.GetProperty("availablePermissions")
            .EnumerateArray()
            .Single(permission => permission.GetProperty("key").GetString() == Permissions.CustomersView)
            .GetProperty("id")
            .GetGuid();

        var updateResponse = await client.PutAsJsonWithXsrfAsync(
            $"/api/admin/roles/{driverRole.GetProperty("id").GetGuid()}/permissions",
            xsrfToken,
            new { permissionIds = new[] { customersViewId } });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<ISecuritySeeder>();
            await seeder.SeedAsync();
        }

        var afterSeed = await GetRoleByNameAsync(client, "Repartidor");
        var permissionKeys = afterSeed.GetProperty("permissions")
            .EnumerateArray()
            .Select(permission => permission.GetProperty("key").GetString())
            .ToArray();

        Assert.Equal(new[] { Permissions.CustomersView }, permissionKeys);
    }

    private static async Task<JsonElement> GetRoleByNameAsync(HttpClient client, string roleName)
    {
        var roles = await client.GetFromJsonAsync<JsonElement>("/api/admin/roles");
        var role = roles.EnumerateArray()
            .Single(currentRole => currentRole.GetProperty("name").GetString() == roleName);

        return await client.GetFromJsonAsync<JsonElement>(
            $"/api/admin/roles/{role.GetProperty("id").GetGuid()}");
    }
}
