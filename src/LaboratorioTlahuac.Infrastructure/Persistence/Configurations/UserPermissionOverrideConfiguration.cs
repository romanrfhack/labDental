using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LaboratorioTlahuac.Domain.Security.Entities;

namespace LaboratorioTlahuac.Infrastructure.Persistence.Configurations;

public sealed class UserPermissionOverrideConfiguration : IEntityTypeConfiguration<UserPermissionOverride>
{
    public void Configure(EntityTypeBuilder<UserPermissionOverride> builder)
    {
        builder.ToTable("UserPermissionOverrides", "Security");

        builder.HasKey(permissionOverride => new
        {
            permissionOverride.UserId,
            permissionOverride.PermissionId
        });

        builder.Property(permissionOverride => permissionOverride.Effect)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.HasOne(permissionOverride => permissionOverride.User)
            .WithMany(user => user.PermissionOverrides)
            .HasForeignKey(permissionOverride => permissionOverride.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(permissionOverride => permissionOverride.Permission)
            .WithMany(permission => permission.UserPermissionOverrides)
            .HasForeignKey(permissionOverride => permissionOverride.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
