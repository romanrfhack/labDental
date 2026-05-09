using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LaboratorioTlahuac.Application.Abstractions.Authentication;
using LaboratorioTlahuac.Application.Abstractions.Security;
using LaboratorioTlahuac.Domain.Security.Entities;
using LaboratorioTlahuac.Infrastructure.Persistence;
using LaboratorioTlahuac.Infrastructure.Security;
using LaboratorioTlahuac.Infrastructure.Security.Authentication;
using LaboratorioTlahuac.Infrastructure.Security.Seed;

namespace LaboratorioTlahuac.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var databaseOptions = configuration
            .GetSection(DatabaseOptions.SectionName)
            .Get<DatabaseOptions>() ?? new DatabaseOptions();

        var connectionString = configuration.GetConnectionString(databaseOptions.ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{databaseOptions.ConnectionStringName}' is required.");
        }

        services.AddDbContext<LaboratorioTlahuacDbContext>(options =>
        {
            if (!string.Equals(databaseOptions.Provider, "SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Unsupported database provider '{databaseOptions.Provider}'.");
            }

            options.UseSqlServer(connectionString);
        });

        services.AddSingleton<IPermissionChecker, ClaimsPermissionChecker>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IAuthSessionService, AuthSessionService>();
        services.AddScoped<ISecuritySeeder, SecuritySeeder>();

        return services;
    }
}
