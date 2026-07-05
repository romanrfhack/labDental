using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LaboratorioTlahuac.Api.Tests;

public sealed class CatalogIntegrationTests(TestApplicationFactory factory)
    : IClassFixture<TestApplicationFactory>
{
    [Fact]
    public async Task PublicCatalogReturnsOkWithoutAuthenticationAndInitialSeed()
    {
        using var freshFactory = new TestApplicationFactory(
            new DateTimeOffset(2026, 5, 9, 12, 0, 0, TimeSpan.Zero));
        var client = freshFactory.CreateClientWithoutRedirects();

        var response = await client.GetAsync("/api/catalog/public");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(12, payload.GetProperty("sections").GetArrayLength());
        Assert.Equal(
            40,
            payload.GetProperty("sections")
                .EnumerateArray()
                .Sum(section => section.GetProperty("products").GetArrayLength()));
    }

    [Fact]
    public async Task PublicCatalogReturnsOnlyActiveSections()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var created = await CreateSectionAsync(client, xsrfToken, isActive: true);
        var sectionId = created.GetProperty("id").GetGuid();
        var sectionKey = created.GetProperty("key").GetString();

        var deactivateResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/admin/catalog/sections/{sectionId}/status",
            xsrfToken,
            new { isActive = false });
        deactivateResponse.EnsureSuccessStatusCode();

        var publicResponse = await client.GetAsync("/api/catalog/public");
        var publicCatalog = await publicResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, publicResponse.StatusCode);
        Assert.DoesNotContain(
            publicCatalog.GetProperty("sections").EnumerateArray(),
            section => section.GetProperty("key").GetString() == sectionKey);
    }

    [Fact]
    public async Task PublicCatalogReturnsOnlyActiveProducts()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var section = await GetSectionByKeyAsync(client, "zirconia");
        var product = await CreateProductAsync(client, xsrfToken, section.GetProperty("id").GetGuid());
        var productId = product.GetProperty("id").GetGuid();
        var productKey = product.GetProperty("key").GetString();

        var deactivateResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/admin/catalog/products/{productId}/status",
            xsrfToken,
            new { isActive = false });
        deactivateResponse.EnsureSuccessStatusCode();

        var publicResponse = await client.GetAsync("/api/catalog/public");
        var publicCatalog = await publicResponse.Content.ReadFromJsonAsync<JsonElement>();
        var publicProducts = publicCatalog.GetProperty("sections")
            .EnumerateArray()
            .SelectMany(section => section.GetProperty("products").EnumerateArray());

        Assert.Equal(HttpStatusCode.OK, publicResponse.StatusCode);
        Assert.DoesNotContain(
            publicProducts,
            currentProduct => currentProduct.GetProperty("key").GetString() == productKey);
    }

    [Fact]
    public async Task PublicCatalogRespectsSortOrderAndNameFallback()
    {
        using var freshFactory = new TestApplicationFactory(
            new DateTimeOffset(2026, 5, 9, 12, 0, 0, TimeSpan.Zero));
        var client = freshFactory.CreateClientWithoutRedirects();

        var response = await client.GetAsync("/api/catalog/public");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        var sections = payload.GetProperty("sections").EnumerateArray().ToArray();
        var zirconiaProducts = sections[0].GetProperty("products").EnumerateArray().ToArray();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("zirconia", sections[0].GetProperty("key").GetString());
        Assert.Equal("emax", sections[1].GetProperty("key").GetString());
        Assert.Equal("zirconia-corona-estratificada", zirconiaProducts[0].GetProperty("key").GetString());
        Assert.Equal("zirconia-corona-monolitica", zirconiaProducts[1].GetProperty("key").GetString());
    }

    [Fact]
    public async Task PublicCatalogDoesNotExposeAdministrativeFields()
    {
        var client = factory.CreateClientWithoutRedirects();

        var response = await client.GetAsync("/api/catalog/public");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        var section = payload.GetProperty("sections").EnumerateArray().First();
        var product = section.GetProperty("products").EnumerateArray().First();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(section.TryGetProperty("id", out _));
        Assert.False(section.TryGetProperty("sortOrder", out _));
        Assert.False(section.TryGetProperty("isActive", out _));
        Assert.False(section.TryGetProperty("createdAtUtc", out _));
        Assert.False(section.TryGetProperty("updatedAtUtc", out _));
        Assert.False(product.TryGetProperty("id", out _));
        Assert.False(product.TryGetProperty("catalogSectionId", out _));
        Assert.False(product.TryGetProperty("sortOrder", out _));
        Assert.False(product.TryGetProperty("isActive", out _));
        Assert.False(product.TryGetProperty("createdAtUtc", out _));
        Assert.False(product.TryGetProperty("updatedAtUtc", out _));
        Assert.True(product.TryGetProperty("priceAmount", out _));
        Assert.True(product.TryGetProperty("currency", out _));
        Assert.True(product.TryGetProperty("imagePath", out _));
        Assert.True(product.TryGetProperty("altText", out _));
    }

    [Fact]
    public async Task AdminCatalogSectionsWithoutSessionReturnsUnauthorized()
    {
        var client = factory.CreateClientWithoutRedirects();

        var response = await client.GetAsync("/api/admin/catalog/sections");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminCatalogSectionsWithoutPermissionReturnsForbidden()
    {
        var client = factory.CreateClientWithoutRedirects();
        await client.LoginAsNoPermissionsUserAsync();

        var response = await client.GetAsync("/api/admin/catalog/sections");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminCanListCreateUpdateAndToggleSections()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();

        var listResponse = await client.GetAsync("/api/admin/catalog/sections");
        var sections = await listResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.Contains(
            sections.EnumerateArray(),
            section => section.GetProperty("key").GetString() == "zirconia");

        var created = await CreateSectionAsync(client, xsrfToken, isActive: true);
        var sectionId = created.GetProperty("id").GetGuid();

        Assert.True(created.GetProperty("isActive").GetBoolean());

        var updateResponse = await client.PutAsJsonWithXsrfAsync(
            $"/api/admin/catalog/sections/{sectionId}",
            xsrfToken,
            new
            {
                key = created.GetProperty("key").GetString(),
                name = "Seccion QA actualizada",
                description = "Descripcion QA actualizada",
                imagePath = "assets/catalog/products/zirconia-corona-monolitica.webp",
                altText = "Imagen QA actualizada",
                sortOrder = 1001,
                isActive = true
            });
        var updated = await updateResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal("Seccion QA actualizada", updated.GetProperty("name").GetString());
        Assert.Equal("assets/catalog/products/zirconia-corona-monolitica.webp", updated.GetProperty("imagePath").GetString());

        var deactivateResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/admin/catalog/sections/{sectionId}/status",
            xsrfToken,
            new { isActive = false });
        var deactivated = await deactivateResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);
        Assert.False(deactivated.GetProperty("isActive").GetBoolean());

        var activateResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/admin/catalog/sections/{sectionId}/status",
            xsrfToken,
            new { isActive = true });
        var activated = await activateResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, activateResponse.StatusCode);
        Assert.True(activated.GetProperty("isActive").GetBoolean());
    }

    [Fact]
    public async Task AdminCanListCreateUpdateToggleAndPriceProducts()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var section = await GetSectionByKeyAsync(client, "emax");
        var sectionId = section.GetProperty("id").GetGuid();

        var listResponse = await client.GetAsync($"/api/admin/catalog/products?sectionId={sectionId}");
        var products = await listResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.All(
            products.EnumerateArray(),
            product => Assert.Equal(sectionId, product.GetProperty("catalogSectionId").GetGuid()));

        var created = await CreateProductAsync(client, xsrfToken, sectionId);
        var productId = created.GetProperty("id").GetGuid();

        Assert.Equal("MXN", created.GetProperty("currency").GetString());
        Assert.True(created.GetProperty("isActive").GetBoolean());

        var updateResponse = await client.PutAsJsonWithXsrfAsync(
            $"/api/admin/catalog/products/{productId}",
            xsrfToken,
            new
            {
                catalogSectionId = sectionId,
                key = created.GetProperty("key").GetString(),
                name = "Producto QA actualizado",
                description = "Descripcion producto QA",
                priceAmount = 987.65m,
                currency = "MXN",
                imagePath = "assets/catalog/products/emax-incrustacion.webp",
                altText = "Imagen producto QA",
                sortOrder = 1002,
                isActive = true
            });
        var updated = await updateResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal("Producto QA actualizado", updated.GetProperty("name").GetString());
        Assert.Equal(987.65m, updated.GetProperty("priceAmount").GetDecimal());

        var deactivateResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/admin/catalog/products/{productId}/status",
            xsrfToken,
            new { isActive = false });
        var deactivated = await deactivateResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);
        Assert.False(deactivated.GetProperty("isActive").GetBoolean());

        var activateResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/admin/catalog/products/{productId}/status",
            xsrfToken,
            new { isActive = true });
        var activated = await activateResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, activateResponse.StatusCode);
        Assert.True(activated.GetProperty("isActive").GetBoolean());

        var priceResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/admin/catalog/products/{productId}/price",
            xsrfToken,
            new { priceAmount = 1234.50m, currency = "MXN" });
        var priced = await priceResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, priceResponse.StatusCode);
        Assert.Equal(1234.50m, priced.GetProperty("priceAmount").GetDecimal());
        Assert.Equal("MXN", priced.GetProperty("currency").GetString());
    }

    [Fact]
    public async Task CatalogRejectsNegativePriceAndUnsafeImagePath()
    {
        var client = factory.CreateClientWithoutRedirects();
        var xsrfToken = await client.LoginAsAdminAsync();
        var section = await GetSectionByKeyAsync(client, "signum");
        var sectionId = section.GetProperty("id").GetGuid();
        var product = await CreateProductAsync(client, xsrfToken, sectionId);
        var productId = product.GetProperty("id").GetGuid();

        var negativePriceResponse = await client.PatchAsJsonWithXsrfAsync(
            $"/api/admin/catalog/products/{productId}/price",
            xsrfToken,
            new { priceAmount = -1m, currency = "MXN" });

        Assert.Equal(HttpStatusCode.BadRequest, negativePriceResponse.StatusCode);

        var unsafeImageResponse = await client.PostAsJsonWithXsrfAsync(
            "/api/admin/catalog/products",
            xsrfToken,
            new
            {
                catalogSectionId = sectionId,
                key = UniqueKey("unsafe-image"),
                name = "Producto con imagen insegura",
                priceAmount = 100m,
                currency = "MXN",
                imagePath = "https://example.com/catalog.webp",
                altText = "Imagen insegura",
                sortOrder = 1003,
                isActive = true
            });

        Assert.Equal(HttpStatusCode.BadRequest, unsafeImageResponse.StatusCode);
    }

    [Fact]
    public async Task DriverCannotAdministerCatalog()
    {
        var adminClient = factory.CreateClientWithoutRedirects();
        var adminXsrfToken = await adminClient.LoginAsAdminAsync();
        var driver = await CreateDriverAsync(adminClient, adminXsrfToken);

        var driverClient = factory.CreateClientWithoutRedirects();
        var driverXsrfToken = await LoginAsAsync(driverClient, driver.Email, driver.Password);

        var listResponse = await driverClient.GetAsync("/api/admin/catalog/sections");
        var createResponse = await driverClient.PostAsJsonWithXsrfAsync(
            "/api/admin/catalog/sections",
            driverXsrfToken,
            new
            {
                key = UniqueKey("driver-section"),
                name = "Seccion Driver",
                sortOrder = 1004,
                isActive = true
            });

        Assert.Equal(HttpStatusCode.Forbidden, listResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);
    }

    private static async Task<JsonElement> CreateSectionAsync(
        HttpClient client,
        string xsrfToken,
        bool isActive)
    {
        var response = await client.PostAsJsonWithXsrfAsync(
            "/api/admin/catalog/sections",
            xsrfToken,
            new
            {
                key = UniqueKey("qa-section"),
                name = "Seccion QA",
                description = "Descripcion QA",
                imagePath = "assets/catalog/products/zirconia-corona-estratificada.webp",
                altText = "Imagen QA",
                sortOrder = 1000,
                isActive
            });
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return payload;
    }

    private static async Task<JsonElement> CreateProductAsync(
        HttpClient client,
        string xsrfToken,
        Guid sectionId)
    {
        var response = await client.PostAsJsonWithXsrfAsync(
            "/api/admin/catalog/products",
            xsrfToken,
            new
            {
                catalogSectionId = sectionId,
                key = UniqueKey("qa-product"),
                name = "Producto QA",
                description = "Descripcion producto QA",
                priceAmount = 456.78m,
                currency = (string?)null,
                imagePath = "assets/catalog/products/zirconia-corona-estratificada.webp",
                altText = "Producto QA",
                sortOrder = 1000,
                isActive = true
            });
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return payload;
    }

    private static async Task<JsonElement> GetSectionByKeyAsync(HttpClient client, string key)
    {
        var response = await client.GetAsync("/api/admin/catalog/sections");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return payload.EnumerateArray()
            .Single(section => section.GetProperty("key").GetString() == key);
    }

    private static async Task<DriverUser> CreateDriverAsync(HttpClient client, string xsrfToken)
    {
        var driverRole = await GetRoleByNameAsync(client, "Repartidor");
        var email = $"{UniqueKey("driver")}@tests.local";
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

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return new DriverUser(email, password);
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

    private static string UniqueKey(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid():N}";
    }

    private sealed record DriverUser(string Email, string Password);
}
