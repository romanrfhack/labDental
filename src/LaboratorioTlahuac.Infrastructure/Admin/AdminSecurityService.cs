using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LaboratorioTlahuac.Application.Abstractions.Security;
using LaboratorioTlahuac.Application.Abstractions.Time;
using LaboratorioTlahuac.Application.Admin;
using LaboratorioTlahuac.Domain.Security;
using LaboratorioTlahuac.Domain.Security.Entities;
using LaboratorioTlahuac.Infrastructure.Persistence;
using LaboratorioTlahuac.Infrastructure.Security.Authentication;

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
    private static readonly string NormalizedAdminRoleName = SecurityTextNormalizer.NormalizeName("Admin");

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
        var user = await FindUserWithSecurityAsync(id, asNoTracking: true, cancellationToken);

        if (user is null)
        {
            return AdminSecurityServiceResult.NotFound<AdminUserDetailResponse>("User was not found.");
        }

        var permissions = await ListPermissionsAsync(cancellationToken);

        return AdminSecurityServiceResult.Success(MapUserDetail(user, permissions));
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

        var roleIds = NormalizeIds(request.RoleIds);
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

        var user = await FindUserWithSecurityAsync(id, asNoTracking: false, cancellationToken);

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

        return await GetUserByIdAsync(id, cancellationToken);
    }

    public async Task<AdminSecurityServiceResult<AdminUserDetailResponse>> UpdateUserStatusAsync(
        Guid id,
        AdminUserStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await FindUserWithSecurityAsync(id, asNoTracking: false, cancellationToken);

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

        return await GetUserByIdAsync(id, cancellationToken);
    }

    public async Task<AdminSecurityServiceResult<AdminUserDetailResponse>> AssignUserRolesAsync(
        Guid id,
        AdminUserRolesRequest request,
        CancellationToken cancellationToken = default)
    {
        var roleIds = NormalizeIds(request.RoleIds);
        var roleCheck = await EnsureRolesExistAsync(roleIds, requireAtLeastOne: true, cancellationToken);

        if (roleCheck.Errors.Count > 0)
        {
            return AdminSecurityServiceResult.Validation<AdminUserDetailResponse>(roleCheck.Errors);
        }

        var user = await FindUserWithSecurityAsync(id, asNoTracking: false, cancellationToken);

        if (user is null)
        {
            return AdminSecurityServiceResult.NotFound<AdminUserDetailResponse>("User was not found.");
        }

        var desiredRolesContainAdmin = await RolesContainAdminAsync(roleIds, cancellationToken);
        var desiredRolesGrantUsersManage = desiredRolesContainAdmin
            || await RolesGrantPermissionAsync(roleIds, Permissions.UsersManage, cancellationToken);
        var usersManageOverride = GetOverrideEffect(user, Permissions.UsersManage);
        var desiredGrantsUsersManage = desiredRolesContainAdmin
            || usersManageOverride == UserPermissionOverrideEffect.Allow
            || (usersManageOverride != UserPermissionOverrideEffect.Deny && desiredRolesGrantUsersManage);

        if (user.IsActive
            && !desiredGrantsUsersManage
            && !await AnyOtherActiveUserWithPermissionAsync(id, Permissions.UsersManage, cancellationToken))
        {
            return AdminSecurityServiceResult.Conflict<AdminUserDetailResponse>(
                "At least one active user with users.manage must remain.");
        }

        await SynchronizeUserRolesAsync(user.Id, roleIds, cancellationToken);
        user.Rename(user.FullName, clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetUserByIdAsync(id, cancellationToken);
    }

    public async Task<AdminSecurityServiceResult<AdminUserDetailResponse>> UpdateUserPermissionsAsync(
        Guid id,
        AdminUserPermissionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidatePermissionOverrides(request.Overrides);

        if (validation.Errors.Count > 0 || validation.Value is null)
        {
            return AdminSecurityServiceResult.Validation<AdminUserDetailResponse>(validation.Errors);
        }

        var permissionIds = validation.Value.Keys.ToHashSet();
        var permissionCheck = await EnsurePermissionsExistAsync(permissionIds, cancellationToken);

        if (permissionCheck.Errors.Count > 0)
        {
            return AdminSecurityServiceResult.Validation<AdminUserDetailResponse>(permissionCheck.Errors);
        }

        var user = await FindUserWithSecurityAsync(id, asNoTracking: false, cancellationToken);

        if (user is null)
        {
            return AdminSecurityServiceResult.NotFound<AdminUserDetailResponse>("User was not found.");
        }

        if (SecurityIdentityMapper.IsAdmin(user) && validation.Value.Count > 0)
        {
            return AdminSecurityServiceResult.Conflict<AdminUserDetailResponse>(
                "Users with the Admin role inherit the protected Admin permission set and cannot have individual overrides.");
        }

        var usersManagePermissionId = await dbContext.Permissions
            .Where(permission => permission.Key == Permissions.UsersManage)
            .Select(permission => (Guid?)permission.Id)
            .SingleOrDefaultAsync(cancellationToken);
        var desiredGrantsUsersManage = SecurityIdentityMapper.IsAdmin(user)
            || GrantsPermissionWithOverrides(user, Permissions.UsersManage, usersManagePermissionId, validation.Value);

        if (user.IsActive
            && !desiredGrantsUsersManage
            && !await AnyOtherActiveUserWithPermissionAsync(id, Permissions.UsersManage, cancellationToken))
        {
            return AdminSecurityServiceResult.Conflict<AdminUserDetailResponse>(
                "At least one active user with users.manage must remain.");
        }

        await SynchronizeUserPermissionOverridesAsync(user.Id, validation.Value, cancellationToken);
        user.Rename(user.FullName, clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetUserByIdAsync(id, cancellationToken);
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

        var user = await FindUserWithSecurityAsync(id, asNoTracking: false, cancellationToken);

        if (user is null)
        {
            return AdminSecurityServiceResult.NotFound<AdminUserDetailResponse>("User was not found.");
        }

        var now = clock.UtcNow;
        user.SetPasswordHash(passwordHasher.HashPassword(user, temporaryPassword));
        user.ClearLockout(now);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetUserByIdAsync(id, cancellationToken);
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
        var role = await FindRoleWithSecurityAsync(id, asNoTracking: true, cancellationToken);

        if (role is null)
        {
            return AdminSecurityServiceResult.NotFound<AdminRoleDetailResponse>("Role was not found.");
        }

        var permissions = await ListPermissionsAsync(cancellationToken);

        return AdminSecurityServiceResult.Success(MapRoleDetail(role, permissions));
    }

    public async Task<AdminSecurityServiceResult<AdminRoleDetailResponse>> UpdateRolePermissionsAsync(
        Guid id,
        AdminRolePermissionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var permissionIds = NormalizeIds(request.PermissionIds);
        var permissionCheck = await EnsurePermissionsExistAsync(permissionIds, cancellationToken);

        if (permissionCheck.Errors.Count > 0)
        {
            return AdminSecurityServiceResult.Validation<AdminRoleDetailResponse>(permissionCheck.Errors);
        }

        var role = await FindRoleWithSecurityAsync(id, asNoTracking: false, cancellationToken);

        if (role is null)
        {
            return AdminSecurityServiceResult.NotFound<AdminRoleDetailResponse>("Role was not found.");
        }

        if (IsRolePermissionEditingLocked(role))
        {
            return AdminSecurityServiceResult.Conflict<AdminRoleDetailResponse>(
                "Admin permissions are protected and cannot be reduced from the UI.");
        }

        var usersManagePermission = await dbContext.Permissions
            .AsNoTracking()
            .SingleAsync(permission => permission.Key == Permissions.UsersManage, cancellationToken);
        var currentlyGrantsUsersManage = role.RolePermissions.Any(
            rolePermission => rolePermission.PermissionId == usersManagePermission.Id);
        var willGrantUsersManage = permissionIds.Contains(usersManagePermission.Id);

        if (currentlyGrantsUsersManage
            && !willGrantUsersManage
            && !await AnyActiveUserWithPermissionAfterRoleChangeAsync(
                role.Id,
                permissionIds,
                usersManagePermission,
                cancellationToken))
        {
            return AdminSecurityServiceResult.Conflict<AdminRoleDetailResponse>(
                "At least one active user with users.manage must remain.");
        }

        await SynchronizeRolePermissionsAsync(role.Id, permissionIds, cancellationToken);
        role.Touch(clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetRoleByIdAsync(id, cancellationToken);
    }

    private async Task<User?> FindUserWithSecurityAsync(
        Guid id,
        bool asNoTracking,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Users
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                    .ThenInclude(role => role!.RolePermissions)
                        .ThenInclude(rolePermission => rolePermission.Permission)
            .Include(user => user.PermissionOverrides)
                .ThenInclude(permissionOverride => permissionOverride.Permission)
            .AsQueryable();

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    private async Task<Role?> FindRoleWithSecurityAsync(
        Guid id,
        bool asNoTracking,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Roles
            .Include(role => role.RolePermissions)
                .ThenInclude(rolePermission => rolePermission.Permission)
            .Include(role => role.UserRoles)
                .ThenInclude(userRole => userRole.User)
            .AsQueryable();

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(role => role.Id == id, cancellationToken);
    }

    private async Task<IReadOnlyList<Permission>> ListPermissionsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Permissions
            .AsNoTracking()
            .OrderBy(permission => permission.Key)
            .ToListAsync(cancellationToken);
    }

    private async Task<IdValidationResult> EnsureRolesExistAsync(
        HashSet<Guid> roleIds,
        bool requireAtLeastOne,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

        if (requireAtLeastOne && roleIds.Count == 0)
        {
            AddError(errors, "RoleIds", "At least one role is required.");
            return new IdValidationResult(errors);
        }

        if (roleIds.Count == 0)
        {
            return new IdValidationResult(errors);
        }

        var existingRoleIds = await dbContext.Roles
            .Where(role => roleIds.Contains(role.Id))
            .Select(role => role.Id)
            .ToListAsync(cancellationToken);

        if (existingRoleIds.Count != roleIds.Count)
        {
            AddError(errors, "RoleIds", "One or more roles do not exist.");
        }

        return new IdValidationResult(errors);
    }

    private async Task<IdValidationResult> EnsurePermissionsExistAsync(
        HashSet<Guid> permissionIds,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

        if (permissionIds.Count == 0)
        {
            return new IdValidationResult(errors);
        }

        var existingPermissionIds = await dbContext.Permissions
            .Where(permission => permissionIds.Contains(permission.Id))
            .Select(permission => permission.Id)
            .ToListAsync(cancellationToken);

        if (existingPermissionIds.Count != permissionIds.Count)
        {
            AddError(errors, "PermissionIds", "One or more permissions do not exist.");
        }

        return new IdValidationResult(errors);
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
            if (!existingRoleIds.Contains(desiredRoleId))
            {
                dbContext.UserRoles.Add(new UserRole(userId, desiredRoleId));
            }
        }
    }

    private async Task SynchronizeRolePermissionsAsync(
        Guid roleId,
        HashSet<Guid> desiredPermissionIds,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.RolePermissions
            .Where(rolePermission => rolePermission.RoleId == roleId)
            .ToListAsync(cancellationToken);
        var existingIds = existing.Select(rolePermission => rolePermission.PermissionId).ToHashSet();

        dbContext.RolePermissions.RemoveRange(
            existing.Where(rolePermission => !desiredPermissionIds.Contains(rolePermission.PermissionId)));

        foreach (var permissionId in desiredPermissionIds)
        {
            if (!existingIds.Contains(permissionId))
            {
                dbContext.RolePermissions.Add(new RolePermission(roleId, permissionId));
            }
        }
    }

    private async Task SynchronizeUserPermissionOverridesAsync(
        Guid userId,
        IReadOnlyDictionary<Guid, UserPermissionOverrideEffect> desiredOverrides,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.UserPermissionOverrides
            .Where(permissionOverride => permissionOverride.UserId == userId)
            .ToListAsync(cancellationToken);
        var existingByPermissionId = existing.ToDictionary(
            permissionOverride => permissionOverride.PermissionId);

        dbContext.UserPermissionOverrides.RemoveRange(
            existing.Where(permissionOverride => !desiredOverrides.ContainsKey(permissionOverride.PermissionId)));

        foreach (var desired in desiredOverrides)
        {
            if (existingByPermissionId.TryGetValue(desired.Key, out var permissionOverride))
            {
                permissionOverride.SetEffect(desired.Value);
            }
            else
            {
                dbContext.UserPermissionOverrides.Add(new UserPermissionOverride(userId, desired.Key, desired.Value));
            }
        }
    }

    private async Task<bool> AnyOtherActiveUserWithPermissionAsync(
        Guid excludedUserId,
        string permission,
        CancellationToken cancellationToken)
    {
        var users = await dbContext.Users
            .Where(user => user.Id != excludedUserId && user.IsActive)
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                    .ThenInclude(role => role!.RolePermissions)
                        .ThenInclude(rolePermission => rolePermission.Permission)
            .Include(user => user.PermissionOverrides)
                .ThenInclude(permissionOverride => permissionOverride.Permission)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return users.Any(user => UserGrantsPermission(user, permission));
    }

    private async Task<bool> AnyActiveUserWithPermissionAfterRoleChangeAsync(
        Guid changedRoleId,
        HashSet<Guid> desiredPermissionIds,
        Permission permission,
        CancellationToken cancellationToken)
    {
        var users = await dbContext.Users
            .Where(user => user.IsActive)
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                    .ThenInclude(role => role!.RolePermissions)
                        .ThenInclude(rolePermission => rolePermission.Permission)
            .Include(user => user.PermissionOverrides)
                .ThenInclude(permissionOverride => permissionOverride.Permission)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return users.Any(user => GrantsPermissionAfterRoleChange(
            user,
            changedRoleId,
            desiredPermissionIds,
            permission));
    }

    private static bool GrantsPermissionAfterRoleChange(
        User user,
        Guid changedRoleId,
        HashSet<Guid> desiredPermissionIds,
        Permission permission)
    {
        if (SecurityIdentityMapper.IsAdmin(user))
        {
            return true;
        }

        var overrideEffect = user.PermissionOverrides
            .FirstOrDefault(permissionOverride => permissionOverride.PermissionId == permission.Id)
            ?.Effect;

        if (overrideEffect == UserPermissionOverrideEffect.Deny)
        {
            return false;
        }

        if (overrideEffect == UserPermissionOverrideEffect.Allow)
        {
            return true;
        }

        return user.UserRoles.Any(userRole =>
            userRole.RoleId == changedRoleId
                ? desiredPermissionIds.Contains(permission.Id)
                : userRole.Role?.RolePermissions.Any(
                    rolePermission => rolePermission.PermissionId == permission.Id) == true);
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

    private async Task<bool> RolesContainAdminAsync(
        HashSet<Guid> roleIds,
        CancellationToken cancellationToken)
    {
        return roleIds.Count > 0
            && await dbContext.Roles.AnyAsync(
                role => roleIds.Contains(role.Id) && role.NormalizedName == NormalizedAdminRoleName,
                cancellationToken);
    }

    private static UserPermissionOverrideEffect? GetOverrideEffect(User user, string permission)
    {
        return user.PermissionOverrides
            .FirstOrDefault(permissionOverride => permissionOverride.Permission?.Key == permission)
            ?.Effect;
    }

    private static bool GrantsPermissionWithOverrides(
        User user,
        string permissionKey,
        Guid? permissionId,
        IReadOnlyDictionary<Guid, UserPermissionOverrideEffect> desiredOverrides)
    {
        if (permissionId.HasValue && desiredOverrides.TryGetValue(permissionId.Value, out var effect))
        {
            return effect == UserPermissionOverrideEffect.Allow;
        }

        return user.UserRoles
            .Select(userRole => userRole.Role)
            .Where(role => role is not null)
            .SelectMany(role => role!.RolePermissions)
            .Select(rolePermission => rolePermission.Permission)
            .Any(permission => permission?.Key == permissionKey);
    }

    private static bool UserGrantsPermission(User user, string permission)
    {
        return SecurityIdentityMapper.GetPermissionKeys(user).Contains(permission, StringComparer.Ordinal);
    }

    private static bool IsRolePermissionEditingLocked(Role role)
    {
        return role.NormalizedName == NormalizedAdminRoleName;
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

    private static ValidatedInput<Dictionary<Guid, UserPermissionOverrideEffect>> ValidatePermissionOverrides(
        IReadOnlyCollection<AdminUserPermissionOverrideRequest>? overrides)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var result = new Dictionary<Guid, UserPermissionOverrideEffect>();

        foreach (var permissionOverride in overrides ?? [])
        {
            if (permissionOverride.PermissionId == Guid.Empty)
            {
                AddError(errors, "Overrides", "Permission id is required for every override.");
                continue;
            }

            if (result.ContainsKey(permissionOverride.PermissionId))
            {
                AddError(errors, "Overrides", "Each permission can have at most one override.");
                continue;
            }

            if (!Enum.TryParse<UserPermissionOverrideEffect>(permissionOverride.Effect, ignoreCase: true, out var effect)
                || !Enum.IsDefined(effect))
            {
                AddError(errors, "Overrides", "Override effect must be Allow or Deny.");
                continue;
            }

            result.Add(permissionOverride.PermissionId, effect);
        }

        return errors.Count > 0
            ? new ValidatedInput<Dictionary<Guid, UserPermissionOverrideEffect>>(null, errors)
            : new ValidatedInput<Dictionary<Guid, UserPermissionOverrideEffect>>(result, errors);
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

    private static AdminUserDetailResponse MapUserDetail(User user, IReadOnlyCollection<Permission> permissions)
    {
        var isLocked = SecurityIdentityMapper.IsAdmin(user);
        var permissionStates = permissions
            .Select(permission =>
            {
                var sourceRoles = user.UserRoles
                    .Where(userRole => userRole.Role?.RolePermissions.Any(
                        rolePermission => rolePermission.PermissionId == permission.Id) == true)
                    .Select(userRole => userRole.Role!.Name)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(role => role, StringComparer.Ordinal)
                    .ToArray();
                var inherited = sourceRoles.Length > 0;
                var permissionOverride = isLocked
                    ? null
                    : user.PermissionOverrides.FirstOrDefault(
                        currentOverride => currentOverride.PermissionId == permission.Id);
                var effective = permissionOverride?.Effect switch
                {
                    UserPermissionOverrideEffect.Allow => true,
                    UserPermissionOverrideEffect.Deny => false,
                    _ => inherited
                };

                return new AdminUserPermissionResponse(
                    permission.Id,
                    permission.Key,
                    permission.Description,
                    inherited,
                    effective,
                    permissionOverride?.Effect.ToString(),
                    sourceRoles);
            })
            .OrderBy(permission => permission.Key, StringComparer.Ordinal)
            .ToArray();

        return new AdminUserDetailResponse(
            user.Id,
            user.Email,
            user.FullName,
            user.IsActive,
            MapUserRoles(user),
            isLocked,
            permissionStates,
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
            IsRolePermissionEditingLocked(role),
            role.UserRoles.Count,
            permissions.Length,
            permissions);
    }

    private static AdminRoleDetailResponse MapRoleDetail(
        Role role,
        IReadOnlyCollection<Permission> availablePermissions)
    {
        return new AdminRoleDetailResponse(
            role.Id,
            role.Name,
            role.Description,
            role.IsSystem,
            IsRolePermissionEditingLocked(role),
            role.UserRoles.Count,
            role.UserRoles.Count(userRole => userRole.User?.IsActive == true),
            MapRolePermissions(role),
            availablePermissions
                .Select(permission => new AdminPermissionResponse(
                    permission.Id,
                    permission.Key,
                    permission.Description))
                .OrderBy(permission => permission.Key, StringComparer.Ordinal)
                .ToArray());
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

    private static HashSet<Guid> NormalizeIds(IReadOnlyCollection<Guid>? ids)
    {
        return (ids ?? [])
            .Where(id => id != Guid.Empty)
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

    private sealed record IdValidationResult(IReadOnlyDictionary<string, string[]> Errors);

    private sealed record ValidatedInput<T>(T? Value, IReadOnlyDictionary<string, string[]> Errors);

    private sealed record ValidatedCreateUser(string Email, string FullName, string TemporaryPassword);

    private sealed record ValidatedUpdateUser(string Email, string FullName);
}
