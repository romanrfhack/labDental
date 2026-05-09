using Microsoft.AspNetCore.Authorization;

namespace LaboratorioTlahuac.Api.Security;

public sealed record PermissionAuthorizationRequirement(string Permission) : IAuthorizationRequirement;
