using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LaboratorioTlahuac.Api.Tests;

public sealed class DeliveryIntegrationTests(TestApplicationFactory factory)
    : IClassFixture<TestApplicationFactory>
{
    private static readonly DateOnly ReceivedDate = new(2026, 5, 9);
    private static readonly DateOnly DeliveryDate = new(2026, 5, 12);

    [Fact]
    public async Task DeliveriesWithoutSessionReturnUnauthorized()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.GetXsrfTokenAsync();

        var listResponse = await client.GetAsync("/api/deliveries");
        var detailResponse = await client.GetAsync($"/api/deliveries/{Guid.NewGuid()}");
        var orderDeliveryResponse = await client.GetAsync($"/api/work-orders/{Guid.NewGuid()}/delivery");
        var createResponse = await client.PostAsJsonWithXsrfAsync(
            $"/api/work-orders/{Guid.NewGuid()}/delivery",
            xsrfToken,
            new { deliveryNotes = "Sin sesion" });
        var assignResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/deliveries/{Guid.NewGuid()}/assign",
            xsrfToken,
            new { assignedToUserId = Guid.NewGuid() });
        var retryResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/deliveries/{Guid.NewGuid()}/retry",
            xsrfToken,
            new { deliveryNotes = "Reintento sin sesion" });

        Assert.Equal(HttpStatusCode.Unauthorized, listResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, detailResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, orderDeliveryResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, assignResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, retryResponse.StatusCode);
    }

    [Fact]
    public async Task DeliveriesWithoutPermissionReturnForbidden()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsNoPermissionsUserAsync();

        var listResponse = await client.GetAsync("/api/deliveries");
        var createResponse = await client.PostAsJsonWithXsrfAsync(
            $"/api/work-orders/{Guid.NewGuid()}/delivery",
            xsrfToken,
            new { deliveryNotes = "Sin permiso" });
        var completeResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/deliveries/{Guid.NewGuid()}/complete",
            xsrfToken,
            new { recipientName = "Recepcion" });
        var retryResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/deliveries/{Guid.NewGuid()}/retry",
            xsrfToken,
            new { deliveryNotes = "Sin permiso" });

        Assert.Equal(HttpStatusCode.Forbidden, listResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, completeResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, retryResponse.StatusCode);
    }

    [Fact]
    public async Task AdminCanCreateListGetAssignMoveOutAndCompleteDelivery()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var driver = await CreateDriverAsync(client, xsrfToken);
        var workOrder = await CreateWorkOrderWithCustomerAsync(client, xsrfToken);
        var workOrderId = workOrder.GetProperty("id").GetGuid();

        var created = await CreateDeliveryAsync(client, xsrfToken, workOrderId);
        var deliveryId = created.GetProperty("id").GetGuid();

        Assert.Equal("PendingAssignment", created.GetProperty("status").GetString());
        Assert.Equal(workOrderId, created.GetProperty("workOrderId").GetGuid());
        Assert.Equal(workOrder.GetProperty("orderNumber").GetString(), created.GetProperty("orderNumber").GetString());
        Assert.Equal("Paciente Delivery", created.GetProperty("patientName").GetString());
        Assert.Equal("Corona zirconia", created.GetProperty("workSummary").GetString());
        Assert.Equal("Calle QA 123", created.GetProperty("customerAddress").GetString());

        var listResponse = await client.GetAsync("/api/deliveries");

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var list = await listResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(
            list.GetProperty("items").EnumerateArray(),
            delivery => delivery.GetProperty("id").GetGuid() == deliveryId);

        var detailResponse = await client.GetAsync($"/api/deliveries/{deliveryId}");
        var detail = await detailResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        Assert.Equal(deliveryId, detail.GetProperty("id").GetGuid());

        var orderDeliveryResponse = await client.GetAsync($"/api/work-orders/{workOrderId}/delivery");
        var orderDelivery = await orderDeliveryResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, orderDeliveryResponse.StatusCode);
        Assert.Equal(deliveryId, orderDelivery.GetProperty("id").GetGuid());

        var assigned = await AssignDeliveryAsync(client, xsrfToken, deliveryId, driver.UserId);

        Assert.Equal("Assigned", assigned.GetProperty("status").GetString());
        Assert.Equal(driver.UserId, assigned.GetProperty("assignedToUserId").GetGuid());
        Assert.Equal(driver.FullName, assigned.GetProperty("assignedToUserFullName").GetString());
        Assert.Equal(JsonValueKind.String, assigned.GetProperty("assignedAtUtc").ValueKind);

        var outForDeliveryResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/deliveries/{deliveryId}/out-for-delivery",
            xsrfToken,
            new { deliveryNotes = "Sale con paquete completo" });
        var outForDelivery = await outForDeliveryResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, outForDeliveryResponse.StatusCode);
        Assert.Equal("OutForDelivery", outForDelivery.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.String, outForDelivery.GetProperty("outForDeliveryAtUtc").ValueKind);

        var completeResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/deliveries/{deliveryId}/complete",
            xsrfToken,
            new { recipientName = "Dra Recepcion", deliveryNotes = "Entregado en recepcion" });
        var completed = await completeResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);
        Assert.Equal("Delivered", completed.GetProperty("status").GetString());
        Assert.Equal("Dra Recepcion", completed.GetProperty("recipientName").GetString());
        Assert.Equal("Delivered", completed.GetProperty("workOrderStatus").GetString());
        Assert.Equal(JsonValueKind.String, completed.GetProperty("deliveredAtUtc").ValueKind);
    }

    [Fact]
    public async Task AdminCanMarkDeliveryFailedWithReason()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var driver = await CreateDriverAsync(client, xsrfToken);
        var delivery = await CreateAssignedDeliveryAsync(client, xsrfToken, driver.UserId);
        var deliveryId = delivery.GetProperty("id").GetGuid();

        var response = await client.PatchAsJsonWithXsrfAsync(
            $"/api/deliveries/{deliveryId}/failed",
            xsrfToken,
            new { failedReason = "Cliente no disponible", deliveryNotes = "Se reagendara" });
        var failed = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("FailedDelivery", failed.GetProperty("status").GetString());
        Assert.Equal("Cliente no disponible", failed.GetProperty("failedReason").GetString());
        Assert.Equal(JsonValueKind.String, failed.GetProperty("failedAtUtc").ValueKind);
    }

    [Fact]
    public async Task AdminCanRetryFailedDelivery()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var driver = await CreateDriverAsync(client, xsrfToken);
        var assigned = await CreateAssignedDeliveryAsync(client, xsrfToken, driver.UserId);
        var deliveryId = assigned.GetProperty("id").GetGuid();

        await MarkFailedAsync(client, xsrfToken, deliveryId, "Cliente no estaba disponible");

        var response = await RetryDeliveryAsync(client, xsrfToken, deliveryId);

        Assert.Equal("OutForDelivery", response.GetProperty("status").GetString());
        Assert.Equal("Received", response.GetProperty("workOrderStatus").GetString());
        Assert.Equal(driver.UserId, response.GetProperty("assignedToUserId").GetGuid());
        Assert.Equal(JsonValueKind.String, response.GetProperty("outForDeliveryAtUtc").ValueKind);
        Assert.Equal(JsonValueKind.Null, response.GetProperty("failedAtUtc").ValueKind);
        Assert.Equal(JsonValueKind.Null, response.GetProperty("failedReason").ValueKind);
    }

    [Fact]
    public async Task AssignedDriverCanRetryFailedDeliveryAndCompleteIt()
    {
        var adminClient = factory.CreateClientWithoutRedirects();
        var adminXsrfToken = await adminClient.LoginAsAdminAsync();
        var driver = await CreateDriverAsync(adminClient, adminXsrfToken);
        var assigned = await CreateAssignedDeliveryAsync(adminClient, adminXsrfToken, driver.UserId);
        var deliveryId = assigned.GetProperty("id").GetGuid();

        await MarkFailedAsync(adminClient, adminXsrfToken, deliveryId, "Consultorio cerrado");

        var driverClient = factory.CreateClientWithoutRedirects();
        var driverXsrfToken = await LoginAsAsync(driverClient, driver.Email, driver.Password);

        var retried = await RetryDeliveryAsync(driverClient, driverXsrfToken, deliveryId);

        Assert.Equal("OutForDelivery", retried.GetProperty("status").GetString());
        Assert.Equal(driver.UserId, retried.GetProperty("assignedToUserId").GetGuid());

        var completeResponse = await driverClient.PatchAsJsonWithXsrfAsync(
            $"/api/deliveries/{deliveryId}/complete",
            driverXsrfToken,
            new { recipientName = "Recepcion segundo intento" });
        var completed = await completeResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);
        Assert.Equal("Delivered", completed.GetProperty("status").GetString());
        Assert.Equal("Delivered", completed.GetProperty("workOrderStatus").GetString());
        Assert.Equal("Recepcion segundo intento", completed.GetProperty("recipientName").GetString());
    }

    [Fact]
    public async Task UnassignedDriverCannotRetryAssignedFailedDelivery()
    {
        var adminClient = factory.CreateClientWithoutRedirects();
        var adminXsrfToken = await adminClient.LoginAsAdminAsync();
        var driver = await CreateDriverAsync(adminClient, adminXsrfToken);
        var otherDriver = await CreateDriverAsync(adminClient, adminXsrfToken);
        var assigned = await CreateAssignedDeliveryAsync(adminClient, adminXsrfToken, driver.UserId);
        var deliveryId = assigned.GetProperty("id").GetGuid();

        await MarkFailedAsync(adminClient, adminXsrfToken, deliveryId, "No habia responsable");

        var otherDriverClient = factory.CreateClientWithoutRedirects();
        var otherDriverXsrfToken = await LoginAsAsync(otherDriverClient, otherDriver.Email, otherDriver.Password);

        var response = await otherDriverClient.PatchAsJsonWithXsrfAsync(
            $"/api/deliveries/{deliveryId}/retry",
            otherDriverXsrfToken,
            new { deliveryNotes = "Intento ajeno" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RetryNonFailedDeliveryReturnsBadRequest()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var driver = await CreateDriverAsync(client, xsrfToken);
        var assigned = await CreateAssignedDeliveryAsync(client, xsrfToken, driver.UserId);

        var response = await client.PatchAsJsonWithXsrfAsync(
            $"/api/deliveries/{assigned.GetProperty("id").GetGuid()}/retry",
            xsrfToken,
            new { deliveryNotes = "No esta fallida" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RetryCancelledWorkOrderReturnsConflict()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var driver = await CreateDriverAsync(client, xsrfToken);
        var assigned = await CreateAssignedDeliveryAsync(client, xsrfToken, driver.UserId);
        var deliveryId = assigned.GetProperty("id").GetGuid();
        var workOrderId = assigned.GetProperty("workOrderId").GetGuid();

        await MarkFailedAsync(client, xsrfToken, deliveryId, "Cancelada despues del intento");
        await ChangeWorkOrderStatusAsync(client, xsrfToken, workOrderId, "Cancelled", "Cancelada para retry");

        var response = await client.PatchAsJsonWithXsrfAsync(
            $"/api/deliveries/{deliveryId}/retry",
            xsrfToken,
            new { deliveryNotes = "Reintento cancelado" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CompleteWithoutRecipientNameReturnsBadRequest()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var driver = await CreateDriverAsync(client, xsrfToken);
        var assigned = await CreateAssignedDeliveryAsync(client, xsrfToken, driver.UserId);
        var deliveryId = assigned.GetProperty("id").GetGuid();
        var outForDeliveryResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/deliveries/{deliveryId}/out-for-delivery",
            xsrfToken,
            new { deliveryNotes = "En ruta" });
        outForDeliveryResponse.EnsureSuccessStatusCode();

        var response = await client.PatchAsJsonWithXsrfAsync(
            $"/api/deliveries/{deliveryId}/complete",
            xsrfToken,
            new { recipientName = " " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task FailedWithoutReasonReturnsBadRequest()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var driver = await CreateDriverAsync(client, xsrfToken);
        var assigned = await CreateAssignedDeliveryAsync(client, xsrfToken, driver.UserId);

        var response = await client.PatchAsJsonWithXsrfAsync(
            $"/api/deliveries/{assigned.GetProperty("id").GetGuid()}/failed",
            xsrfToken,
            new { failedReason = " " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InvalidTransitionReturnsBadRequest()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var workOrder = await CreateWorkOrderWithCustomerAsync(client, xsrfToken);
        var delivery = await CreateDeliveryAsync(client, xsrfToken, workOrder.GetProperty("id").GetGuid());

        var response = await client.PatchAsJsonWithXsrfAsync(
            $"/api/deliveries/{delivery.GetProperty("id").GetGuid()}/out-for-delivery",
            xsrfToken,
            new { deliveryNotes = "Sin asignacion" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DriverCanViewAssignedDeliveriesAndComplete()
    {
        var adminClient = factory.CreateClientWithoutRedirects();
        var adminXsrfToken = await adminClient.LoginAsAdminAsync();
        var driver = await CreateDriverAsync(adminClient, adminXsrfToken);
        var assigned = await CreateAssignedDeliveryAsync(adminClient, adminXsrfToken, driver.UserId);
        var deliveryId = assigned.GetProperty("id").GetGuid();
        var outForDeliveryResponse = await adminClient.PatchAsJsonWithXsrfAsync(
            $"/api/deliveries/{deliveryId}/out-for-delivery",
            adminXsrfToken,
            new { deliveryNotes = "Salida registrada por admin" });
        outForDeliveryResponse.EnsureSuccessStatusCode();

        var driverClient = factory.CreateClientWithoutRedirects();
        var driverXsrfToken = await LoginAsAsync(driverClient, driver.Email, driver.Password);

        var mineResponse = await driverClient.GetAsync("/api/deliveries?assignedToMe=true");

        Assert.Equal(HttpStatusCode.OK, mineResponse.StatusCode);
        var mine = await mineResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(
            mine.GetProperty("items").EnumerateArray(),
            delivery => delivery.GetProperty("id").GetGuid() == deliveryId);

        var allVisibleResponse = await driverClient.GetAsync("/api/deliveries");

        Assert.Equal(HttpStatusCode.OK, allVisibleResponse.StatusCode);
        var allVisible = await allVisibleResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.All(
            allVisible.GetProperty("items").EnumerateArray(),
            delivery => Assert.Equal(driver.UserId, delivery.GetProperty("assignedToUserId").GetGuid()));

        var completeResponse = await driverClient.PatchAsJsonWithXsrfAsync(
            $"/api/deliveries/{deliveryId}/complete",
            driverXsrfToken,
            new { recipientName = "Recepcion movil" });
        var completed = await completeResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);
        Assert.Equal("Delivered", completed.GetProperty("status").GetString());
        Assert.Equal("Recepcion movil", completed.GetProperty("recipientName").GetString());
    }

    [Fact]
    public async Task DriverCannotAssignAnotherDriver()
    {
        var adminClient = factory.CreateClientWithoutRedirects();
        var adminXsrfToken = await adminClient.LoginAsAdminAsync();
        var driver = await CreateDriverAsync(adminClient, adminXsrfToken);
        var otherDriver = await CreateDriverAsync(adminClient, adminXsrfToken);
        var assigned = await CreateAssignedDeliveryAsync(adminClient, adminXsrfToken, driver.UserId);

        var driverClient = factory.CreateClientWithoutRedirects();
        var driverXsrfToken = await LoginAsAsync(driverClient, driver.Email, driver.Password);

        var response = await driverClient.PatchAsJsonWithXsrfAsync(
            $"/api/deliveries/{assigned.GetProperty("id").GetGuid()}/assign",
            driverXsrfToken,
            new { assignedToUserId = otherDriver.UserId });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task<JsonElement> CreateAssignedDeliveryAsync(
        HttpClient client,
        string xsrfToken,
        Guid driverUserId)
    {
        var workOrder = await CreateWorkOrderWithCustomerAsync(client, xsrfToken);
        var delivery = await CreateDeliveryAsync(client, xsrfToken, workOrder.GetProperty("id").GetGuid());

        return await AssignDeliveryAsync(client, xsrfToken, delivery.GetProperty("id").GetGuid(), driverUserId);
    }

    private static async Task<JsonElement> CreateDeliveryAsync(
        HttpClient client,
        string xsrfToken,
        Guid workOrderId)
    {
        var response = await client.PostAsJsonWithXsrfAsync(
            $"/api/work-orders/{workOrderId}/delivery",
            xsrfToken,
            new { deliveryNotes = "Entrega QA" });
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return payload;
    }

    private static async Task<JsonElement> AssignDeliveryAsync(
        HttpClient client,
        string xsrfToken,
        Guid deliveryId,
        Guid driverUserId)
    {
        var response = await client.PatchAsJsonWithXsrfAsync(
            $"/api/deliveries/{deliveryId}/assign",
            xsrfToken,
            new { assignedToUserId = driverUserId, deliveryNotes = "Asignada a repartidor" });
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return payload;
    }

    private static async Task<JsonElement> MarkFailedAsync(
        HttpClient client,
        string xsrfToken,
        Guid deliveryId,
        string failedReason)
    {
        var response = await client.PatchAsJsonWithXsrfAsync(
            $"/api/deliveries/{deliveryId}/failed",
            xsrfToken,
            new { failedReason });
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return payload;
    }

    private static async Task<JsonElement> RetryDeliveryAsync(
        HttpClient client,
        string xsrfToken,
        Guid deliveryId)
    {
        var response = await client.PatchAsJsonWithXsrfAsync(
            $"/api/deliveries/{deliveryId}/retry",
            xsrfToken,
            new { deliveryNotes = "Reintento QA" });
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return payload;
    }

    private static async Task ChangeWorkOrderStatusAsync(
        HttpClient client,
        string xsrfToken,
        Guid workOrderId,
        string status,
        string notes)
    {
        var response = await client.PatchAsJsonWithXsrfAsync(
            $"/api/work-orders/{workOrderId}/status",
            xsrfToken,
            new { status, notes });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<JsonElement> CreateWorkOrderWithCustomerAsync(HttpClient client, string xsrfToken)
    {
        var customer = await CreateCustomerAsync(client, xsrfToken, UniqueName("Dr Delivery"));

        return await CreateWorkOrderAsync(
            client,
            xsrfToken,
            CreateWorkOrderRequest(
                customer.GetProperty("id").GetGuid(),
                patientName: "Paciente Delivery"));
    }

    private static async Task<JsonElement> CreateCustomerAsync(
        HttpClient client,
        string xsrfToken,
        string displayName)
    {
        var response = await client.PostAsJsonWithXsrfAsync(
            "/api/customers",
            xsrfToken,
            new
            {
                type = "Doctor",
                displayName,
                contactName = "Recepcion QA",
                phone = "555-0101",
                whatsApp = "555-0102",
                address = "Calle QA 123"
            });
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return payload;
    }

    private static object CreateWorkOrderRequest(
        Guid customerId,
        string? patientName = null,
        string workDescription = "Corona zirconia")
    {
        return new
        {
            customerId,
            internalDoctorId = (Guid?)null,
            patientName = patientName ?? UniqueName("Paciente"),
            receivedDate = ReceivedDate,
            referenceNumber = UniqueName("REF"),
            workDescription,
            dentalColor = "A2",
            firstTrialDate = new DateOnly(2026, 5, 10),
            secondTrialDate = (DateOnly?)null,
            deliveryDate = DeliveryDate,
            totalAmount = 1500m,
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

    private static async Task<DriverUser> CreateDriverAsync(HttpClient client, string xsrfToken)
    {
        var driverRole = await GetRoleByNameAsync(client, "Repartidor");
        var email = UniqueEmail("driver");
        var password = "DriverPass123!";
        var fullName = $"Repartidor QA {Guid.NewGuid():N}";
        var response = await client.PostAsJsonWithXsrfAsync(
            "/api/admin/users",
            xsrfToken,
            new
            {
                email,
                fullName,
                temporaryPassword = password,
                roleIds = new[] { driverRole.GetProperty("id").GetGuid() }
            });
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return new DriverUser(payload.GetProperty("id").GetGuid(), email, password, fullName);
    }

    private static async Task<JsonElement> GetRoleByNameAsync(HttpClient client, string roleName)
    {
        var response = await client.GetAsync("/api/admin/roles");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return payload.EnumerateArray()
            .Single(role => role.GetProperty("name").GetString() == roleName);
    }

    private static async Task<string> LoginAsAsync(HttpClient client, string email, string password)
    {
        var xsrfToken = await client.GetXsrfTokenAsync();
        var response = await client.PostAsJsonWithXsrfAsync(
            "/api/auth/login",
            xsrfToken,
            new { email, password });

        response.EnsureSuccessStatusCode();

        return await client.GetXsrfTokenAsync();
    }

    private static string UniqueName(string prefix)
    {
        return $"{prefix} {Guid.NewGuid():N}";
    }

    private static string UniqueEmail(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid():N}@tests.local";
    }

    private sealed record DriverUser(Guid UserId, string Email, string Password, string FullName);
}
