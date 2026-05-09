using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Antiforgery;
using LaboratorioTlahuac.Application.Abstractions.Authentication;
using LaboratorioTlahuac.Application.Authentication;
using LaboratorioTlahuac.Domain.Security;

namespace LaboratorioTlahuac.Api.Endpoints;

public static class AuthEndpoints
{
    private const int LockedStatusCode = 423;

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/auth")
            .WithTags("Authentication");

        group
            .MapGet(
                "/csrf",
                (
                    IAntiforgery antiforgery,
                    HttpContext httpContext,
                    IWebHostEnvironment environment) =>
                    IssueCsrfToken(antiforgery, httpContext, environment))
            .AllowAnonymous()
            .WithName("AuthCsrf");

        group
            .MapPost(
                "/login",
                async (
                    LoginRequest request,
                    IAuthSessionService authSessionService,
                    HttpContext httpContext,
                    CancellationToken cancellationToken) =>
                    await LoginAsync(request, authSessionService, httpContext, cancellationToken))
            .AllowAnonymous()
            .WithName("AuthLogin");

        group
            .MapPost("/logout", async (HttpContext httpContext) => await LogoutAsync(httpContext))
            .RequireAuthorization()
            .WithName("AuthLogout");

        group
            .MapGet(
                "/me",
                async (
                    IAuthSessionService authSessionService,
                    HttpContext httpContext,
                    CancellationToken cancellationToken) =>
                    await MeAsync(authSessionService, httpContext, cancellationToken))
            .RequireAuthorization()
            .WithName("AuthMe");

        return endpoints;
    }

    private static IResult IssueCsrfToken(
        IAntiforgery antiforgery,
        HttpContext httpContext,
        IWebHostEnvironment environment)
    {
        var tokens = antiforgery.GetAndStoreTokens(httpContext);

        httpContext.Response.Cookies.Append(
            "XSRF-TOKEN",
            tokens.RequestToken ?? string.Empty,
            new CookieOptions
            {
                HttpOnly = false,
                SameSite = SameSiteMode.Lax,
                Secure = environment.IsProduction() || httpContext.Request.IsHttps,
                Path = "/"
            });

        return Results.NoContent();
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        IAuthSessionService authSessionService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(LoginRequest.Email)] = ["Email is required."],
                [nameof(LoginRequest.Password)] = ["Password is required."]
            });
        }

        var result = await authSessionService.LoginAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (!result.Succeeded || result.User is null)
        {
            return result.FailureReason is LoginFailureReason.Inactive or LoginFailureReason.LockedOut
                ? Results.Problem(
                    title: "Usuario inactivo o bloqueado.",
                    statusCode: LockedStatusCode)
                : Results.Unauthorized();
        }

        var principal = CreatePrincipal(result.User);
        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                AllowRefresh = true,
                IsPersistent = false,
                IssuedUtc = DateTimeOffset.UtcNow
            });

        return Results.Ok(AuthUserResponse.FromAuthenticatedUser(result.User));
    }

    private static async Task<IResult> LogoutAsync(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return Results.NoContent();
    }

    private static async Task<IResult> MeAsync(
        IAuthSessionService authSessionService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await authSessionService.GetCurrentUserAsync(
            httpContext.User,
            cancellationToken);

        return currentUser is null
            ? Results.Unauthorized()
            : Results.Ok(AuthUserResponse.FromAuthenticatedUser(currentUser));
    }

    private static ClaimsPrincipal CreatePrincipal(AuthenticatedUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName)
        };

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(user.Permissions.Select(permission => new Claim(PermissionClaimTypes.Permission, permission)));

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme,
            ClaimTypes.Name,
            ClaimTypes.Role);

        return new ClaimsPrincipal(identity);
    }
}

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthUserResponse(
    Guid Id,
    string Email,
    string FullName,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions)
{
    public static AuthUserResponse FromAuthenticatedUser(AuthenticatedUser user)
    {
        return new AuthUserResponse(
            user.Id,
            user.Email,
            user.FullName,
            user.Roles,
            user.Permissions);
    }
}
