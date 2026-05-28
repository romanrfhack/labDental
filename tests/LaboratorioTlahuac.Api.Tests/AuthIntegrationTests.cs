using System.Data.Common;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using LaboratorioTlahuac.Application.Abstractions.Time;
using LaboratorioTlahuac.Domain.Security;
using LaboratorioTlahuac.Domain.Security.Entities;
using LaboratorioTlahuac.Infrastructure.Persistence;

namespace LaboratorioTlahuac.Api.Tests;

public sealed class AuthIntegrationTests(TestApplicationFactory factory)
    : IClassFixture<TestApplicationFactory>
{
    [Fact]
    public async Task HealthStillRespondsOk()
    {
        var client = factory.CreateClientWithoutRedirects();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task MeWithoutCookieReturnsUnauthorized()
    {
        var client = factory.CreateClientWithoutRedirects();

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }

    [Fact]
    public async Task CsrfEndpointEmitsReadableXsrfCookie()
    {
        var client = factory.CreateClientWithoutRedirects();

        var response = await client.GetAsync("/api/auth/csrf");
        var xsrfCookie = Assert.Single(
            response.Headers.GetValues("Set-Cookie"),
            header => header.StartsWith("XSRF-TOKEN=", StringComparison.Ordinal));

        Assert.Contains(response.StatusCode, new[] { HttpStatusCode.NoContent, HttpStatusCode.OK });
        Assert.StartsWith("XSRF-TOKEN=", xsrfCookie, StringComparison.Ordinal);
        Assert.DoesNotContain("httponly", xsrfCookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoginWithInvalidCredentialsReturnsUnauthorized()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.GetXsrfTokenAsync();

        var response = await client.PostAsJsonWithXsrfAsync(
            "/api/auth/login",
            xsrfToken,
            new { email = "admin@tests.local", password = "wrong-password" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }

    [Theory]
    [InlineData("inactive@tests.local", "InactivePass123!")]
    [InlineData("locked@tests.local", "LockedPass123!")]
    public async Task LoginWithInactiveOrLockedUserReturnsLocked(string email, string password)
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.GetXsrfTokenAsync();

        var response = await client.PostAsJsonWithXsrfAsync(
            "/api/auth/login",
            xsrfToken,
            new { email, password });

        Assert.Equal((HttpStatusCode)423, response.StatusCode);
    }

    [Fact]
    public async Task MutableApiRequestWithoutXsrfTokenReturnsBadRequest()
    {
        var client = factory.CreateClientWithoutRedirects();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = "admin@tests.local", password = "AdminPass123!" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }

    [Fact]
    public async Task LoginWithValidCredentialsEmitsCookie()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.GetXsrfTokenAsync();

        var response = await client.PostAsJsonWithXsrfAsync(
            "/api/auth/login",
            xsrfToken,
            new { email = "admin@tests.local", password = "AdminPass123!" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(
            response.Headers.GetValues("Set-Cookie"),
            header => header.StartsWith("Ldt.Dev.Auth=", StringComparison.Ordinal));
        Assert.Contains(
            response.Headers.GetValues("Set-Cookie"),
            header => header.Contains("httponly", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task MeWithValidCookieReturnsUserWithoutPasswordHash()
    {
        var client = factory.CreateClientWithoutRedirects();
        await client.LoginAsAdminAsync();

        var response = await client.GetAsync("/api/auth/me");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("admin@tests.local", payload.GetProperty("email").GetString());
        Assert.False(payload.TryGetProperty("passwordHash", out _));
    }

    [Fact]
    public async Task PermissionProtectedEndpointReturnsForbiddenWhenPermissionIsMissing()
    {
        var client = factory.CreateClientWithoutRedirects();
        await client.LoginAsLimitedUserAsync();

        var response = await client.GetAsync("/api/security/permissions-check");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }

    [Fact]
    public async Task PermissionProtectedEndpointReturnsOkWhenPermissionExists()
    {
        var client = factory.CreateClientWithoutRedirects();
        await client.LoginAsAdminAsync();

        var response = await client.GetAsync("/api/security/permissions-check");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedMutableEndpointWithoutXsrfTokenReturnsBadRequest()
    {
        var client = factory.CreateClientWithoutRedirects();
        await client.LoginAsAdminAsync();

        var response = await client.PostAsync("/api/security/csrf-check", content: null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedMutableEndpointWithXsrfTokenReturnsOk()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();

        var response = await client.PostWithXsrfAsync("/api/security/csrf-check", xsrfToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task LogoutWithoutXsrfTokenReturnsBadRequest()
    {
        var client = factory.CreateClientWithoutRedirects();
        await client.LoginAsAdminAsync();

        var response = await client.PostAsync("/api/auth/logout", content: null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task LogoutWithXsrfTokenWorks()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();

        var response = await client.PostWithXsrfAsync("/api/auth/logout", xsrfToken);

        Assert.Contains(response.StatusCode, new[] { HttpStatusCode.NoContent, HttpStatusCode.OK });
    }

    [Fact]
    public async Task CustomersWithoutSessionReturnsUnauthorized()
    {
        var client = factory.CreateClientWithoutRedirects();

        var response = await client.GetAsync("/api/customers");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CustomersWithSessionButWithoutViewPermissionReturnsForbidden()
    {
        var client = factory.CreateClientWithoutRedirects();
        await client.LoginAsLimitedUserAsync();

        var response = await client.GetAsync("/api/customers");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CustomersWithViewPermissionReturnsOk()
    {
        var client = factory.CreateClientWithoutRedirects();
        await client.LoginAsAdminAsync();

        var response = await client.GetAsync("/api/customers");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, payload.GetProperty("page").GetInt32());
    }

    [Fact]
    public async Task CreateCustomerWithoutXsrfReturnsBadRequest()
    {
        var client = factory.CreateClientWithoutRedirects();
        await client.LoginAsAdminAsync();

        var response = await client.PostAsJsonAsync(
            "/api/customers",
            new { type = "Doctor", displayName = UniqueName("Dr Sin XSRF") });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomerWithPermissionAndXsrfCreatesCustomer()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var displayName = UniqueName("Dr Creacion");

        var created = await CreateCustomerAsync(client, xsrfToken, "Doctor", displayName);

        Assert.Equal("Doctor", created.GetProperty("type").GetString());
        Assert.Equal(displayName, created.GetProperty("displayName").GetString());
        Assert.True(created.GetProperty("isActive").GetBoolean());
    }

    [Fact]
    public async Task CreateCustomerWithInvalidRequestReturnsBadRequest()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();

        var response = await client.PostAsJsonWithXsrfAsync(
            "/api/customers",
            xsrfToken,
            new { type = "Doctor", displayName = " " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomerChangesEditableFields()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var created = await CreateCustomerAsync(client, xsrfToken, "Doctor", UniqueName("Dr Original"));
        var customerId = created.GetProperty("id").GetGuid();
        var updatedName = UniqueName("Dr Actualizado");

        var response = await client.PutAsJsonWithXsrfAsync(
            $"/api/customers/{customerId}",
            xsrfToken,
            new
            {
                type = "Doctor",
                displayName = updatedName,
                legalName = "Razon social actualizada",
                contactName = "Contacto",
                phone = "555-0000"
            });
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(updatedName, payload.GetProperty("displayName").GetString());
        Assert.Equal("Razon social actualizada", payload.GetProperty("legalName").GetString());
    }

    [Fact]
    public async Task UpdateCustomerStatusDeactivatesWithoutDeleting()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var created = await CreateCustomerAsync(client, xsrfToken, "Doctor", UniqueName("Dr Desactivar"));
        var customerId = created.GetProperty("id").GetGuid();

        var response = await client.PatchAsJsonWithXsrfAsync(
            $"/api/customers/{customerId}/status",
            xsrfToken,
            new { isActive = false });
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(payload.GetProperty("isActive").GetBoolean());

        var getResponse = await client.GetAsync($"/api/customers/{customerId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task CustomerListDoesNotReturnInactiveCustomersByDefault()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var displayName = UniqueName("Dr Inactivo Default");
        var created = await CreateCustomerAsync(client, xsrfToken, "Doctor", displayName);
        var customerId = created.GetProperty("id").GetGuid();

        var statusResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/customers/{customerId}/status",
            xsrfToken,
            new { isActive = false });
        statusResponse.EnsureSuccessStatusCode();

        var response = await client.GetAsync($"/api/customers?search={Uri.EscapeDataString(displayName)}");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(payload.GetProperty("items").EnumerateArray());
    }

    [Fact]
    public async Task CreateInternalDoctorForClinicWorks()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var clinic = await CreateCustomerAsync(client, xsrfToken, "Clinic", UniqueName("Clinica Internos"));
        var clinicId = clinic.GetProperty("id").GetGuid();
        var doctorName = UniqueName("Dra Interna");

        var response = await client.PostAsJsonWithXsrfAsync(
            $"/api/customers/{clinicId}/internal-doctors",
            xsrfToken,
            new { fullName = doctorName, email = "interna@tests.local" });
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(doctorName, payload.GetProperty("fullName").GetString());
    }

    [Fact]
    public async Task CreateInternalDoctorForDoctorCustomerFails()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var doctor = await CreateCustomerAsync(client, xsrfToken, "Doctor", UniqueName("Dr Sin Internos"));
        var doctorCustomerId = doctor.GetProperty("id").GetGuid();

        var response = await client.PostAsJsonWithXsrfAsync(
            $"/api/customers/{doctorCustomerId}/internal-doctors",
            xsrfToken,
            new { fullName = UniqueName("Interno Invalido") });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangingClinicWithActiveInternalDoctorsToDoctorFailsWithConflict()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var clinic = await CreateCustomerAsync(client, xsrfToken, "Clinic", UniqueName("Clinica Bloqueada"));
        var clinicId = clinic.GetProperty("id").GetGuid();

        var internalDoctorResponse = await client.PostAsJsonWithXsrfAsync(
            $"/api/customers/{clinicId}/internal-doctors",
            xsrfToken,
            new { fullName = UniqueName("Dr Activo") });
        internalDoctorResponse.EnsureSuccessStatusCode();

        var response = await client.PutAsJsonWithXsrfAsync(
            $"/api/customers/{clinicId}",
            xsrfToken,
            new { type = "Doctor", displayName = clinic.GetProperty("displayName").GetString() });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CustomerResponsesDoNotExposePasswordHash()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var created = await CreateCustomerAsync(client, xsrfToken, "Doctor", UniqueName("Dr Sin Seguridad"));
        var customerId = created.GetProperty("id").GetGuid();

        var response = await client.GetAsync($"/api/customers/{customerId}");
        var json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("passwordHash", json, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<JsonElement> CreateCustomerAsync(
        HttpClient client,
        string xsrfToken,
        string type,
        string displayName)
    {
        var response = await client.PostAsJsonWithXsrfAsync(
            "/api/customers",
            xsrfToken,
            new { type, displayName });
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return payload;
    }

    private static string UniqueName(string prefix)
    {
        return $"{prefix} {Guid.NewGuid():N}";
    }
}

public sealed class TestApplicationFactory : WebApplicationFactory<Program>
{
    private readonly DateTimeOffset utcNow;
    private readonly string? dashboardBusinessTimeZone;
    private SqliteConnection? connection;

    public TestApplicationFactory()
        : this(new DateTimeOffset(2026, 5, 9, 12, 0, 0, TimeSpan.Zero))
    {
    }

    internal TestApplicationFactory(DateTimeOffset utcNow, string? dashboardBusinessTimeZone = null)
    {
        this.utcNow = utcNow;
        this.dashboardBusinessTimeZone = dashboardBusinessTimeZone;
    }

    public HttpClient CreateClientWithoutRedirects()
    {
        return CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
    }

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_context, configuration) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:",
                ["SecuritySeed:RunOnStartup"] = "false"
            };

            if (!string.IsNullOrWhiteSpace(dashboardBusinessTimeZone))
            {
                settings["Dashboard:BusinessTimeZone"] = dashboardBusinessTimeZone;
            }

            configuration.AddInMemoryCollection(settings);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbConnection>();
            services.RemoveAll<DbContextOptions<LaboratorioTlahuacDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<LaboratorioTlahuacDbContext>>();
            services.RemoveAll<LaboratorioTlahuacDbContext>();
            services.RemoveAll<IClock>();

            connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            services.AddSingleton<DbConnection>(connection);
            services.AddDbContext<LaboratorioTlahuacDbContext>((serviceProvider, options) =>
            {
                options.UseSqlite(serviceProvider.GetRequiredService<DbConnection>());
            });
            services.AddSingleton<IClock>(new TestClock(utcNow));

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<LaboratorioTlahuacDbContext>();

            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            SeedDatabase(dbContext);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            connection?.Dispose();
        }
    }

    private static void SeedDatabase(LaboratorioTlahuacDbContext dbContext)
    {
        var now = DateTimeOffset.UtcNow;
        var permissions = Permissions.All
            .Select(permissionKey => Permission.Create(
                permissionKey,
                Permissions.Descriptions.TryGetValue(permissionKey, out var description)
                    ? description
                    : permissionKey,
                now))
            .ToDictionary(permission => permission.Key, StringComparer.Ordinal);

        var adminRole = Role.Create("Admin", "Administrador del sistema.", isSystem: true, now);
        var limitedRole = Role.Create("Limited", "Usuario con permisos limitados.", isSystem: false, now);
        var dashboardOrdersRole = Role.Create(
            "DashboardOrders",
            "Usuario con dashboard y operacion.",
            isSystem: false,
            now);
        var dashboardPaymentsRole = Role.Create(
            "DashboardPayments",
            "Usuario con dashboard y cobranza.",
            isSystem: false,
            now);
        var dashboardCustomersRole = Role.Create(
            "DashboardCustomers",
            "Usuario con dashboard y clientes.",
            isSystem: false,
            now);

        dbContext.Permissions.AddRange(permissions.Values);
        dbContext.Roles.AddRange(
            adminRole,
            limitedRole,
            dashboardOrdersRole,
            dashboardPaymentsRole,
            dashboardCustomersRole);
        dbContext.RolePermissions.AddRange(
            permissions.Values.Select(permission => new RolePermission(adminRole.Id, permission.Id)));
        dbContext.RolePermissions.Add(new RolePermission(limitedRole.Id, permissions[Permissions.ReportsView].Id));
        dbContext.RolePermissions.AddRange(
            new RolePermission(dashboardOrdersRole.Id, permissions[Permissions.ReportsView].Id),
            new RolePermission(dashboardOrdersRole.Id, permissions[Permissions.OrdersView].Id),
            new RolePermission(dashboardPaymentsRole.Id, permissions[Permissions.ReportsView].Id),
            new RolePermission(dashboardPaymentsRole.Id, permissions[Permissions.PaymentsView].Id),
            new RolePermission(dashboardCustomersRole.Id, permissions[Permissions.ReportsView].Id),
            new RolePermission(dashboardCustomersRole.Id, permissions[Permissions.CustomersView].Id));

        var passwordHasher = new PasswordHasher<User>();
        var admin = CreateUser("admin@tests.local", "Admin Test", "AdminPass123!", passwordHasher, now);
        var limited = CreateUser("limited@tests.local", "Limited Test", "LimitedPass123!", passwordHasher, now);
        var noPermissions = CreateUser(
            "no-permissions@tests.local",
            "No Permissions Test",
            "NoPermissionsPass123!",
            passwordHasher,
            now);
        var dashboardOrders = CreateUser(
            "dashboard-orders@tests.local",
            "Dashboard Orders Test",
            "DashboardOrdersPass123!",
            passwordHasher,
            now);
        var dashboardPayments = CreateUser(
            "dashboard-payments@tests.local",
            "Dashboard Payments Test",
            "DashboardPaymentsPass123!",
            passwordHasher,
            now);
        var dashboardCustomers = CreateUser(
            "dashboard-customers@tests.local",
            "Dashboard Customers Test",
            "DashboardCustomersPass123!",
            passwordHasher,
            now);
        var inactive = CreateUser("inactive@tests.local", "Inactive Test", "InactivePass123!", passwordHasher, now);
        var locked = CreateUser("locked@tests.local", "Locked Test", "LockedPass123!", passwordHasher, now);

        inactive.Deactivate(now);
        locked.LockUntil(now.AddHours(1), now);

        dbContext.Users.AddRange(
            admin,
            limited,
            noPermissions,
            dashboardOrders,
            dashboardPayments,
            dashboardCustomers,
            inactive,
            locked);
        dbContext.UserRoles.AddRange(
            new UserRole(admin.Id, adminRole.Id),
            new UserRole(limited.Id, limitedRole.Id),
            new UserRole(dashboardOrders.Id, dashboardOrdersRole.Id),
            new UserRole(dashboardPayments.Id, dashboardPaymentsRole.Id),
            new UserRole(dashboardCustomers.Id, dashboardCustomersRole.Id),
            new UserRole(inactive.Id, adminRole.Id),
            new UserRole(locked.Id, adminRole.Id));

        dbContext.SaveChanges();
    }

    private static User CreateUser(
        string email,
        string fullName,
        string password,
        PasswordHasher<User> passwordHasher,
        DateTimeOffset now)
    {
        var user = User.Create(email, fullName, "pending-password-hash", now);
        user.SetPasswordHash(passwordHasher.HashPassword(user, password));

        return user;
    }
}

internal sealed class TestClock(DateTimeOffset utcNow) : IClock
{
    private long ticksOffset = -1;

    public DateTimeOffset UtcNow => utcNow.AddTicks(Interlocked.Increment(ref ticksOffset));
}

internal static class AuthTestClientExtensions
{
    public static async Task<string> LoginAsAdminAsync(this HttpClient client)
    {
        return await client.LoginAsAsync("admin@tests.local", "AdminPass123!");
    }

    public static async Task<string> LoginAsLimitedUserAsync(this HttpClient client)
    {
        return await client.LoginAsAsync("limited@tests.local", "LimitedPass123!");
    }

    public static async Task<string> LoginAsNoPermissionsUserAsync(this HttpClient client)
    {
        return await client.LoginAsAsync("no-permissions@tests.local", "NoPermissionsPass123!");
    }

    public static async Task<string> LoginAsDashboardOrdersUserAsync(this HttpClient client)
    {
        return await client.LoginAsAsync("dashboard-orders@tests.local", "DashboardOrdersPass123!");
    }

    public static async Task<string> LoginAsDashboardPaymentsUserAsync(this HttpClient client)
    {
        return await client.LoginAsAsync("dashboard-payments@tests.local", "DashboardPaymentsPass123!");
    }

    public static async Task<string> LoginAsDashboardCustomersUserAsync(this HttpClient client)
    {
        return await client.LoginAsAsync("dashboard-customers@tests.local", "DashboardCustomersPass123!");
    }

    private static async Task<string> LoginAsAsync(this HttpClient client, string email, string password)
    {
        var xsrfToken = await client.GetXsrfTokenAsync();
        var response = await client.PostAsJsonWithXsrfAsync(
            "/api/auth/login",
            xsrfToken,
            new { email, password });

        response.EnsureSuccessStatusCode();

        return await client.GetXsrfTokenAsync();
    }

    public static async Task<string> GetXsrfTokenAsync(this HttpClient client)
    {
        var response = await client.GetAsync("/api/auth/csrf");
        response.EnsureSuccessStatusCode();

        return GetCookieValue(response, "XSRF-TOKEN");
    }

    public static Task<HttpResponseMessage> PostWithXsrfAsync(
        this HttpClient client,
        string requestUri,
        string xsrfToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Headers.Add("X-XSRF-TOKEN", xsrfToken);

        return client.SendAsync(request);
    }

    public static Task<HttpResponseMessage> PostAsJsonWithXsrfAsync<TValue>(
        this HttpClient client,
        string requestUri,
        string xsrfToken,
        TValue value)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(value)
        };
        request.Headers.Add("X-XSRF-TOKEN", xsrfToken);

        return client.SendAsync(request);
    }

    public static Task<HttpResponseMessage> PutAsJsonWithXsrfAsync<TValue>(
        this HttpClient client,
        string requestUri,
        string xsrfToken,
        TValue value)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, requestUri)
        {
            Content = JsonContent.Create(value)
        };
        request.Headers.Add("X-XSRF-TOKEN", xsrfToken);

        return client.SendAsync(request);
    }

    public static Task<HttpResponseMessage> PatchAsJsonWithXsrfAsync<TValue>(
        this HttpClient client,
        string requestUri,
        string xsrfToken,
        TValue value)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, requestUri)
        {
            Content = JsonContent.Create(value)
        };
        request.Headers.Add("X-XSRF-TOKEN", xsrfToken);

        return client.SendAsync(request);
    }

    private static string GetCookieValue(HttpResponseMessage response, string cookieName)
    {
        var setCookieHeader = GetSetCookieHeader(response, cookieName);
        var cookieValue = setCookieHeader
            .Split(';', StringSplitOptions.TrimEntries)[0]
            .Split('=', count: 2)[1];

        return Uri.UnescapeDataString(cookieValue);
    }

    private static string GetSetCookieHeader(HttpResponseMessage response, string cookieName)
    {
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders));

        return Assert.Single(
            setCookieHeaders,
            header => header.StartsWith($"{cookieName}=", StringComparison.Ordinal));
    }
}
