using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using LaboratorioTlahuac.Domain.Security;
using LaboratorioTlahuac.Domain.Security.Entities;
using LaboratorioTlahuac.Infrastructure.Persistence;
using LaboratorioTlahuac.Infrastructure.Security.Seed;

namespace LaboratorioTlahuac.Api.Tests;

public sealed class SecuritySeederTests
{
    private const string QaEmail = "qa-limited@tests.local";
    private const string QaPassword = "QaLimitedPass123!";
    private const string QaFullName = "QA Limited Test";

    [Fact]
    public async Task LimitedQaUserSeedDoesNotCreateOutsideDevelopment()
    {
        await using var harness = await SecuritySeederHarness.CreateAsync(
            CompleteLimitedQaSettings(),
            isDevelopment: false);

        await harness.RunSeederAsync();

        Assert.Null(await FindUserAsync(harness.DbContext, QaEmail));
    }

    [Fact]
    public async Task LimitedQaUserSeedDoesNotCreateWhenDisabled()
    {
        var settings = CompleteLimitedQaSettings();
        settings["SecuritySeed:LimitedQaUser:RunOnStartup"] = "false";
        await using var harness = await SecuritySeederHarness.CreateAsync(settings);

        await harness.RunSeederAsync();

        Assert.Null(await FindUserAsync(harness.DbContext, QaEmail));
    }

    [Theory]
    [InlineData("SecuritySeed:LimitedQaUser:Email")]
    [InlineData("SecuritySeed:LimitedQaUser:Password")]
    [InlineData("SecuritySeed:LimitedQaUser:FullName")]
    public async Task LimitedQaUserSeedDoesNotCreateWhenRequiredConfigurationIsMissing(string missingKey)
    {
        var settings = CompleteLimitedQaSettings();
        settings[missingKey] = "";
        await using var harness = await SecuritySeederHarness.CreateAsync(settings);

        await harness.RunSeederAsync();

        Assert.Null(await FindUserAsync(harness.DbContext, QaEmail));
    }

    [Fact]
    public async Task LimitedQaUserSeedCreatesUserInDevelopmentWithCompleteConfiguration()
    {
        await using var harness = await SecuritySeederHarness.CreateAsync(CompleteLimitedQaSettings());

        await harness.RunSeederAsync();

        var user = await FindUserAsync(harness.DbContext, QaEmail);

        Assert.NotNull(user);
        Assert.True(user.IsActive);
        Assert.Equal(QaFullName, user.FullName);
    }

    [Fact]
    public async Task LimitedQaUserSeedWithoutConfiguredPermissionsDoesNotGrantReportsView()
    {
        var settings = CompleteLimitedQaSettings();
        settings.Remove("SecuritySeed:LimitedQaUser:Permissions");
        await using var harness = await SecuritySeederHarness.CreateAsync(settings);

        await harness.RunSeederAsync();

        var permissionKeys = await GetUserPermissionKeysAsync(harness.DbContext, QaEmail);

        Assert.Empty(permissionKeys);
        Assert.DoesNotContain(Permissions.ReportsView, permissionKeys);
    }

    [Fact]
    public async Task LimitedQaUserSeedGrantsOnlyExplicitConfiguredPermissions()
    {
        await using var harness = await SecuritySeederHarness.CreateAsync(CompleteLimitedQaSettings());

        await harness.RunSeederAsync();

        var permissionKeys = await GetUserPermissionKeysAsync(harness.DbContext, QaEmail);

        Assert.Equal([Permissions.CustomersView], permissionKeys);
        Assert.DoesNotContain(Permissions.ReportsView, permissionKeys);
    }

    [Fact]
    public async Task LimitedQaUserSeedDoesNotAlterExistingAdmin()
    {
        await using var harness = await SecuritySeederHarness.CreateAsync(CompleteLimitedQaSettings());
        var adminSnapshot = await SeedAdminAsync(harness.DbContext);

        await harness.RunSeederAsync();

        var admin = await harness.DbContext.Users
            .AsNoTracking()
            .SingleAsync(user => user.Id == adminSnapshot.Id);
        var adminPermissionCount = await GetUserPermissionKeysAsync(harness.DbContext, adminSnapshot.Email);

        Assert.Equal(adminSnapshot.Email, admin.Email);
        Assert.Equal(adminSnapshot.FullName, admin.FullName);
        Assert.Equal(adminSnapshot.PasswordHash, admin.PasswordHash);
        Assert.Equal(Permissions.All.Count, adminPermissionCount.Count);
        Assert.DoesNotContain("Limited QA", await GetUserRoleNamesAsync(harness.DbContext, adminSnapshot.Email));
    }

