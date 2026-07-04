using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LaboratorioTlahuac.Api.Tests;

public sealed class WorkOrderIntegrationTests(TestApplicationFactory factory)
    : IClassFixture<TestApplicationFactory>
{
    private static readonly DateOnly ReceivedDate = new(2026, 5, 9);
    private static readonly DateOnly DeliveryDate = new(2026, 5, 12);

    [Fact]
    public async Task WorkOrdersWithoutSessionReturnsUnauthorized()
    {
        var client = factory.CreateClientWithoutRedirects();

        var response = await client.GetAsync("/api/work-orders");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WorkOrdersWithSessionButWithoutViewPermissionReturnsForbidden()
    {
        var client = factory.CreateClientWithoutRedirects();
        await client.LoginAsLimitedUserAsync();

        var response = await client.GetAsync("/api/work-orders");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task WorkOrdersWithViewPermissionReturnsOk()
    {
        var client = factory.CreateClientWithoutRedirects();
        await client.LoginAsAdminAsync();

        var response = await client.GetAsync("/api/work-orders");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, payload.GetProperty("page").GetInt32());
    }

    [Fact]
    public async Task CreateWorkOrderWithoutXsrfReturnsBadRequest()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var customer = await CreateCustomerAsync(client, xsrfToken, "Doctor", UniqueName("Dr Sin XSRF"));

        var response = await client.PostAsJsonAsync(
            "/api/work-orders",
            CreateWorkOrderRequest(customer.GetProperty("id").GetGuid(), patientName: UniqueName("Paciente")));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkOrderWithPermissionAndXsrfCreatesWorkOrder()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var customer = await CreateCustomerAsync(client, xsrfToken, "Doctor", UniqueName("Dr Orden"));
        var patientName = UniqueName("Paciente Creacion");

        var created = await CreateWorkOrderAsync(
            client,
            xsrfToken,
            CreateWorkOrderRequest(customer.GetProperty("id").GetGuid(), patientName: patientName));

        Assert.StartsWith("OT-2026", created.GetProperty("orderNumber").GetString(), StringComparison.Ordinal);
        Assert.Equal("Received", created.GetProperty("status").GetString());
        Assert.Equal("Recibida", created.GetProperty("statusLabel").GetString());
        Assert.Equal(patientName, created.GetProperty("patientName").GetString());
    }

    [Fact]
    public async Task CreateWorkOrderWithInvalidRequestReturnsBadRequest()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var customer = await CreateCustomerAsync(client, xsrfToken, "Doctor", UniqueName("Dr Invalido"));

        var response = await client.PostAsJsonWithXsrfAsync(
            "/api/work-orders",
            xsrfToken,
            CreateWorkOrderRequest(
                customer.GetProperty("id").GetGuid(),
                patientName: " ",
                totalAmount: -1));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkOrderGeneratesUniqueOrderNumber()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var customer = await CreateCustomerAsync(client, xsrfToken, "Doctor", UniqueName("Dr Folios"));
        var customerId = customer.GetProperty("id").GetGuid();

        var first = await CreateWorkOrderAsync(
            client,
            xsrfToken,
            CreateWorkOrderRequest(customerId, patientName: UniqueName("Paciente Folio 1")));
        var second = await CreateWorkOrderAsync(
            client,
            xsrfToken,
            CreateWorkOrderRequest(customerId, patientName: UniqueName("Paciente Folio 2")));

        Assert.NotEqual(
            first.GetProperty("orderNumber").GetString(),
            second.GetProperty("orderNumber").GetString());
    }

    [Fact]
    public async Task CreateWorkOrderSetsInitialReceivedStatus()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var customer = await CreateCustomerAsync(client, xsrfToken, "Doctor", UniqueName("Dr Estado Inicial"));

        var created = await CreateWorkOrderAsync(
            client,
            xsrfToken,
            CreateWorkOrderRequest(customer.GetProperty("id").GetGuid(), patientName: UniqueName("Paciente Estado")));

        Assert.Equal("Received", created.GetProperty("status").GetString());
        Assert.False(created.GetProperty("isCancelled").GetBoolean());
    }

    [Fact]
    public async Task CreateWorkOrderCreatesInitialStatusHistory()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var customer = await CreateCustomerAsync(client, xsrfToken, "Doctor", UniqueName("Dr Historial Inicial"));

        var created = await CreateWorkOrderAsync(
            client,
            xsrfToken,
            CreateWorkOrderRequest(customer.GetProperty("id").GetGuid(), patientName: UniqueName("Paciente Historial")));
        var history = created.GetProperty("statusHistory").EnumerateArray().ToArray();

        Assert.Single(history);
        Assert.Equal("Received", history[0].GetProperty("toStatus").GetString());
        Assert.Equal(JsonValueKind.Null, history[0].GetProperty("fromStatus").ValueKind);
    }

    [Fact]
    public async Task CreateWorkOrderWithInternalDoctorFromAnotherCustomerFails()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var firstClinic = await CreateCustomerAsync(client, xsrfToken, "Clinic", UniqueName("Clinica Origen"));
        var secondClinic = await CreateCustomerAsync(client, xsrfToken, "Clinic", UniqueName("Clinica Destino"));
        var internalDoctor = await CreateInternalDoctorAsync(
            client,
            xsrfToken,
            firstClinic.GetProperty("id").GetGuid(),
            UniqueName("Dra Ajena"));

        var response = await client.PostAsJsonWithXsrfAsync(
            "/api/work-orders",
            xsrfToken,
            CreateWorkOrderRequest(
                secondClinic.GetProperty("id").GetGuid(),
                internalDoctorId: internalDoctor.GetProperty("id").GetGuid(),
                patientName: UniqueName("Paciente Ajeno")));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkOrderWithInternalDoctorForNonClinicCustomerFails()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var clinic = await CreateCustomerAsync(client, xsrfToken, "Clinic", UniqueName("Clinica Interno"));
        var doctorCustomer = await CreateCustomerAsync(client, xsrfToken, "Doctor", UniqueName("Dr No Clinica"));
        var internalDoctor = await CreateInternalDoctorAsync(
            client,
            xsrfToken,
            clinic.GetProperty("id").GetGuid(),
            UniqueName("Dra Interna"));

        var response = await client.PostAsJsonWithXsrfAsync(
            "/api/work-orders",
            xsrfToken,
            CreateWorkOrderRequest(
                doctorCustomer.GetProperty("id").GetGuid(),
                internalDoctorId: internalDoctor.GetProperty("id").GetGuid(),
                patientName: UniqueName("Paciente No Clinica")));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateWorkOrderChangesGeneralFields()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var customer = await CreateCustomerAsync(client, xsrfToken, "Doctor", UniqueName("Dr Editar"));
        var created = await CreateWorkOrderAsync(
            client,
            xsrfToken,
            CreateWorkOrderRequest(customer.GetProperty("id").GetGuid(), patientName: UniqueName("Paciente Original")));
        var orderId = created.GetProperty("id").GetGuid();
        var updatedPatient = UniqueName("Paciente Actualizado");

        var response = await client.PutAsJsonWithXsrfAsync(
            $"/api/work-orders/{orderId}",
            xsrfToken,
            CreateWorkOrderRequest(
                customer.GetProperty("id").GetGuid(),
                patientName: updatedPatient,
                workDescription: "Corona zirconia actualizada"));
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(updatedPatient, payload.GetProperty("patientName").GetString());
        Assert.Equal("Corona zirconia actualizada", payload.GetProperty("workDescription").GetString());
    }

    [Fact]
    public async Task UpdateWorkOrderDoesNotChangeStatus()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var customer = await CreateCustomerAsync(client, xsrfToken, "Doctor", UniqueName("Dr Status Put"));
        var created = await CreateWorkOrderAsync(
            client,
            xsrfToken,
            CreateWorkOrderRequest(customer.GetProperty("id").GetGuid(), patientName: UniqueName("Paciente Put")));
        var orderId = created.GetProperty("id").GetGuid();

        var statusResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/work-orders/{orderId}/status",
            xsrfToken,
            new { status = "InProcess" });
        statusResponse.EnsureSuccessStatusCode();

        var response = await client.PutAsJsonWithXsrfAsync(
            $"/api/work-orders/{orderId}",
            xsrfToken,
            new
            {
                customerId = customer.GetProperty("id").GetGuid(),
                internalDoctorId = (Guid?)null,
                patientName = UniqueName("Paciente Put Actualizado"),
                receivedDate = ReceivedDate,
                workDescription = "Trabajo actualizado",
                deliveryDate = DeliveryDate,
                status = "Delivered"
            });
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("InProcess", payload.GetProperty("status").GetString());
    }

    [Fact]
    public async Task ChangeStatusCreatesHistory()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var customer = await CreateCustomerAsync(client, xsrfToken, "Doctor", UniqueName("Dr Cambio Estado"));
        var created = await CreateWorkOrderAsync(
            client,
            xsrfToken,
            CreateWorkOrderRequest(customer.GetProperty("id").GetGuid(), patientName: UniqueName("Paciente Cambio")));
        var orderId = created.GetProperty("id").GetGuid();

        var response = await client.PatchAsJsonWithXsrfAsync(
            $"/api/work-orders/{orderId}/status",
            xsrfToken,
            new { status = "InProcess", notes = "Se inicia trabajo." });
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        var history = payload.GetProperty("statusHistory").EnumerateArray().ToArray();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("InProcess", payload.GetProperty("status").GetString());
        Assert.Equal(2, history.Length);
        Assert.Equal("InProcess", history[0].GetProperty("toStatus").GetString());
        Assert.Equal("Received", history[0].GetProperty("fromStatus").GetString());
    }

    [Fact]
    public async Task ChangeStatusToCancelledWithoutNotesFails()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var customer = await CreateCustomerAsync(client, xsrfToken, "Doctor", UniqueName("Dr Cancelar Sin Nota"));
        var created = await CreateWorkOrderAsync(
            client,
            xsrfToken,
            CreateWorkOrderRequest(customer.GetProperty("id").GetGuid(), patientName: UniqueName("Paciente Sin Nota")));

        var response = await client.PatchAsJsonWithXsrfAsync(
            $"/api/work-orders/{created.GetProperty("id").GetGuid()}/status",
            xsrfToken,
            new { status = "Cancelled", notes = " " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCancelledWorkOrderFails()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var customer = await CreateCustomerAsync(client, xsrfToken, "Doctor", UniqueName("Dr Cancelado Editar"));
        var created = await CreateWorkOrderAsync(
            client,
            xsrfToken,
            CreateWorkOrderRequest(customer.GetProperty("id").GetGuid(), patientName: UniqueName("Paciente Cancelado")));
        var orderId = created.GetProperty("id").GetGuid();

        var cancelResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/work-orders/{orderId}/status",
            xsrfToken,
            new { status = "Cancelled", notes = "Cancelado por solicitud." });
        cancelResponse.EnsureSuccessStatusCode();

        var response = await client.PutAsJsonWithXsrfAsync(
            $"/api/work-orders/{orderId}",
            xsrfToken,
            CreateWorkOrderRequest(
                customer.GetProperty("id").GetGuid(),
                patientName: UniqueName("Paciente No Edita")));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task WorkOrderListExcludesCancelledByDefault()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var customer = await CreateCustomerAsync(client, xsrfToken, "Doctor", UniqueName("Dr Lista Cancelado"));
        var patientName = UniqueName("Paciente Cancelado Default");
        var created = await CreateWorkOrderAsync(
            client,
            xsrfToken,
            CreateWorkOrderRequest(customer.GetProperty("id").GetGuid(), patientName: patientName));
        var orderId = created.GetProperty("id").GetGuid();

        var cancelResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/work-orders/{orderId}/status",
            xsrfToken,
            new { status = "Cancelled", notes = "Cancelado para prueba." });
        cancelResponse.EnsureSuccessStatusCode();

        var response = await client.GetAsync($"/api/work-orders?search={Uri.EscapeDataString(patientName)}");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(payload.GetProperty("items").EnumerateArray());
    }

    [Fact]
    public async Task WorkOrderListCanIncludeCancelled()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var customer = await CreateCustomerAsync(client, xsrfToken, "Doctor", UniqueName("Dr Lista Con Cancelado"));
        var patientName = UniqueName("Paciente Cancelado Incluido");
        var created = await CreateWorkOrderAsync(
            client,
            xsrfToken,
            CreateWorkOrderRequest(customer.GetProperty("id").GetGuid(), patientName: patientName));
        var orderId = created.GetProperty("id").GetGuid();

        var cancelResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/work-orders/{orderId}/status",
            xsrfToken,
            new { status = "Cancelled", notes = "Cancelado para incluir." });
        cancelResponse.EnsureSuccessStatusCode();

        var response = await client.GetAsync(
            $"/api/work-orders?includeCancelled=true&search={Uri.EscapeDataString(patientName)}");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        var items = payload.GetProperty("items").EnumerateArray().ToArray();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(items);
        Assert.True(items[0].GetProperty("isCancelled").GetBoolean());
    }

    [Fact]
    public async Task WorkOrderListIncludesDeliverySummaryWhenDeliveryExists()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var admin = await GetCurrentUserAsync(client);
        var customer = await CreateCustomerAsync(client, xsrfToken, "Doctor", UniqueName("Dr Lista Entrega"));
        var patientName = UniqueName("Paciente Lista Entrega");
        var created = await CreateWorkOrderAsync(
            client,
            xsrfToken,
            CreateWorkOrderRequest(customer.GetProperty("id").GetGuid(), patientName: patientName));
        var delivery = await CreateDeliveryAsync(client, xsrfToken, created.GetProperty("id").GetGuid());
        var deliveryId = delivery.GetProperty("id").GetGuid();

        await AssignDeliveryAsync(
            client,
            xsrfToken,
            deliveryId,
            admin.GetProperty("id").GetGuid());

        var response = await client.GetAsync(
            $"/api/work-orders?status=Received&search={Uri.EscapeDataString(patientName)}");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        var items = payload.GetProperty("items").EnumerateArray().ToArray();
        var item = Assert.Single(items);
        var deliverySummary = item.GetProperty("delivery");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Received", item.GetProperty("status").GetString());
        Assert.Equal(deliveryId, deliverySummary.GetProperty("deliveryId").GetGuid());
        Assert.Equal("Assigned", deliverySummary.GetProperty("deliveryStatus").GetString());
        Assert.Equal("Asignada", deliverySummary.GetProperty("deliveryStatusLabel").GetString());
        Assert.Equal(admin.GetProperty("fullName").GetString(), deliverySummary.GetProperty("assignedToUserName").GetString());
    }

    [Fact]
    public async Task WorkOrderListShowsFailedDeliveryAndKeepsWorkOrderStatus()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var admin = await GetCurrentUserAsync(client);
        var customer = await CreateCustomerAsync(client, xsrfToken, "Doctor", UniqueName("Dr Lista No Entregada"));
        var patientName = UniqueName("Paciente Lista No Entregada");
        var created = await CreateWorkOrderAsync(
            client,
            xsrfToken,
            CreateWorkOrderRequest(customer.GetProperty("id").GetGuid(), patientName: patientName));
        var delivery = await CreateDeliveryAsync(client, xsrfToken, created.GetProperty("id").GetGuid());
        var deliveryId = delivery.GetProperty("id").GetGuid();

        await AssignDeliveryAsync(
            client,
            xsrfToken,
            deliveryId,
            admin.GetProperty("id").GetGuid());

        var failedResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/deliveries/{deliveryId}/failed",
            xsrfToken,
            new { failedReason = "Cliente no disponible" });
        failedResponse.EnsureSuccessStatusCode();

        var response = await client.GetAsync($"/api/work-orders?search={Uri.EscapeDataString(patientName)}");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        var item = Assert.Single(payload.GetProperty("items").EnumerateArray().ToArray());
        var deliverySummary = item.GetProperty("delivery");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Received", item.GetProperty("status").GetString());
        Assert.Equal("Recibida", item.GetProperty("statusLabel").GetString());
        Assert.Equal("FailedDelivery", deliverySummary.GetProperty("deliveryStatus").GetString());
        Assert.Equal("No entregada", deliverySummary.GetProperty("deliveryStatusLabel").GetString());
        Assert.Equal(JsonValueKind.String, deliverySummary.GetProperty("failedAtUtc").ValueKind);
    }

    [Fact]
    public async Task WorkOrderListReturnsNullDeliveryWhenOrderHasNoDelivery()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var customer = await CreateCustomerAsync(client, xsrfToken, "Doctor", UniqueName("Dr Sin Entrega"));
        var patientName = UniqueName("Paciente Sin Entrega");

        await CreateWorkOrderAsync(
            client,
            xsrfToken,
            CreateWorkOrderRequest(customer.GetProperty("id").GetGuid(), patientName: patientName));

        var response = await client.GetAsync($"/api/work-orders?search={Uri.EscapeDataString(patientName)}");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        var item = Assert.Single(payload.GetProperty("items").EnumerateArray().ToArray());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(JsonValueKind.Null, item.GetProperty("delivery").ValueKind);
    }

    [Fact]
    public async Task WorkOrderStatusesReturnsSpanishLabels()
    {
        var client = factory.CreateClientWithoutRedirects();
        await client.LoginAsAdminAsync();

        var response = await client.GetAsync("/api/work-orders/statuses");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        var statuses = payload.EnumerateArray().ToArray();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(statuses, status =>
            status.GetProperty("value").GetString() == "Received"
            && status.GetProperty("label").GetString() == "Recibida");
        Assert.Contains(statuses, status =>
            status.GetProperty("value").GetString() == "Cancelled"
            && status.GetProperty("label").GetString() == "Cancelada");
    }

    [Fact]
    public async Task HealthStillWorksWithWorkOrders()
    {
        var client = factory.CreateClientWithoutRedirects();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static object CreateWorkOrderRequest(
        Guid customerId,
        Guid? internalDoctorId = null,
        string? patientName = null,
        string workDescription = "Corona zirconia",
        decimal? totalAmount = 1500)
    {
        return new
        {
            customerId,
            internalDoctorId,
            patientName = patientName ?? UniqueName("Paciente"),
            receivedDate = ReceivedDate,
            referenceNumber = UniqueName("REF"),
            workDescription,
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

    private static async Task<JsonElement> GetCurrentUserAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/auth/me");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return payload;
    }

    private static async Task<JsonElement> CreateDeliveryAsync(
        HttpClient client,
        string xsrfToken,
        Guid workOrderId)
    {
        var response = await client.PostAsJsonWithXsrfAsync(
            $"/api/work-orders/{workOrderId}/delivery",
            xsrfToken,
            new { deliveryNotes = "Entrega en listado" });
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return payload;
    }

    private static async Task AssignDeliveryAsync(
        HttpClient client,
        string xsrfToken,
        Guid deliveryId,
        Guid assignedToUserId)
    {
        var response = await client.PatchAsJsonWithXsrfAsync(
            $"/api/deliveries/{deliveryId}/assign",
            xsrfToken,
            new { assignedToUserId, deliveryNotes = "Asignada para listado" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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

    private static async Task<JsonElement> CreateInternalDoctorAsync(
        HttpClient client,
        string xsrfToken,
        Guid customerId,
        string fullName)
    {
        var response = await client.PostAsJsonWithXsrfAsync(
            $"/api/customers/{customerId}/internal-doctors",
            xsrfToken,
            new { fullName });
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return payload;
    }

    private static string UniqueName(string prefix)
    {
        return $"{prefix} {Guid.NewGuid():N}";
    }
}
