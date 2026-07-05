using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LaboratorioTlahuac.Application.Abstractions.Authentication;
using LaboratorioTlahuac.Application.Abstractions.Security;
using LaboratorioTlahuac.Application.Abstractions.Time;
using LaboratorioTlahuac.Application.Admin;
using LaboratorioTlahuac.Application.Catalog;
using LaboratorioTlahuac.Application.Customers;
using LaboratorioTlahuac.Application.Dashboard;
using LaboratorioTlahuac.Application.Deliveries;
using LaboratorioTlahuac.Domain.Security.Entities;
using LaboratorioTlahuac.Infrastructure.Admin;
using LaboratorioTlahuac.Infrastructure.Catalog;
using LaboratorioTlahuac.Infrastructure.Customers;
using LaboratorioTlahuac.Infrastructure.Dashboard;
using LaboratorioTlahuac.Infrastructure.Deliveries;
using LaboratorioTlahuac.Infrastructure.Persistence;
using LaboratorioTlahuac.Infrastructure.Security;
using LaboratorioTlahuac.Infrastructure.Security.Authentication;
using LaboratorioTlahuac.Infrastructure.Security.Seed;
using LaboratorioTlahuac.Infrastructure.Time;
using LaboratorioTlahuac.Application.Payments;
using LaboratorioTlahuac.Application.WorkOrders;
using LaboratorioTlahuac.Infrastructure.Payments;
using LaboratorioTlahuac.Infrastructure.WorkOrders;

namespace LaboratorioTlahuac.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment)
    {
        var databaseOptions = configuration
            .GetSection(DatabaseOptions.SectionName)
            .Get<DatabaseOptions>() ?? new DatabaseOptions();
        var dashboardOptions = configuration
            .GetSection(DashboardOptions.SectionName)
            .Get<DashboardOptions>() ?? new DashboardOptions();

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
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton(dashboardOptions);
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IAuthSessionService, AuthSessionService>();
        services.AddScoped<IAdminSecurityService, AdminSecurityService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IDeliveryService, DeliveryService>();
        services.AddScoped<IWorkOrderService, WorkOrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddSingleton<IWorkOrderNumberGenerator, GuidWorkOrderNumberGenerator>();
        services.AddSingleton(new SecuritySeedRuntimeOptions { IsDevelopment = isDevelopment });
        services.AddScoped<ISecuritySeeder, SecuritySeeder>();
        services.AddScoped<ICatalogSeeder, CatalogSeeder>();

        return services;
    }
}