    [Fact]
    public async Task BaselineSeedCreatesDeliveryPermissionsAndDriverRolePermissions()
    {
        await using var harness = await SecuritySeederHarness.CreateAsync(
            new Dictionary<string, string?>
            {
                ["SecuritySeed:EnsureBaselineOnStartup"] = "true",
                ["SecuritySeed:RunOnStartup"] = "false",
                ["SecuritySeed:LimitedQaUser:RunOnStartup"] = "false"
            });

        await harness.RunSeederAsync();

        var permissionKeys = await harness.DbContext.Permissions
            .AsNoTracking()
            .Select(permission => permission.Key)
            .ToListAsync();
        var driverPermissionKeys = await GetRolePermissionKeysAsync(harness.DbContext, "Repartidor");

        Assert.Contains(Permissions.DeliveriesView, permissionKeys);
        Assert.Contains(Permissions.DeliveriesAssign, permissionKeys);
        Assert.Contains(Permissions.DeliveriesUpdate, permissionKeys);
        Assert.Contains(Permissions.DeliveriesComplete, permissionKeys);
        Assert.Contains(Permissions.CatalogView, permissionKeys);
        Assert.Contains(Permissions.CatalogManage, permissionKeys);
        Assert.Equal(
            [Permissions.DeliveriesComplete, Permissions.DeliveriesView],
            driverPermissionKeys);
        Assert.DoesNotContain(Permissions.UsersManage, driverPermissionKeys);
        Assert.DoesNotContain(Permissions.RolesManage, driverPermissionKeys);
        Assert.DoesNotContain(Permissions.OrdersView, driverPermissionKeys);
        Assert.DoesNotContain(Permissions.CatalogView, driverPermissionKeys);
        Assert.DoesNotContain(Permissions.CatalogManage, driverPermissionKeys);
    }

    [Fact]
    public async Task BaselineSeedAddsMissingPermissionsToExistingAdminRole()
    {
        await using var harness = await SecuritySeederHarness.CreateAsync(
            new Dictionary<string, string?>
            {
                ["SecuritySeed:EnsureBaselineOnStartup"] = "true",
                ["SecuritySeed:RunOnStartup"] = "false",
                ["SecuritySeed:LimitedQaUser:RunOnStartup"] = "false"
            });
        var now = DateTimeOffset.UtcNow;
        var existingPermission = Permission.Create(
            Permissions.OrdersView,
            Permissions.Descriptions[Permissions.OrdersView],
            now);
        var adminRole = Role.Create("Admin", "Administrador del sistema.", isSystem: true, now);

        harness.DbContext.Permissions.Add(existingPermission);
        harness.DbContext.Roles.Add(adminRole);
        harness.DbContext.RolePermissions.Add(new RolePermission(adminRole.Id, existingPermission.Id));
        await harness.DbContext.SaveChangesAsync();

        await harness.RunSeederAsync();

        var adminPermissionKeys = await GetRolePermissionKeysAsync(harness.DbContext, "Admin");

        Assert.Equal(Permissions.All.Count, adminPermissionKeys.Count);
        Assert.Contains(Permissions.DeliveriesView, adminPermissionKeys);
        Assert.Contains(Permissions.DeliveriesAssign, adminPermissionKeys);
        Assert.Contains(Permissions.DeliveriesUpdate, adminPermissionKeys);
        Assert.Contains(Permissions.DeliveriesComplete, adminPermissionKeys);
        Assert.Contains(Permissions.CatalogView, adminPermissionKeys);
        Assert.Contains(Permissions.CatalogManage, adminPermissionKeys);
    }

    private static Dictionary<string, string?> CompleteLimitedQaSettings()
    {
        return new Dictionary<string, string?>
        {
            ["SecuritySeed:RunOnStartup"] = "false",
            ["SecuritySeed:LimitedQaUser:RunOnStartup"] = "true",
            ["SecuritySeed:LimitedQaUser:Email"] = QaEmail,
            ["SecuritySeed:LimitedQaUser:Password"] = QaPassword,
            ["SecuritySeed:LimitedQaUser:FullName"] = QaFullName,
            ["SecuritySeed:LimitedQaUser:Permissions"] = Permissions.CustomersView
        };
    }

