using Microsoft.AspNetCore.Authorization;
using LaboratorioTlahuac.Application.Abstractions.Security;

namespace LaboratorioTlahuac.Api.Security;

public sealed class PermissionAuthorizationHandler(IPermissionChecker permissionChecker)
    : AuthorizationHandler<PermissionAuthorizationRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionAuthorizationRequirement requirement)
    {
        if (await permissionChecker.HasPermissionAsync(context.User, requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
