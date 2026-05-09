using LaboratorioTlahuac.Domain.Security;

namespace LaboratorioTlahuac.Api.Endpoints;

public static class SecurityDiagnosticEndpoints
{
    public static IEndpointRouteBuilder MapSecurityDiagnosticEndpoints(
        this IEndpointRouteBuilder endpoints,
        IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
        {
            return endpoints;
        }

        endpoints
            .MapGet(
                "/api/security/permissions-check",
                () => Results.Ok(new PermissionCheckResponse(Permissions.UsersManage)))
            .RequireAuthorization(Permissions.UsersManage)
            .WithName("SecurityPermissionsCheck")
            .WithTags("Security");

        endpoints
            .MapPost(
                "/api/security/csrf-check",
                () => Results.Ok(new CsrfCheckResponse("ok")))
            .RequireAuthorization(Permissions.UsersManage)
            .WithName("SecurityCsrfCheck")
            .WithTags("Security");

        return endpoints;
    }
}

public sealed record PermissionCheckResponse(string RequiredPermission);

public sealed record CsrfCheckResponse(string Status);
