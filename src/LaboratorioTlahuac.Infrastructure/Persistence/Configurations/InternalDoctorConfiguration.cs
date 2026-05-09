using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LaboratorioTlahuac.Domain.Customers.Entities;

namespace LaboratorioTlahuac.Infrastructure.Persistence.Configurations;

public sealed class InternalDoctorConfiguration : IEntityTypeConfiguration<InternalDoctor>
{
    public void Configure(EntityTypeBuilder<InternalDoctor> builder)
    {
        builder.ToTable("InternalDoctors");

        builder.HasKey(internalDoctor => internalDoctor.Id);

        builder.Property(internalDoctor => internalDoctor.FullName)
            .HasMaxLength(InternalDoctor.FullNameMaxLength)
            .IsRequired();

        builder.Property(internalDoctor => internalDoctor.Phone)
            .HasMaxLength(Customer.PhoneMaxLength);

        builder.Property(internalDoctor => internalDoctor.WhatsApp)
            .HasMaxLength(Customer.WhatsAppMaxLength);

        builder.Property(internalDoctor => internalDoctor.Email)
            .HasMaxLength(Customer.EmailMaxLength);

        builder.Property(internalDoctor => internalDoctor.Notes)
            .HasMaxLength(Customer.NotesMaxLength);

        builder.Property(internalDoctor => internalDoctor.IsActive)
            .IsRequired();

        builder.Property(internalDoctor => internalDoctor.CreatedAtUtc)
            .IsRequired();

        builder.Property(internalDoctor => internalDoctor.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(internalDoctor => internalDoctor.CustomerId);
        builder.HasIndex(internalDoctor => internalDoctor.IsActive);
    }
}
