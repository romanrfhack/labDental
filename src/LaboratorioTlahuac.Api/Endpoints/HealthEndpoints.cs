namespace LaboratorioTlahuac.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet("/health", () => Results.Ok(new HealthResponse("Healthy", "LaboratorioTlahuac.Api")))
            .AllowAnonymous()
            .WithName("Health");

        return endpoints;
    }
}

public sealed record HealthResponse(string Status, string Application);
