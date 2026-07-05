using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LaboratorioTlahuac.Domain.Catalog.Entities;

namespace LaboratorioTlahuac.Infrastructure.Persistence.Configurations;

public sealed class CatalogSectionConfiguration : IEntityTypeConfiguration<CatalogSection>
{
    public void Configure(EntityTypeBuilder<CatalogSection> builder)
    {
        builder.ToTable("CatalogSections");

        builder.HasKey(section => section.Id);

        builder.Property(section => section.Key)
            .HasMaxLength(CatalogSection.KeyMaxLength)
            .IsRequired();

        builder.Property(section => section.Name)
            .HasMaxLength(CatalogSection.NameMaxLength)
            .IsRequired();

        builder.Property(section => section.Description)
            .HasMaxLength(CatalogSection.DescriptionMaxLength);

        builder.Property(section => section.ImagePath)
            .HasMaxLength(CatalogSection.ImagePathMaxLength);

        builder.Property(section => section.AltText)
            .HasMaxLength(CatalogSection.AltTextMaxLength);

        builder.Property(section => section.SortOrder)
            .IsRequired();

        builder.Property(section => section.IsActive)
            .IsRequired();

        builder.Property(section => section.CreatedAtUtc)
            .IsRequired();

        builder.Property(section => section.UpdatedAtUtc)
            .IsRequired();

        builder.HasMany(section => section.Products)
            .WithOne(product => product.CatalogSection)
            .HasForeignKey(product => product.CatalogSectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(section => section.Key)
            .IsUnique();
        builder.HasIndex(section => section.SortOrder);
        builder.HasIndex(section => section.IsActive);
    }
}
