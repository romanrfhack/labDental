using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LaboratorioTlahuac.Api.Endpoints;
using LaboratorioTlahuac.Api.Security;
using LaboratorioTlahuac.Application;
using LaboratorioTlahuac.Domain.Security;
using LaboratorioTlahuac.Infrastructure;
using LaboratorioTlahuac.Infrastructure.Persistence;
using LaboratorioTlahuac.Infrastructure.Security.Seed;

var builder = WebApplication.CreateBuilder(args);

const string DevelopmentCorsPolicy = "DevelopmentAngular";

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = builder.Configuration["Authentication:Antiforgery:CookieName"] ?? "Ldt.AntiForgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsProduction()
        ? CookieSecurePolicy.Always
        : CookieSecurePolicy.SameAsRequest;
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = builder.Configuration["Authentication:Cookie:Name"] ?? "__Host-Ldt.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = builder.Environment.IsProduction()
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Path = "/";
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                if (IsApiRequest(context.Request))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = context =>
            {
                if (IsApiRequest(context.Request))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            },
            OnValidatePrincipal = async context =>
            {
                var userIdClaim = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return;
                }

                var dbContext = context.HttpContext.RequestServices.GetRequiredService<LaboratorioTlahuacDbContext>();
                var now = DateTimeOffset.UtcNow;
                var userState = await dbContext.Users
                    .AsNoTracking()
                    .Where(user => user.Id == userId)
                    .Select(user => new { user.IsActive, user.LockoutEndUtc })
                    .FirstOrDefaultAsync(context.HttpContext.RequestAborted);

                if (userState is null
                    || !userState.IsActive
                    || (userState.LockoutEndUtc is not null && userState.LockoutEndUtc > now))
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
            }
        };
    });

builder.Services.AddAuthorization(options => options.AddPermissionPolicies());
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? [];

    options.AddPolicy(DevelopmentCorsPolicy, policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Configuration.GetValue<bool>("SecuritySeed:RunOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var seeder = scope.ServiceProvider.GetRequiredService<ISecuritySeeder>();
    await seeder.SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseCors(DevelopmentCorsPolicy);
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    if (!RequiresAntiforgeryValidation(context.Request))
    {
        await next(context);
        return;
    }

    var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();

    try
    {
        await antiforgery.ValidateRequestAsync(context);
    }
    catch (AntiforgeryValidationException)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new
        {
            title = "Invalid or missing XSRF token.",
            status = StatusCodes.Status400BadRequest
        });
        return;
    }

    await next(context);
});

app.MapHealthEndpoints();
app.MapAuthEndpoints();
app.MapSecurityDiagnosticEndpoints(app.Environment);

app.Run();

static bool IsApiRequest(HttpRequest request)
{
    return request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
}

static bool RequiresAntiforgeryValidation(HttpRequest request)
{
    if (!request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
    {
        return false;
    }

    return !HttpMethods.IsGet(request.Method)
        && !HttpMethods.IsHead(request.Method)
        && !HttpMethods.IsOptions(request.Method)
        && !HttpMethods.IsTrace(request.Method);
}

public partial class Program;
