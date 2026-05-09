using Microsoft.EntityFrameworkCore;
using LaboratorioTlahuac.Domain.Customers.Entities;
using LaboratorioTlahuac.Domain.Security.Entities;

namespace LaboratorioTlahuac.Infrastructure.Persistence;

public sealed class LaboratorioTlahuacDbContext(DbContextOptions<LaboratorioTlahuacDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<InternalDoctor> InternalDoctors => Set<InternalDoctor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LaboratorioTlahuacDbContext).Assembly);
    }
}
