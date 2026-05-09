using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LaboratorioTlahuac.Domain.Payments.Entities;
using LaboratorioTlahuac.Domain.Security.Entities;
using LaboratorioTlahuac.Domain.WorkOrders.Entities;

namespace LaboratorioTlahuac.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(payment => payment.Id);

        builder.Property(payment => payment.PaymentDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(payment => payment.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(payment => payment.Method)
            .HasConversion<string>()
            .HasMaxLength(Payment.MethodMaxLength)
            .IsRequired();

        builder.Property(payment => payment.Reference)
            .HasMaxLength(Payment.ReferenceMaxLength);

        builder.Property(payment => payment.Notes)
            .HasMaxLength(Payment.NotesMaxLength);

        builder.Property(payment => payment.IsCancelled)
            .IsRequired();

        builder.Property(payment => payment.CancellationReason)
            .HasMaxLength(Payment.CancellationReasonMaxLength);

        builder.Property(payment => payment.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(payment => payment.WorkOrder)
            .WithMany(workOrder => workOrder.Payments)
            .HasForeignKey(payment => payment.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(payment => payment.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(payment => payment.CancelledByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(payment => payment.WorkOrderId);
        builder.HasIndex(payment => payment.PaymentDate);
        builder.HasIndex(payment => payment.Method);
        builder.HasIndex(payment => payment.IsCancelled);
        builder.HasIndex(payment => payment.CreatedAtUtc);
    }
}
