using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LaboratorioTlahuac.Application.Abstractions.Security;
using LaboratorioTlahuac.Application.Abstractions.Time;
using LaboratorioTlahuac.Application.Admin;
using LaboratorioTlahuac.Domain.Security;
using LaboratorioTlahuac.Domain.Security.Entities;
using LaboratorioTlahuac.Infrastructure.Persistence;

namespace LaboratorioTlahuac.Infrastructure.Admin;

public sealed class AdminSecurityService(
    LaboratorioTlahuacDbContext dbContext,
    IPasswordHasher<User> passwordHasher,
    IClock clock,
    ICurrentUser currentUser)
    : IAdminSecurityService
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;
    private const int MinimumTemporaryPasswordLength = 10;

    public async Task<AdminSecurityServiceResult<AdminPagedResponse<AdminUserListItemResponse>>> ListUsersAsync(
        AdminUserListQuery query,
        CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var page = query.Page ?? DefaultPage;
        var pageSize = query.PageSize ?? DefaultPageSize;

        if (page < 1)
        {
            AddError(errors, nameof(query.Page), "Page must be greater than or equal to 1.");
        }

        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            AddError(errors, nameof(query.PageSize), "PageSize must be between 1 and 100.");
        }

        if (errors.Count > 0)
        {
            return AdminSecurityServiceResult.Validation<AdminPagedResponse<AdminUserListItemResponse>>(errors);
        }

        var usersQuery = dbContext.Users
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .AsNoTracking();

        if (query.IsActive.HasValue)
        {
            usersQuery = usersQuery.Where(user => user.IsActive == query.IsActive.Value);
        }

        if (query.RoleId.HasValue)
        {
            usersQuery = usersQuery.Where(user =>
                user.UserRoles.Any(userRole => userRole.RoleId == query.RoleId.Value));
        }

        var search = NormalizeOptional(query.Search);

        if (search is not null)
        {
            var pattern = $"%{search}%";
            usersQuery = usersQuery.Where(user =>
                EF.Functions.Like(user.Email, pattern)
                || EF.Functions.Like(user.FullName, pattern)
                || user.UserRoles.Any(userRole =>
                    userRole.Role != null && EF.Functions.Like(userRole.Role.Name, pattern)));
        }

        var totalCount = await usersQuery.CountAsync(cancellationToken);
        var users = await usersQuery
            .OrderByDescending(user => user.IsActive)
            .ThenBy(user => user.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return AdminSecurityServiceResult.Success(
            new AdminPagedResponse<AdminUserListItemResponse>(
                users.Select(MapUserListItem).ToArray(),
                page,
                pageSize,
                totalCount));
    }

    public async Task<AdminSecurityServiceResult<AdminUserDetailResponse>> GetUserByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await FindUserWithRolesAsync(id, asNoTracking: true, cancellationToken);

        return user is null
            ? AdminSecurityServiceResult.NotFound<AdminUserDetailResponse>("User was not found.")
            : AdminSecurityServiceResult.Success(MapUserDetail(user));
    }

    public async Task<AdminSecurityServiceResult<AdminUserDetailResponse>> CreateUserAsync(
        AdminUserCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var input = ValidateUserCreate(request);

        if (input.Errors.Count > 0 || input.Value is null)
        {
            return AdminSecurityServiceResult.Validation<AdminUserDetailResponse>(input.Errors);
        }

        var roleIds = NormalizeRoleIds(request.RoleIds);
        var roleCheck = await EnsureRolesExistAsync(roleIds, requireAtLeastOne: true, cancellationToken);

        if (roleCheck.Errors.Count > 0)
        {
            return AdminSecurityServiceResult.Validation<AdminUserDetailResponse>(roleCheck.Errors);
        }

        var normalizedEmail = SecurityTextNormalizer.NormalizeEmail(input.Value.Email);
        var emailExists = await dbContext.Users
            .AnyAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            return AdminSecurityServiceResult.Conflict<AdminUserDetailResponse>(
                "A user with this email already exists.");
        }

        var now = clock.UtcNow;
        var user = User.Create(input.Value.Email, input.Value.FullName, "pending-password-hash", now);
        user.SetPasswordHash(passwordHasher.HashPassword(user, input.Value.TemporaryPassword));

        dbContext.Users.Add(user);

        foreach (var roleId in roleIds)
        {
            dbContext.UserRoles.Add(new UserRole(user.Id, roleId));
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetUserByIdAsync(user.Id, cancellationToken);
    }

    public async Task<AdminSecurityServiceResult<AdminUserDetailResponse>> UpdateUserAsync(
        Guid id,
        AdminUserUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var input = ValidateUserUpdate(request);

        if (input.Errors.Count > 0 || input.Value is null)
        {
            return AdminSecurityServiceResult.Validation<AdminUserDetailResponse>(input.Errors);
        }

        var user = await FindUserWithRolesAsync(id, asNoTracking: false, cancellationToken);

        if (user is null)
        {
            return AdminSecurityServiceResult.NotFound<AdminUserDetailResponse>("User was not found.");
        }

        var normalizedEmail = SecurityTextNormalizer.NormalizeEmail(input.Value.Email);
        var emailInUse = await dbContext.Users
            .AnyAsync(
                currentUser => currentUser.Id != id && currentUser.NormalizedEmail == normalizedEmail,
                cancellationToken);

        if (emailInUse)
        {
            return AdminSecurityServiceResult.Conflict<AdminUserDetailResponse>(
                "A user with this email already exists.");
        }

        user.UpdateProfile(input.Value.Email, input.Value.FullName, clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        return AdminSecurityServiceResult.Success(MapUserDetail(user));
    }

    public async Task<AdminSecurityServiceResult<AdminUserDetailResponse>> UpdateUserStatusAsync(
        Guid id,
        AdminUserStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await FindUserWithRolesAsync(id, asNoTracking: false, cancellationToken);

        if (user is null)
        {
            return AdminSecurityServiceResult.NotFound<AdminUserDetailResponse>("User was not found.");
        }

        if (!request.IsActive)
        {
            if (currentUser.UserId == id)
            {
                return AdminSecurityServiceResult.Conflict<AdminUserDetailResponse>(
                    "Users cannot deactivate their own account.");
            }

            if (UserGrantsPermission(user, Permissions.UsersManage)
                && !await AnyOtherActiveUserWithPermissionAsync(id, Permissions.UsersManage, cancellationToken))
            {
                return AdminSecurityServiceResult.Conflict<AdminUserDetailResponse>(
                    "At least one active user with users.manage must remain.");
            }
        }

        if (request.IsActive)
        {
            user.Activate(clock.UtcNow);
        }
        else
        {
            user.Deactivate(clock.UtcNow);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return AdminSecurityServiceResult.Success(MapUserDetail(user));
    }

    public async Task<AdminSecurityServiceResult<AdminUserDetailResponse>> AssignUserRolesAsync(
        Guid id,
        AdminUserRolesRequest request,
        CancellationToken cancellationToken = default)
    {
        var roleIds = NormalizeRoleIds(request.RoleIds);
        var roleCheck = await EnsureRolesExistAsync(roleIds, requireAtLeastOne: true, cancellationToken);

        if (roleCheck.Errors.Count > 0)
        {
            return AdminSecurityServiceResult.Validation<AdminUserDetailResponse>(roleCheck.Errors);
        }

        var user = await FindUserWithRolesAsync(id, asNoTracking: false, cancellationToken);

        if (user is null)
        {
            return AdminSecurityServiceResult.NotFound<AdminUserDetailResponse>("User was not found.");
        }

        var desiredRolesGrantUsersManage = await RolesGrantPermissionAsync(
            roleIds,
            Permissions.UsersManage,
            cancellationToken);

        if (user.IsActive
            && !desiredRolesGrantUsersManage
            && !await AnyOtherActiveUserWithPermissionAsync(id, Permissions.UsersManage, cancellationToken))
        {
            return AdminSecurityServiceResult.Conflict<AdminUserDetailResponse>(
                "At least one active user with users.manage must remain.");
        }

        await SynchronizeUserRolesAsync(user.Id, roleIds, cancellationToken);
        user.Rename(user.FullName, clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        var updatedUser = await FindUserWithRolesAsync(id, asNoTracking: true, cancellationToken);

        return updatedUser is null
            ? AdminSecurityServiceResult.NotFound<AdminUserDetailResponse>("User was not found.")
            : AdminSecurityServiceResult.Success(MapUserDetail(updatedUser));
    }

    public async Task<AdminSecurityServiceResult<AdminUserDetailResponse>> SetTemporaryPasswordAsync(
        Guid id,
        AdminUserTemporaryPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var temporaryPassword = NormalizeOptional(request.TemporaryPassword);

        ValidateTemporaryPassword(temporaryPassword, errors);

        if (errors.Count > 0 || temporaryPassword is null)
        {
            return AdminSecurityServiceResult.Validation<AdminUserDetailResponse>(errors);
        }

        var user = await FindUserWithRolesAsync(id, asNoTracking: false, cancellationToken);

        if (user is null)
        {
            return AdminSecurityServiceResult.NotFound<AdminUserDetailResponse>("User was not found.");
        }

        var now = clock.UtcNow;
        user.SetPasswordHash(passwordHasher.HashPassword(user, temporaryPassword));
        user.ClearLockout(now);
        await dbContext.SaveChangesAsync(cancellationToken);

        return AdminSecurityServiceResult.Success(MapUserDetail(user));
    }

    public async Task<AdminSecurityServiceResult<IReadOnlyCollection<AdminRoleListItemResponse>>> ListRolesAsync(
        CancellationToken cancellationToken = default)
    {
        var roles = await dbContext.Roles
            .Include(role => role.RolePermissions)
                .ThenInclude(rolePermission => rolePermission.Permission)
            .Include(role => role.UserRoles)
            .AsNoTracking()
            .OrderBy(role => role.Name)
            .ToListAsync(cancellationToken);

        return AdminSecurityServiceResult.Success<IReadOnlyCollection<AdminRoleListItemResponse>>(
            roles.Select(MapRoleListItem).ToArray());
    }

    public async Task<AdminSecurityServiceResult<AdminRoleDetailResponse>> GetRoleByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var role = await dbContext.Roles
            .Include(currentRole => currentRole.RolePermissions)
                .ThenInclude(rolePermission => rolePermission.Permission)
            .Include(currentRole => currentRole.UserRoles)
                .ThenInclude(userRole => userRole.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(currentRole => currentRole.Id == id, cancellationToken);

        return role is null
            ? AdminSecurityServiceResult.NotFound<AdminRoleDetailResponse>("Role was not found.")
            : AdminSecurityServiceResult.Success(MapRoleDetail(role));
    }

    private async Task<User?> FindUserWithRolesAsync(
        Guid id,
        bool asNoTracking,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Users
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                    .ThenInclude(role => role!.RolePermissions)
                        .ThenInclude(rolePermission => rolePermission.Permission)
            .AsQueryable();

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    private async Task<RoleValidationResult> EnsureRolesExistAsync(
        HashSet<Guid> roleIds,
        bool requireAtLeastOne,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

        if (requireAtLeastOne && roleIds.Count == 0)
        {
            AddError(errors, "RoleIds", "At least one role is required.");
            return new RoleValidationResult(errors);
        }

        if (roleIds.Count == 0)
        {
            return new RoleValidationResult(errors);
        }

        var existingRoleIds = await dbContext.Roles
            .Where(role => roleIds.Contains(role.Id))
            .Select(role => role.Id)
            .ToListAsync(cancellationToken);

        if (existingRoleIds.Count != roleIds.Count)
        {
            AddError(errors, "RoleIds", "One or more roles do not exist.");
        }

        return new RoleValidationResult(errors);
    }

    private async Task SynchronizeUserRolesAsync(
        Guid userId,
        HashSet<Guid> desiredRoleIds,
        CancellationToken cancellationToken)
    {
        var existingUserRoles = await dbContext.UserRoles
            .Where(userRole => userRole.UserId == userId)
            .ToListAsync(cancellationToken);
        var existingRoleIds = existingUserRoles
            .Select(userRole => userRole.RoleId)
            .ToHashSet();

        dbContext.UserRoles.RemoveRange(
            existingUserRoles.Where(userRole => !desiredRoleIds.Contains(userRole.RoleId)));

        foreach (var desiredRoleId in desiredRoleIds)
        {
            if (existingRoleIds.Contains(desiredRoleId))
            {
                continue;
            }

            dbContext.UserRoles.Add(new UserRole(userId, desiredRoleId));
        }
    }

    private async Task<bool> AnyOtherActiveUserWithPermissionAsync(
        Guid excludedUserId,
        string permission,
        CancellationToken cancellationToken)
    {
        return await dbContext.UserRoles
            .Where(userRole => userRole.UserId != excludedUserId && userRole.User!.IsActive)
            .Join(
                dbContext.RolePermissions,
                userRole => userRole.RoleId,
                rolePermission => rolePermission.RoleId,
                (_, rolePermission) => rolePermission.PermissionId)
            .Join(
                dbContext.Permissions,
                permissionId => permissionId,
                currentPermission => currentPermission.Id,
                (_, currentPermission) => currentPermission.Key)
            .AnyAsync(permissionKey => permissionKey == permission, cancellationToken);
    }

    private async Task<bool> RolesGrantPermissionAsync(
        HashSet<Guid> roleIds,
        string permission,
        CancellationToken cancellationToken)
    {
        if (roleIds.Count == 0)
        {
            return false;
        }

        return await dbContext.RolePermissions
            .Where(rolePermission => roleIds.Contains(rolePermission.RoleId))
            .Join(
                dbContext.Permissions,
                rolePermission => rolePermission.PermissionId,
                currentPermission => currentPermission.Id,
                (_, currentPermission) => currentPermission.Key)
            .AnyAsync(permissionKey => permissionKey == permission, cancellationToken);
    }

    private static ValidatedInput<ValidatedCreateUser> ValidateUserCreate(AdminUserCreateRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var email = NormalizeOptional(request.Email);
        var fullName = NormalizeOptional(request.FullName);
        var temporaryPassword = NormalizeOptional(request.TemporaryPassword);

        ValidateEmail(email, errors);
        ValidateFullName(fullName, errors);
        ValidateTemporaryPassword(temporaryPassword, errors);

        return errors.Count > 0 || email is null || fullName is null || temporaryPassword is null
            ? new ValidatedInput<ValidatedCreateUser>(null, errors)
            : new ValidatedInput<ValidatedCreateUser>(
                new ValidatedCreateUser(email, fullName, temporaryPassword),
                errors);
    }

    private static ValidatedInput<ValidatedUpdateUser> ValidateUserUpdate(AdminUserUpdateRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var email = NormalizeOptional(request.Email);
        var fullName = NormalizeOptional(request.FullName);

        ValidateEmail(email, errors);
        ValidateFullName(fullName, errors);

        return errors.Count > 0 || email is null || fullName is null
            ? new ValidatedInput<ValidatedUpdateUser>(null, errors)
            : new ValidatedInput<ValidatedUpdateUser>(
                new ValidatedUpdateUser(email, fullName),
                errors);
    }

    private static void ValidateEmail(string? email, Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            AddError(errors, "Email", "Email is required.");
            return;
        }

        try
        {
            var address = new MailAddress(email);

            if (!string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase))
            {
                AddError(errors, "Email", "Email must be valid.");
            }
        }
        catch (FormatException)
        {
            AddError(errors, "Email", "Email must be valid.");
        }
    }

    private static void ValidateFullName(string? fullName, Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            AddError(errors, "FullName", "Full name is required.");
        }
    }

    private static void ValidateTemporaryPassword(string? temporaryPassword, Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(temporaryPassword))
        {
            AddError(errors, "TemporaryPassword", "Temporary password is required.");
            return;
        }

        if (temporaryPassword.Length < MinimumTemporaryPasswordLength)
        {
            AddError(
                errors,
                "TemporaryPassword",
                $"Temporary password must have at least {MinimumTemporaryPasswordLength} characters.");
        }
    }

    private static AdminUserListItemResponse MapUserListItem(User user)
    {
        return new AdminUserListItemResponse(
            user.Id,
            user.Email,
            user.FullName,
            user.IsActive,
            MapUserRoles(user),
            user.LastLoginAtUtc,
            user.CreatedAtUtc,
            user.UpdatedAtUtc);
    }

    private static AdminUserDetailResponse MapUserDetail(User user)
    {
        return new AdminUserDetailResponse(
            user.Id,
            user.Email,
            user.FullName,
            user.IsActive,
            MapUserRoles(user),
            user.LastLoginAtUtc,
            user.CreatedAtUtc,
            user.UpdatedAtUtc);
    }

    private static AdminRoleListItemResponse MapRoleListItem(Role role)
    {
        var permissions = MapRolePermissions(role);

        return new AdminRoleListItemResponse(
            role.Id,
            role.Name,
            role.Description,
            role.IsSystem,
            role.UserRoles.Count,
            permissions.Length,
            permissions);
    }

    private static AdminRoleDetailResponse MapRoleDetail(Role role)
    {
        return new AdminRoleDetailResponse(
            role.Id,
            role.Name,
            role.Description,
            role.IsSystem,
            role.UserRoles.Count,
            role.UserRoles.Count(userRole => userRole.User?.IsActive == true),
            MapRolePermissions(role));
    }

    private static AdminRoleSummaryResponse[] MapUserRoles(User user)
    {
        return user.UserRoles
            .Select(userRole => userRole.Role)
            .Where(role => role is not null)
            .Select(role => new AdminRoleSummaryResponse(
                role!.Id,
                role.Name,
                role.Description,
                role.IsSystem))
            .OrderBy(role => role.Name, StringComparer.Ordinal)
            .ToArray();
    }

    private static AdminPermissionResponse[] MapRolePermissions(Role role)
    {
        return role.RolePermissions
            .Select(rolePermission => rolePermission.Permission)
            .Where(permission => permission is not null)
            .Select(permission => new AdminPermissionResponse(
                permission!.Id,
                permission.Key,
                permission.Description))
            .OrderBy(permission => permission.Key, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool UserGrantsPermission(User user, string permission)
    {
        return user.UserRoles
            .Select(userRole => userRole.Role)
            .Where(role => role is not null)
            .SelectMany(role => role!.RolePermissions)
            .Select(rolePermission => rolePermission.Permission)
            .Any(currentPermission => currentPermission?.Key == permission);
    }

    private static HashSet<Guid> NormalizeRoleIds(IReadOnlyCollection<Guid>? roleIds)
    {
        return (roleIds ?? [])
            .Where(roleId => roleId != Guid.Empty)
            .ToHashSet();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static void AddError(
        Dictionary<string, string[]> errors,
        string key,
        string message)
    {
        errors[key] = [message];
    }

    private sealed record RoleValidationResult(IReadOnlyDictionary<string, string[]> Errors);

    private sealed record ValidatedInput<T>(T? Value, IReadOnlyDictionary<string, string[]> Errors);

    private sealed record ValidatedCreateUser(string Email, string FullName, string TemporaryPassword);

    private sealed record ValidatedUpdateUser(string Email, string FullName);
}
