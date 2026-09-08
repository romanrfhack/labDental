using Microsoft.EntityFrameworkCore;
using LaboratorioTlahuac.Domain.Catalog.Entities;
using LaboratorioTlahuac.Domain.Customers.Entities;
using LaboratorioTlahuac.Domain.Deliveries.Entities;
using LaboratorioTlahuac.Domain.Payments.Entities;
using LaboratorioTlahuac.Domain.Security.Entities;
using LaboratorioTlahuac.Domain.WorkOrders.Entities;

namespace LaboratorioTlahuac.Infrastructure.Persistence;

public sealed class LaboratorioTlahuacDbContext(DbContextOptions<LaboratorioTlahuacDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<UserPermissionOverride> UserPermissionOverrides => Set<UserPermissionOverride>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<InternalDoctor> InternalDoctors => Set<InternalDoctor>();

    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    public DbSet<WorkOrderStatusHistory> WorkOrderStatusHistory => Set<WorkOrderStatusHistory>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<WorkOrderDelivery> WorkOrderDeliveries => Set<WorkOrderDelivery>();

    public DbSet<CatalogSection> CatalogSections => Set<CatalogSection>();

    public DbSet<CatalogProduct> CatalogProducts => Set<CatalogProduct>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LaboratorioTlahuacDbContext).Assembly);
    }
}
