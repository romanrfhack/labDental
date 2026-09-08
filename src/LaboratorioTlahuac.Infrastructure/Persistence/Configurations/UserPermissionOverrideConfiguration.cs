using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LaboratorioTlahuac.Domain.Security.Entities;

namespace LaboratorioTlahuac.Infrastructure.Persistence.Configurations;

public sealed class UserPermissionOverrideConfiguration : IEntityTypeConfiguration<UserPermissionOverride>
{
    public void Configure(EntityTypeBuilder<UserPermissionOverride> builder)
    {
        builder.ToTable("UserPermissionOverrides", "Security");

        builder.HasKey(userPermission => new { userPermission.UserId, userPermission.PermissionId });

        builder.Property(userPermission => userPermission.Effect)
            .HasConversion<int>()
            .IsRequired();

        builder.HasOne(userPermission => userPermission.User)
            .WithMany()
            .HasForeignKey(userPermission => userPermission.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(userPermission => userPermission.Permission)
            .WithMany()
            .HasForeignKey(userPermission => userPermission.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(userPermission => userPermission.PermissionId);
    }
}
