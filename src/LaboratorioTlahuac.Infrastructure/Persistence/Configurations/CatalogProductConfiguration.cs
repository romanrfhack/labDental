using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LaboratorioTlahuac.Domain.Catalog.Entities;

namespace LaboratorioTlahuac.Infrastructure.Persistence.Configurations;

public sealed class CatalogProductConfiguration : IEntityTypeConfiguration<CatalogProduct>
{
    public void Configure(EntityTypeBuilder<CatalogProduct> builder)
    {
        builder.ToTable("CatalogProducts");

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Key)
            .HasMaxLength(CatalogProduct.KeyMaxLength)
            .IsRequired();

        builder.Property(product => product.Name)
            .HasMaxLength(CatalogProduct.NameMaxLength)
            .IsRequired();

        builder.Property(product => product.Description)
            .HasMaxLength(CatalogProduct.DescriptionMaxLength);

        builder.Property(product => product.PriceAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(product => product.Currency)
            .HasMaxLength(CatalogProduct.CurrencyMaxLength)
            .IsRequired();

        builder.Property(product => product.ImagePath)
            .HasMaxLength(CatalogProduct.ImagePathMaxLength);

        builder.Property(product => product.AltText)
            .HasMaxLength(CatalogProduct.AltTextMaxLength);

        builder.Property(product => product.SortOrder)
            .IsRequired();

        builder.Property(product => product.IsActive)
            .IsRequired();

        builder.Property(product => product.CreatedAtUtc)
            .IsRequired();

        builder.Property(product => product.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(product => product.Key)
            .IsUnique();
        builder.HasIndex(product => product.CatalogSectionId);
        builder.HasIndex(product => product.SortOrder);
        builder.HasIndex(product => product.IsActive);
    }
}
