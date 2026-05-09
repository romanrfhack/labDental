using Microsoft.AspNetCore.Authorization;
using LaboratorioTlahuac.Domain.Security;

namespace LaboratorioTlahuac.Api.Security;

public static class AuthorizationOptionsExtensions
{
    public static void AddPermissionPolicies(this AuthorizationOptions options)
    {
        foreach (var permission in Permissions.All)
        {
            options.AddPolicy(
                permission,
                policy => policy.Requirements.Add(new PermissionAuthorizationRequirement(permission)));
        }
    }
}
