using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LaboratorioTlahuac.Api.Tests;

public sealed class DashboardIntegrationTests
{
    private static readonly DateOnly ReceivedDate = new(2026, 5, 1);
    private static readonly DateOnly Today = new(2026, 5, 9);
    private static readonly DateOnly Tomorrow = new(2026, 5, 10);
    private static readonly DateOnly Yesterday = new(2026, 5, 8);

    [Fact]
    public async Task SummaryWithoutSessionReturnsUnauthorized()
    {
        using var factory = new TestApplicationFactory();
        var client = factory.CreateClientWithoutRedirects();

        var response = await client.GetAsync("/api/dashboard/summary");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SummaryWithoutReportsPermissionReturnsForbidden()
    {
        using var factory = new TestApplicationFactory();
        var client = factory.CreateClientWithoutRedirects();
        await client.LoginAsNoPermissionsUserAsync();

        var response = await client.GetAsync("/api/dashboard/summary");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SummaryWithReportsPermissionReturnsOk()
    {
        using var factory = new TestApplicationFactory();
        var client = factory.CreateClientWithoutRedirects();
        await client.LoginAsLimitedUserAsync();

        var response = await client.GetAsync("/api/dashboard/summary");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(JsonValueKind.Null, payload.GetProperty("operationalSummary").ValueKind);
        Assert.Equal(JsonValueKind.Null, payload.GetProperty("financialSummary").ValueKind);
        Assert.Equal(JsonValueKind.Null, payload.GetProperty("customerSummary").ValueKind);
    }

    [Fact]
    public async Task UserWithOrdersPermissionReceivesOperationalSummary()
    {
        using var factory = new TestApplicationFactory();
        var client = factory.CreateClientWithoutRedirects();
        await client.LoginAsDashboardOrdersUserAsync();

        var payload = await GetDashboardSummaryAsync(client);

        Assert.Equal(JsonValueKind.Object, payload.GetProperty("operationalSummary").ValueKind);
        Assert.Equal(JsonValueKind.Null, payload.GetProperty("financialSummary").ValueKind);
    }

    [Fact]
    public async Task UserWithoutOrdersPermissionDoesNotReceiveOperationalSummary()
    {
        using var factory = new TestApplicationFactory();
        var client = factory.CreateClientWithoutRedirects();
        await client.LoginAsLimitedUserAsync();

        var payload = await GetDashboardSummaryAsync(client);

        Assert.Equal(JsonValueKind.Null, payload.GetProperty("operationalSummary").ValueKind);
    }

    [Fact]
    public async Task UserWithPaymentsPermissionReceivesFinancialSummary()
    {
        using var factory = new TestApplicationFactory();
        var client = factory.CreateClientWithoutRedirects();
        await client.LoginAsDashboardPaymentsUserAsync();

        var payload = await GetDashboardSummaryAsync(client);

        Assert.Equal(JsonValueKind.Object, payload.GetProperty("financialSummary").ValueKind);
        Assert.Equal(JsonValueKind.Null, payload.GetProperty("operationalSummary").ValueKind);
    }

    [Fact]
    public async Task UserWithoutPaymentsPermissionDoesNotReceiveFinancialSummary()
    {
        using var factory = new TestApplicationFactory();
        var client = factory.CreateClientWithoutRedirects();
        await client.LoginAsLimitedUserAsync();

        var payload = await GetDashboardSummaryAsync(client);

        Assert.Equal(JsonValueKind.Null, payload.GetProperty("financialSummary").ValueKind);
    }

    [Fact]
    public async Task UserWithCustomersPermissionReceivesCustomerSummary()
    {
        using var factory = new TestApplicationFactory();
        var adminClient = factory.CreateClientWithoutRedirects();
        var adminXsrfToken = await adminClient.LoginAsAdminAsync();
        await CreateCustomerAsync(adminClient, adminXsrfToken, "Doctor", UniqueName("Dr Dashboard"));
        await CreateCustomerAsync(adminClient, adminXsrfToken, "Clinic", UniqueName("Clinica Dashboard"));
        var inactive = await CreateCustomerAsync(adminClient, adminXsrfToken, "Other", UniqueName("Otro Inactivo"));
        await UpdateCustomerStatusAsync(
            adminClient,
            adminXsrfToken,
            inactive.GetProperty("id").GetGuid(),
            isActive: false);

        var dashboardClient = factory.CreateClientWithoutRedirects();
        await dashboardClient.LoginAsDashboardCustomersUserAsync();

        var payload = await GetDashboardSummaryAsync(dashboardClient);
        var customerSummary = payload.GetProperty("customerSummary");

        Assert.Equal(2, customerSummary.GetProperty("activeCustomersCount").GetInt32());
        Assert.Equal(1, customerSummary.GetProperty("activeDoctorsCount").GetInt32());
        Assert.Equal(1, customerSummary.GetProperty("activeClinicsCount").GetInt32());
        Assert.Equal(1, customerSummary.GetProperty("inactiveCustomersCount").GetInt32());
    }

    [Fact]
    public async Task TotalReceivableSumsOnlyPositiveBalances()
    {
        using var factory = new TestApplicationFactory();
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        await CreateWorkOrderWithCustomerAsync(client, xsrfToken, totalAmount: 100);
        var second = await CreateWorkOrderWithCustomerAsync(client, xsrfToken, totalAmount: 200);
        var overpaid = await CreateWorkOrderWithCustomerAsync(client, xsrfToken, totalAmount: 300);
        await CreatePaymentAsync(client, xsrfToken, second.GetProperty("id").GetGuid(), amount: 50);
        await CreatePaymentAsync(client, xsrfToken, overpaid.GetProperty("id").GetGuid(), amount: 350);

        var payload = await GetDashboardSummaryAsync(client);
        var financialSummary = payload.GetProperty("financialSummary");

        Assert.Equal(250m, financialSummary.GetProperty("totalReceivable").GetDecimal());
        Assert.Equal(2, financialSummary.GetProperty("ordersWithPendingBalanceCount").GetInt32());
        Assert.Equal(1, financialSummary.GetProperty("overpaidOrdersCount").GetInt32());
    }

    [Fact]
    public async Task TotalReceivableExcludesCancelledWorkOrders()
    {
        using var factory = new TestApplicationFactory();
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var order = await CreateWorkOrderWithCustomerAsync(client, xsrfToken, totalAmount: 1000);
        await ChangeWorkOrderStatusAsync(client, xsrfToken, order.GetProperty("id").GetGuid(), "Cancelled");

        var payload = await GetDashboardSummaryAsync(client);
        var financialSummary = payload.GetProperty("financialSummary");

        Assert.Equal(0m, financialSummary.GetProperty("totalReceivable").GetDecimal());
        Assert.Equal(0, financialSummary.GetProperty("ordersWithPendingBalanceCount").GetInt32());
        Assert.Equal(0, financialSummary.GetProperty("unpaidOrdersCount").GetInt32());
    }

    [Fact]
    public async Task TotalReceivableIgnoresWorkOrdersWithoutTotalAmount()
    {
        using var factory = new TestApplicationFactory();
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        await CreateWorkOrderWithCustomerAsync(client, xsrfToken, totalAmount: null);

        var payload = await GetDashboardSummaryAsync(client);
        var financialSummary = payload.GetProperty("financialSummary");

        Assert.Equal(0m, financialSummary.GetProperty("totalReceivable").GetDecimal());
        Assert.Equal(0, financialSummary.GetProperty("unpaidOrdersCount").GetInt32());
    }

    [Fact]
    public async Task CancelledPaymentsDoNotCountForReceivableBalance()
    {
        using var factory = new TestApplicationFactory();
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var order = await CreateWorkOrderWithCustomerAsync(client, xsrfToken, totalAmount: 1000);
        var orderId = order.GetProperty("id").GetGuid();
        var cancelledPayment = await CreatePaymentAsync(client, xsrfToken, orderId, amount: 600);
        await CancelPaymentAsync(
            client,
            xsrfToken,
            orderId,
            cancelledPayment.GetProperty("payment").GetProperty("id").GetGuid());
        await CreatePaymentAsync(client, xsrfToken, orderId, amount: 100);

        var payload = await GetDashboardSummaryAsync(client);

        Assert.Equal(900m, payload.GetProperty("financialSummary").GetProperty("totalReceivable").GetDecimal());
        Assert.Equal(1, payload.GetProperty("financialSummary").GetProperty("cancelledPaymentsCount").GetInt32());
    }

    [Fact]
    public async Task OverdueCountExcludesDeliveredAndCancelledWorkOrders()
    {
        using var factory = new TestApplicationFactory();
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        await CreateWorkOrderWithCustomerAsync(client, xsrfToken, deliveryDate: Yesterday);
        var delivered = await CreateWorkOrderWithCustomerAsync(client, xsrfToken, deliveryDate: Yesterday);
        var cancelled = await CreateWorkOrderWithCustomerAsync(client, xsrfToken, deliveryDate: Yesterday);
        await ChangeWorkOrderStatusAsync(client, xsrfToken, delivered.GetProperty("id").GetGuid(), "Delivered");
        await ChangeWorkOrderStatusAsync(client, xsrfToken, cancelled.GetProperty("id").GetGuid(), "Cancelled");

        var payload = await GetDashboardSummaryAsync(client);

        Assert.Equal(1, payload.GetProperty("operationalSummary").GetProperty("overdueCount").GetInt32());
    }

    [Fact]
    public async Task DueTodayCountUsesFixedClockDate()
    {
        using var factory = new TestApplicationFactory();
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        await CreateWorkOrderWithCustomerAsync(client, xsrfToken, deliveryDate: Today);
        var deliveredToday = await CreateWorkOrderWithCustomerAsync(client, xsrfToken, deliveryDate: Today);
        await ChangeWorkOrderStatusAsync(client, xsrfToken, deliveredToday.GetProperty("id").GetGuid(), "Delivered");
        await CreateWorkOrderWithCustomerAsync(client, xsrfToken, deliveryDate: Tomorrow);

        var payload = await GetDashboardSummaryAsync(client);

        Assert.Equal(1, payload.GetProperty("operationalSummary").GetProperty("dueTodayCount").GetInt32());
    }

    [Fact]
    public async Task ByStatusReturnsWorkOrderCounts()
    {
        using var factory = new TestApplicationFactory();
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        await CreateWorkOrderWithCustomerAsync(client, xsrfToken);
        var inProcess = await CreateWorkOrderWithCustomerAsync(client, xsrfToken);
        var delivered = await CreateWorkOrderWithCustomerAsync(client, xsrfToken);
        await ChangeWorkOrderStatusAsync(client, xsrfToken, inProcess.GetProperty("id").GetGuid(), "InProcess");
        await ChangeWorkOrderStatusAsync(client, xsrfToken, delivered.GetProperty("id").GetGuid(), "Delivered");

        var payload = await GetDashboardSummaryAsync(client);
        var byStatus = payload.GetProperty("operationalSummary").GetProperty("byStatus").EnumerateArray().ToArray();

        Assert.Contains(byStatus, row =>
            row.GetProperty("status").GetString() == "Received"
            && row.GetProperty("count").GetInt32() == 1);
        Assert.Contains(byStatus, row =>
            row.GetProperty("status").GetString() == "InProcess"
            && row.GetProperty("count").GetInt32() == 1);
        Assert.Contains(byStatus, row =>
            row.GetProperty("status").GetString() == "Delivered"
            && row.GetProperty("count").GetInt32() == 1);
    }

    [Fact]
    public async Task LatestWorkOrdersAreLimitedToFive()
    {
        using var factory = new TestApplicationFactory();
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();

        for (var index = 0; index < 6; index++)
        {
            await CreateWorkOrderWithCustomerAsync(client, xsrfToken, patientName: UniqueName($"Paciente {index}"));
        }

        var payload = await GetDashboardSummaryAsync(client);
        var latestWorkOrders = payload
            .GetProperty("operationalSummary")
            .GetProperty("latestWorkOrders")
            .EnumerateArray()
            .ToArray();

        Assert.Equal(5, latestWorkOrders.Length);
    }

    [Fact]
    public async Task LatestPaymentsAreLimitedToFiveAndExcludeCancelledPayments()
    {
        using var factory = new TestApplicationFactory();
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var order = await CreateWorkOrderWithCustomerAsync(client, xsrfToken, totalAmount: 10000);
        var orderId = order.GetProperty("id").GetGuid();
        var cancelledPayment = await CreatePaymentAsync(client, xsrfToken, orderId, amount: 10);
        await CancelPaymentAsync(
            client,
            xsrfToken,
            orderId,
            cancelledPayment.GetProperty("payment").GetProperty("id").GetGuid());

        for (var index = 0; index < 5; index++)
        {
            await CreatePaymentAsync(client, xsrfToken, orderId, amount: 20 + index);
        }

        var payload = await GetDashboardSummaryAsync(client);
        var latestPayments = payload
            .GetProperty("financialSummary")
            .GetProperty("latestPayments")
            .EnumerateArray()
            .ToArray();

        Assert.Equal(5, latestPayments.Length);
        Assert.DoesNotContain(latestPayments, payment => payment.GetProperty("amount").GetDecimal() == 10m);
    }

    [Fact]
    public async Task HealthStillWorksWithDashboard()
    {
        using var factory = new TestApplicationFactory();
        var client = factory.CreateClientWithoutRedirects();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<JsonElement> GetDashboardSummaryAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/dashboard/summary");
        var json = await response.Content.ReadAsStringAsync();

        Assert.True(response.StatusCode == HttpStatusCode.OK, json);

        return JsonSerializer.Deserialize<JsonElement>(json);
    }

    private static async Task<JsonElement> CreateWorkOrderWithCustomerAsync(
        HttpClient client,
        string xsrfToken,
        DateOnly? deliveryDate = null,
        decimal? totalAmount = 1500,
        string? patientName = null)
    {
        var customer = await CreateCustomerAsync(client, xsrfToken, "Doctor", UniqueName("Dr Dashboard"));

        return await CreateWorkOrderAsync(
            client,
            xsrfToken,
            CreateWorkOrderRequest(
                customer.GetProperty("id").GetGuid(),
                deliveryDate,
                totalAmount,
                patientName));
    }

    private static object CreateWorkOrderRequest(
        Guid customerId,
        DateOnly? deliveryDate,
        decimal? totalAmount,
        string? patientName)
    {
        return new
        {
            customerId,
            internalDoctorId = (Guid?)null,
            patientName = patientName ?? UniqueName("Paciente Dashboard"),
            receivedDate = ReceivedDate,
            referenceNumber = UniqueName("REF"),
            workDescription = "Corona zirconia",
            dentalColor = "A2",
            firstTrialDate = (DateOnly?)null,
            secondTrialDate = (DateOnly?)null,
            deliveryDate = deliveryDate ?? Tomorrow,
            totalAmount,
            notes = "Observaciones de dashboard"
        };
    }

    private static async Task<JsonElement> CreateWorkOrderAsync(
        HttpClient client,
        string xsrfToken,
        object request)
    {
        var response = await client.PostAsJsonWithXsrfAsync("/api/work-orders", xsrfToken, request);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return payload;
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

    private static async Task UpdateCustomerStatusAsync(
        HttpClient client,
        string xsrfToken,
        Guid customerId,
        bool isActive)
    {
        var response = await client.PatchAsJsonWithXsrfAsync(
            $"/api/customers/{customerId}/status",
            xsrfToken,
            new { isActive });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task ChangeWorkOrderStatusAsync(
        HttpClient client,
        string xsrfToken,
        Guid orderId,
        string status)
    {
        var response = await client.PatchAsJsonWithXsrfAsync(
            $"/api/work-orders/{orderId}/status",
            xsrfToken,
            new { status, notes = $"Cambio a {status}" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<JsonElement> CreatePaymentAsync(
        HttpClient client,
        string xsrfToken,
        Guid workOrderId,
        decimal amount)
    {
        var response = await client.PostAsJsonWithXsrfAsync(
            $"/api/work-orders/{workOrderId}/payments",
            xsrfToken,
            new
            {
                paymentDate = Today,
                amount,
                method = "Cash",
                reference = UniqueName("PAY"),
                notes = "Abono de prueba"
            });
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return payload;
    }

    private static async Task CancelPaymentAsync(
        HttpClient client,
        string xsrfToken,
        Guid workOrderId,
        Guid paymentId)
    {
        var response = await client.PatchAsJsonWithXsrfAsync(
            $"/api/work-orders/{workOrderId}/payments/{paymentId}/cancel",
            xsrfToken,
            new { reason = "Cancelado para dashboard" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static string UniqueName(string prefix)
    {
        return $"{prefix} {Guid.NewGuid():N}";
    }
}
