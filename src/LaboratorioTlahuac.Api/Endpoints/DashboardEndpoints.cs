using LaboratorioTlahuac.Application.Dashboard;
using LaboratorioTlahuac.Domain.Security;

namespace LaboratorioTlahuac.Api.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/dashboard")
            .WithTags("Dashboard");

        group.MapGet(
                "/summary",
                async (
                    IDashboardService dashboardService,
                    CancellationToken cancellationToken) =>
                    Results.Ok(await dashboardService.GetSummaryAsync(cancellationToken)))
            .RequireAuthorization(Permissions.ReportsView)
            .WithName("DashboardSummary");

        return endpoints;
    }
}
