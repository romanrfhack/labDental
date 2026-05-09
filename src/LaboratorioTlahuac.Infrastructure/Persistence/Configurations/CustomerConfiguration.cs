using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LaboratorioTlahuac.Domain.Customers.Entities;

namespace LaboratorioTlahuac.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.Type)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(customer => customer.DisplayName)
            .HasMaxLength(Customer.DisplayNameMaxLength)
            .IsRequired();

        builder.Property(customer => customer.LegalName)
            .HasMaxLength(Customer.LegalNameMaxLength);

        builder.Property(customer => customer.ContactName)
            .HasMaxLength(Customer.ContactNameMaxLength);

        builder.Property(customer => customer.Phone)
            .HasMaxLength(Customer.PhoneMaxLength);

        builder.Property(customer => customer.WhatsApp)
            .HasMaxLength(Customer.WhatsAppMaxLength);

        builder.Property(customer => customer.Email)
            .HasMaxLength(Customer.EmailMaxLength);

        builder.Property(customer => customer.Address)
            .HasMaxLength(Customer.AddressMaxLength);

        builder.Property(customer => customer.Notes)
            .HasMaxLength(Customer.NotesMaxLength);

        builder.Property(customer => customer.IsActive)
            .IsRequired();

        builder.Property(customer => customer.CreatedAtUtc)
            .IsRequired();

        builder.Property(customer => customer.UpdatedAtUtc)
            .IsRequired();

        builder.HasMany(customer => customer.InternalDoctors)
            .WithOne(internalDoctor => internalDoctor.Customer)
            .HasForeignKey(internalDoctor => internalDoctor.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(customer => customer.Type);
        builder.HasIndex(customer => customer.IsActive);
        builder.HasIndex(customer => customer.DisplayName);
    }
}
