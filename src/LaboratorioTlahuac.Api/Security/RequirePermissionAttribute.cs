using Microsoft.AspNetCore.Authorization;

namespace LaboratorioTlahuac.Api.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute(string permission) : AuthorizeAttribute(permission)
{
    public string Permission { get; } = permission;
}
