using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LaboratorioTlahuac.Api.Tests;

public sealed class PaymentIntegrationTests(TestApplicationFactory factory)
    : IClassFixture<TestApplicationFactory>
{
    private static readonly DateOnly ReceivedDate = new(2026, 5, 9);
    private static readonly DateOnly DeliveryDate = new(2026, 5, 12);
    private static readonly DateOnly PaymentDate = new(2026, 5, 9);

    [Fact]
    public async Task WorkOrderPaymentsWithoutSessionReturnsUnauthorized()
    {
        var client = factory.CreateClientWithoutRedirects();

        var response = await client.GetAsync($"/api/work-orders/{Guid.NewGuid()}/payments");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WorkOrderPaymentsWithSessionButWithoutViewPermissionReturnsForbidden()
    {
        var client = factory.CreateClientWithoutRedirects();
        await client.LoginAsLimitedUserAsync();

        var response = await client.GetAsync($"/api/work-orders/{Guid.NewGuid()}/payments");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task WorkOrderPaymentsWithViewPermissionReturnsOk()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var workOrder = await CreateWorkOrderWithCustomerAsync(client, xsrfToken, totalAmount: 1500);

        var response = await client.GetAsync($"/api/work-orders/{workOrder.GetProperty("id").GetGuid()}/payments");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(JsonValueKind.Array, payload.ValueKind);
    }

    [Fact]
    public async Task CreatePaymentWithoutXsrfReturnsBadRequest()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var workOrder = await CreateWorkOrderWithCustomerAsync(client, xsrfToken, totalAmount: 1500);

        var response = await client.PostAsJsonAsync(
            $"/api/work-orders/{workOrder.GetProperty("id").GetGuid()}/payments",
            CreatePaymentRequest(amount: 1000));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePaymentWithoutCreatePermissionReturnsForbidden()
    {
        var adminClient = factory.CreateClientWithoutRedirects();
        var adminXsrfToken = await adminClient.LoginAsAdminAsync();
        var workOrder = await CreateWorkOrderWithCustomerAsync(adminClient, adminXsrfToken, totalAmount: 1500);

        var limitedClient = factory.CreateClientWithoutRedirects();
        var limitedXsrfToken = await limitedClient.LoginAsLimitedUserAsync();

        var response = await limitedClient.PostAsJsonWithXsrfAsync(
            $"/api/work-orders/{workOrder.GetProperty("id").GetGuid()}/payments",
            limitedXsrfToken,
            CreatePaymentRequest(amount: 1000));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreatePaymentWithPermissionAndXsrfCreatesPayment()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var workOrder = await CreateWorkOrderWithCustomerAsync(client, xsrfToken, totalAmount: 1500);

        var created = await CreatePaymentAsync(
            client,
            xsrfToken,
            workOrder.GetProperty("id").GetGuid(),
            CreatePaymentRequest(amount: 1000, reference: UniqueName("PAY")));

        Assert.Equal("Cash", created.GetProperty("payment").GetProperty("method").GetString());
        Assert.Equal("Efectivo", created.GetProperty("payment").GetProperty("methodLabel").GetString());
        Assert.Equal(1000m, created.GetProperty("summary").GetProperty("paidAmount").GetDecimal());
        Assert.Equal("Partial", created.GetProperty("summary").GetProperty("paymentStatus").GetString());
    }

    [Fact]
    public async Task CreatePaymentWithInvalidAmountReturnsBadRequest()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var workOrder = await CreateWorkOrderWithCustomerAsync(client, xsrfToken, totalAmount: 1500);

        var response = await client.PostAsJsonWithXsrfAsync(
            $"/api/work-orders/{workOrder.GetProperty("id").GetGuid()}/payments",
            xsrfToken,
            CreatePaymentRequest(amount: 0));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePaymentWithTotalAmountNotSetReturnsConflict()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var workOrder = await CreateWorkOrderWithCustomerAsync(client, xsrfToken, totalAmount: null);

        var response = await client.PostAsJsonWithXsrfAsync(
            $"/api/work-orders/{workOrder.GetProperty("id").GetGuid()}/payments",
            xsrfToken,
            CreatePaymentRequest(amount: 1000));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreatePaymentOnCancelledWorkOrderReturnsConflict()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var workOrder = await CreateWorkOrderWithCustomerAsync(client, xsrfToken, totalAmount: 1500);
        var workOrderId = workOrder.GetProperty("id").GetGuid();

        var cancelOrderResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/work-orders/{workOrderId}/status",
            xsrfToken,
            new { status = "Cancelled", notes = "Cancelada para prueba de pagos." });
        cancelOrderResponse.EnsureSuccessStatusCode();

        var response = await client.PostAsJsonWithXsrfAsync(
            $"/api/work-orders/{workOrderId}/payments",
            xsrfToken,
            CreatePaymentRequest(amount: 1000));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreatePaymentAllowsOverpaymentAndSummaryIsOverpaid()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var workOrder = await CreateWorkOrderWithCustomerAsync(client, xsrfToken, totalAmount: 100);

        var created = await CreatePaymentAsync(
            client,
            xsrfToken,
            workOrder.GetProperty("id").GetGuid(),
            CreatePaymentRequest(amount: 125));

        Assert.Equal("Overpaid", created.GetProperty("summary").GetProperty("paymentStatus").GetString());
        Assert.Equal("Saldo a favor / revisar", created.GetProperty("summary").GetProperty("paymentStatusLabel").GetString());
        Assert.Equal(-25m, created.GetProperty("summary").GetProperty("balance").GetDecimal());
    }

    [Fact]
    public async Task SummaryCalculatesPaidAmountAndBalance()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var workOrder = await CreateWorkOrderWithCustomerAsync(client, xsrfToken, totalAmount: 1500);
        var workOrderId = workOrder.GetProperty("id").GetGuid();
        await CreatePaymentAsync(client, xsrfToken, workOrderId, CreatePaymentRequest(amount: 1000));

        var response = await client.GetAsync($"/api/work-orders/{workOrderId}/payments/summary");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1000m, payload.GetProperty("paidAmount").GetDecimal());
        Assert.Equal(500m, payload.GetProperty("balance").GetDecimal());
        Assert.Equal("Partial", payload.GetProperty("paymentStatus").GetString());
    }

    [Fact]
    public async Task SummaryIgnoresCancelledPayments()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var workOrder = await CreateWorkOrderWithCustomerAsync(client, xsrfToken, totalAmount: 1000);
        var workOrderId = workOrder.GetProperty("id").GetGuid();
        await CreatePaymentAsync(client, xsrfToken, workOrderId, CreatePaymentRequest(amount: 400));
        var second = await CreatePaymentAsync(client, xsrfToken, workOrderId, CreatePaymentRequest(amount: 600));
        var secondPaymentId = second.GetProperty("payment").GetProperty("id").GetGuid();

        var cancelResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/work-orders/{workOrderId}/payments/{secondPaymentId}/cancel",
            xsrfToken,
            new { reason = "Captura incorrecta" });
        cancelResponse.EnsureSuccessStatusCode();

        var response = await client.GetAsync($"/api/work-orders/{workOrderId}/payments/summary");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(400m, payload.GetProperty("paidAmount").GetDecimal());
        Assert.Equal(600m, payload.GetProperty("balance").GetDecimal());
        Assert.Equal(1, payload.GetProperty("activePaymentsCount").GetInt32());
        Assert.Equal(1, payload.GetProperty("cancelledPaymentsCount").GetInt32());
    }

    [Fact]
    public async Task CancelPaymentWithoutXsrfReturnsBadRequest()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var workOrder = await CreateWorkOrderWithCustomerAsync(client, xsrfToken, totalAmount: 1500);
        var workOrderId = workOrder.GetProperty("id").GetGuid();
        var payment = await CreatePaymentAsync(client, xsrfToken, workOrderId, CreatePaymentRequest(amount: 1000));
        var paymentId = payment.GetProperty("payment").GetProperty("id").GetGuid();

        var response = await client.PatchAsJsonAsync(
            $"/api/work-orders/{workOrderId}/payments/{paymentId}/cancel",
            new { reason = "Sin XSRF" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CancelPaymentWithoutCancelPermissionReturnsForbidden()
    {
        var adminClient = factory.CreateClientWithoutRedirects();
        var adminXsrfToken = await adminClient.LoginAsAdminAsync();
        var workOrder = await CreateWorkOrderWithCustomerAsync(adminClient, adminXsrfToken, totalAmount: 1500);
        var workOrderId = workOrder.GetProperty("id").GetGuid();
        var payment = await CreatePaymentAsync(adminClient, adminXsrfToken, workOrderId, CreatePaymentRequest(amount: 1000));
        var paymentId = payment.GetProperty("payment").GetProperty("id").GetGuid();

        var limitedClient = factory.CreateClientWithoutRedirects();
        var limitedXsrfToken = await limitedClient.LoginAsLimitedUserAsync();

        var response = await limitedClient.PatchAsJsonWithXsrfAsync(
            $"/api/work-orders/{workOrderId}/payments/{paymentId}/cancel",
            limitedXsrfToken,
            new { reason = "Sin permiso" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CancelPaymentWithReasonCancelsPayment()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var workOrder = await CreateWorkOrderWithCustomerAsync(client, xsrfToken, totalAmount: 1500);
        var workOrderId = workOrder.GetProperty("id").GetGuid();
        var payment = await CreatePaymentAsync(client, xsrfToken, workOrderId, CreatePaymentRequest(amount: 1000));
        var paymentId = payment.GetProperty("payment").GetProperty("id").GetGuid();

        var response = await client.PatchAsJsonWithXsrfAsync(
            $"/api/work-orders/{workOrderId}/payments/{paymentId}/cancel",
            xsrfToken,
            new { reason = "Captura incorrecta" });
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(payload.GetProperty("payment").GetProperty("isCancelled").GetBoolean());
        Assert.Equal("Captura incorrecta", payload.GetProperty("payment").GetProperty("cancellationReason").GetString());
        Assert.Equal(0m, payload.GetProperty("summary").GetProperty("paidAmount").GetDecimal());
    }

    [Fact]
    public async Task CancelPaymentWithoutReasonReturnsBadRequest()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var workOrder = await CreateWorkOrderWithCustomerAsync(client, xsrfToken, totalAmount: 1500);
        var workOrderId = workOrder.GetProperty("id").GetGuid();
        var payment = await CreatePaymentAsync(client, xsrfToken, workOrderId, CreatePaymentRequest(amount: 1000));
        var paymentId = payment.GetProperty("payment").GetProperty("id").GetGuid();

        var response = await client.PatchAsJsonWithXsrfAsync(
            $"/api/work-orders/{workOrderId}/payments/{paymentId}/cancel",
            xsrfToken,
            new { reason = " " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CancelAlreadyCancelledPaymentReturnsConflict()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var workOrder = await CreateWorkOrderWithCustomerAsync(client, xsrfToken, totalAmount: 1500);
        var workOrderId = workOrder.GetProperty("id").GetGuid();
        var payment = await CreatePaymentAsync(client, xsrfToken, workOrderId, CreatePaymentRequest(amount: 1000));
        var paymentId = payment.GetProperty("payment").GetProperty("id").GetGuid();

        var firstResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/work-orders/{workOrderId}/payments/{paymentId}/cancel",
            xsrfToken,
            new { reason = "Primera cancelacion" });
        firstResponse.EnsureSuccessStatusCode();

        var response = await client.PatchAsJsonWithXsrfAsync(
            $"/api/work-orders/{workOrderId}/payments/{paymentId}/cancel",
            xsrfToken,
            new { reason = "Segunda cancelacion" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task PaymentsListReturnsPayments()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var reference = UniqueName("LIST");
        var workOrder = await CreateWorkOrderWithCustomerAsync(client, xsrfToken, totalAmount: 1500);
        await CreatePaymentAsync(
            client,
            xsrfToken,
            workOrder.GetProperty("id").GetGuid(),
            CreatePaymentRequest(amount: 1000, reference: reference));

        var response = await client.GetAsync($"/api/payments?search={Uri.EscapeDataString(reference)}");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        var items = payload.GetProperty("items").EnumerateArray().ToArray();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(items);
        Assert.Equal(reference, items[0].GetProperty("reference").GetString());
    }

    [Fact]
    public async Task PaymentsListExcludesCancelledByDefault()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var reference = UniqueName("CANCELLED-DEFAULT");
        var workOrder = await CreateWorkOrderWithCustomerAsync(client, xsrfToken, totalAmount: 1500);
        var workOrderId = workOrder.GetProperty("id").GetGuid();
        var payment = await CreatePaymentAsync(
            client,
            xsrfToken,
            workOrderId,
            CreatePaymentRequest(amount: 1000, reference: reference));
        var paymentId = payment.GetProperty("payment").GetProperty("id").GetGuid();
        var cancelResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/work-orders/{workOrderId}/payments/{paymentId}/cancel",
            xsrfToken,
            new { reason = "Cancelado para filtro default" });
        cancelResponse.EnsureSuccessStatusCode();

        var response = await client.GetAsync($"/api/payments?search={Uri.EscapeDataString(reference)}");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(payload.GetProperty("items").EnumerateArray());
    }

    [Fact]
    public async Task PaymentsListIncludesCancelledWhenRequested()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var reference = UniqueName("CANCELLED-INCLUDED");
        var workOrder = await CreateWorkOrderWithCustomerAsync(client, xsrfToken, totalAmount: 1500);
        var workOrderId = workOrder.GetProperty("id").GetGuid();
        var payment = await CreatePaymentAsync(
            client,
            xsrfToken,
            workOrderId,
            CreatePaymentRequest(amount: 1000, reference: reference));
        var paymentId = payment.GetProperty("payment").GetProperty("id").GetGuid();
        var cancelResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/work-orders/{workOrderId}/payments/{paymentId}/cancel",
            xsrfToken,
            new { reason = "Cancelado para filtro incluido" });
        cancelResponse.EnsureSuccessStatusCode();

        var response = await client.GetAsync(
            $"/api/payments?includeCancelled=true&search={Uri.EscapeDataString(reference)}");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        var items = payload.GetProperty("items").EnumerateArray().ToArray();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(items);
        Assert.True(items[0].GetProperty("isCancelled").GetBoolean());
    }

    [Fact]
    public async Task PaymentMethodsReturnsSpanishLabels()
    {
        var client = factory.CreateClientWithoutRedirects();
        await client.LoginAsAdminAsync();

        var response = await client.GetAsync("/api/payments/methods");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        var methods = payload.EnumerateArray().ToArray();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(methods, method =>
            method.GetProperty("value").GetString() == "Cash"
            && method.GetProperty("label").GetString() == "Efectivo");
        Assert.Contains(methods, method =>
            method.GetProperty("value").GetString() == "BankTransfer"
            && method.GetProperty("label").GetString() == "Transferencia");
    }

    [Fact]
    public async Task HealthStillWorksWithPayments()
    {
        var client = factory.CreateClientWithoutRedirects();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static object CreatePaymentRequest(
        decimal amount,
        string method = "Cash",
        string? reference = null,
        string? notes = "Abono de prueba")
    {
        return new
        {
            paymentDate = PaymentDate,
            amount,
            method,
            reference,
            notes
        };
    }

    private static async Task<JsonElement> CreatePaymentAsync(
        HttpClient client,
        string xsrfToken,
        Guid workOrderId,
        object request)
    {
        var response = await client.PostAsJsonWithXsrfAsync(
            $"/api/work-orders/{workOrderId}/payments",
            xsrfToken,
            request);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return payload;
    }

    private static async Task<JsonElement> CreateWorkOrderWithCustomerAsync(
        HttpClient client,
        string xsrfToken,
        decimal? totalAmount)
    {
        var customer = await CreateCustomerAsync(client, xsrfToken, "Doctor", UniqueName("Dr Pagos"));

        return await CreateWorkOrderAsync(
            client,
            xsrfToken,
            CreateWorkOrderRequest(
                customer.GetProperty("id").GetGuid(),
                patientName: UniqueName("Paciente Pago"),
                totalAmount: totalAmount));
    }

    private static object CreateWorkOrderRequest(
        Guid customerId,
        string? patientName = null,
        decimal? totalAmount = 1500)
    {
        return new
        {
            customerId,
            internalDoctorId = (Guid?)null,
            patientName = patientName ?? UniqueName("Paciente"),
            receivedDate = ReceivedDate,
            referenceNumber = UniqueName("REF"),
            workDescription = "Corona zirconia",
            dentalColor = "A2",
            firstTrialDate = new DateOnly(2026, 5, 10),
            secondTrialDate = (DateOnly?)null,
            deliveryDate = DeliveryDate,
            totalAmount,
            notes = "Observaciones de prueba"
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

    private static string UniqueName(string prefix)
    {
        return $"{prefix} {Guid.NewGuid():N}";
    }
}
