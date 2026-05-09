using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LaboratorioTlahuac.Domain.Security.Entities;

namespace LaboratorioTlahuac.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles", "Security");

        builder.HasKey(role => role.Id);

        builder.Property(role => role.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(role => role.NormalizedName)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(role => role.NormalizedName)
            .IsUnique();

        builder.Property(role => role.Description)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(role => role.IsSystem)
            .IsRequired();

        builder.Property(role => role.CreatedAtUtc)
            .IsRequired();

        builder.Property(role => role.UpdatedAtUtc)
            .IsRequired();
    }
}
