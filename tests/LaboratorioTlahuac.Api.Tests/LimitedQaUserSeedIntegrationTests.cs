using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LaboratorioTlahuac.Domain.Security;

namespace LaboratorioTlahuac.Api.Tests;

public sealed class LimitedQaUserSeedIntegrationTests
{
    private const string QaEmail = "qa-limited-api@tests.local";
    private const string QaPassword = "QaLimitedApiPass123!";
    private const string QaFullName = "QA Limited API Test";

    [Fact]
    public async Task LimitedQaUserCanLoginAndIsForbiddenFromDashboardSummary()
    {
        using var factory = new TestApplicationFactory(
            new DateTimeOffset(2026, 5, 9, 12, 0, 0, TimeSpan.Zero),
            extraSettings: new Dictionary<string, string?>
            {
                ["SecuritySeed:RunOnStartup"] = "false",
                ["SecuritySeed:LimitedQaUser:RunOnStartup"] = "true",
                ["SecuritySeed:LimitedQaUser:Email"] = QaEmail,
                ["SecuritySeed:LimitedQaUser:Password"] = QaPassword,
                ["SecuritySeed:LimitedQaUser:FullName"] = QaFullName,
                ["SecuritySeed:LimitedQaUser:Permissions"] = Permissions.CustomersView
            },
            seedDatabase: false);
        var anonymousClient = factory.CreateClientWithoutRedirects();
        var anonymousDashboardResponse = await anonymousClient.GetAsync("/api/dashboard/summary");

        Assert.Equal(HttpStatusCode.Unauthorized, anonymousDashboardResponse.StatusCode);

        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.GetXsrfTokenAsync();
        var loginResponse = await client.PostAsJsonWithXsrfAsync(
            "/api/auth/login",
            xsrfToken,
            new { email = QaEmail, password = QaPassword });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var meResponse = await client.GetAsync("/api/auth/me");
        var mePayload = await meResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        Assert.Equal(QaEmail, mePayload.GetProperty("email").GetString());
        Assert.False(mePayload.TryGetProperty("passwordHash", out _));
        Assert.Contains(
            mePayload.GetProperty("permissions").EnumerateArray(),
            permission => permission.GetString() == Permissions.CustomersView);
        Assert.DoesNotContain(
            mePayload.GetProperty("permissions").EnumerateArray(),
            permission => permission.GetString() == Permissions.ReportsView);

        var customersResponse = await client.GetAsync("/api/customers");
        var dashboardResponse = await client.GetAsync("/api/dashboard/summary");

        Assert.Equal(HttpStatusCode.OK, customersResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, dashboardResponse.StatusCode);
    }
}