    private static async Task<User?> FindUserAsync(LaboratorioTlahuacDbContext dbContext, string email)
    {
        var normalizedEmail = SecurityTextNormalizer.NormalizeEmail(email);

        return await dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail);
    }

    private static async Task<IReadOnlyList<string>> GetUserPermissionKeysAsync(
        LaboratorioTlahuacDbContext dbContext,
        string email)
    {
        var normalizedEmail = SecurityTextNormalizer.NormalizeEmail(email);
        var user = await dbContext.Users
            .Include(currentUser => currentUser.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                    .ThenInclude(role => role!.RolePermissions)
                        .ThenInclude(rolePermission => rolePermission.Permission)
            .AsNoTracking()
            .SingleAsync(currentUser => currentUser.NormalizedEmail == normalizedEmail);

        return user.UserRoles
            .Select(userRole => userRole.Role)
            .Where(role => role is not null)
            .SelectMany(role => role!.RolePermissions)
            .Select(rolePermission => rolePermission.Permission)
            .Where(permission => permission is not null)
            .Select(permission => permission!.Key)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(permission => permission, StringComparer.Ordinal)
            .ToArray();
    }

    private static async Task<IReadOnlyList<string>> GetUserRoleNamesAsync(
        LaboratorioTlahuacDbContext dbContext,
        string email)
    {
        var normalizedEmail = SecurityTextNormalizer.NormalizeEmail(email);
        var user = await dbContext.Users
            .Include(currentUser => currentUser.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .AsNoTracking()
            .SingleAsync(currentUser => currentUser.NormalizedEmail == normalizedEmail);

        return user.UserRoles
            .Select(userRole => userRole.Role)
            .Where(role => role is not null)
            .Select(role => role!.Name)
            .OrderBy(role => role, StringComparer.Ordinal)
            .ToArray();
    }

    private static async Task<IReadOnlyList<string>> GetRolePermissionKeysAsync(
        LaboratorioTlahuacDbContext dbContext,
        string roleName)
    {
        var normalizedRoleName = SecurityTextNormalizer.NormalizeName(roleName);
        var role = await dbContext.Roles
            .Include(currentRole => currentRole.RolePermissions)
                .ThenInclude(rolePermission => rolePermission.Permission)
            .AsNoTracking()
            .SingleAsync(currentRole => currentRole.NormalizedName == normalizedRoleName);

        return role.RolePermissions
            .Select(rolePermission => rolePermission.Permission)
            .Where(permission => permission is not null)
            .Select(permission => permission!.Key)
            .OrderBy(permission => permission, StringComparer.Ordinal)
            .ToArray();
    }

    private static async Task<AdminSnapshot> SeedAdminAsync(LaboratorioTlahuacDbContext dbContext)
    {
        var now = DateTimeOffset.UtcNow;
        var permissions = Permissions.All
            .Select(permissionKey => Permission.Create(
                permissionKey,
                Permissions.Descriptions.TryGetValue(permissionKey, out var description)
                    ? description
                    : permissionKey,
                now))
            .ToDictionary(permission => permission.Key, StringComparer.Ordinal);
        var adminRole = Role.Create("Admin", "Administrador del sistema.", isSystem: true, now);
        var passwordHasher = new PasswordHasher<User>();
        var admin = User.Create("admin@tests.local", "Admin Test", "pending-password-hash", now);

        admin.SetPasswordHash(passwordHasher.HashPassword(admin, "AdminSeededPass123!"));
        dbContext.Permissions.AddRange(permissions.Values);
        dbContext.Roles.Add(adminRole);
        dbContext.RolePermissions.AddRange(
            permissions.Values.Select(permission => new RolePermission(adminRole.Id, permission.Id)));
        dbContext.Users.Add(admin);
        dbContext.UserRoles.Add(new UserRole(admin.Id, adminRole.Id));
        await dbContext.SaveChangesAsync();

        return new AdminSnapshot(admin.Id, admin.Email, admin.FullName, admin.PasswordHash);
    }

    private sealed record AdminSnapshot(Guid Id, string Email, string FullName, string PasswordHash);

    private sealed class SecuritySeederHarness : IAsyncDisposable
    {
        private readonly SqliteConnection connection;
        private readonly IConfiguration configuration;
        private readonly SecuritySeedRuntimeOptions runtimeOptions;

        private SecuritySeederHarness(
            SqliteConnection connection,
            LaboratorioTlahuacDbContext dbContext,
            IConfiguration configuration,
            SecuritySeedRuntimeOptions runtimeOptions)
        {
            this.connection = connection;
            DbContext = dbContext;
            this.configuration = configuration;
            this.runtimeOptions = runtimeOptions;
        }

        public LaboratorioTlahuacDbContext DbContext { get; }

        public static async Task<SecuritySeederHarness> CreateAsync(
            IReadOnlyDictionary<string, string?> settings,
            bool isDevelopment = true)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<LaboratorioTlahuacDbContext>()
                .UseSqlite(connection)
                .Options;
            var dbContext = new LaboratorioTlahuacDbContext(options);
            await dbContext.Database.EnsureCreatedAsync();

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
            var runtimeOptions = new SecuritySeedRuntimeOptions { IsDevelopment = isDevelopment };

            return new SecuritySeederHarness(connection, dbContext, configuration, runtimeOptions);
        }

        public async Task RunSeederAsync()
        {
            var seeder = new SecuritySeeder(
                DbContext,
                new PasswordHasher<User>(),
                configuration,
                runtimeOptions,
                NullLogger<SecuritySeeder>.Instance);

            await seeder.SeedAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
