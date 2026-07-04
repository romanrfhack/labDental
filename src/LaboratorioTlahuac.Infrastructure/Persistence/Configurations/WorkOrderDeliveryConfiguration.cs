using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LaboratorioTlahuac.Domain.Deliveries.Entities;
using LaboratorioTlahuac.Domain.Security.Entities;

namespace LaboratorioTlahuac.Infrastructure.Persistence.Configurations;

public sealed class WorkOrderDeliveryConfiguration : IEntityTypeConfiguration<WorkOrderDelivery>
{
    public void Configure(EntityTypeBuilder<WorkOrderDelivery> builder)
    {
        builder.ToTable("WorkOrderDeliveries");

        builder.HasKey(delivery => delivery.Id);

        builder.Property(delivery => delivery.Status)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(delivery => delivery.RecipientName)
            .HasMaxLength(WorkOrderDelivery.RecipientNameMaxLength);

        builder.Property(delivery => delivery.DeliveryNotes)
            .HasMaxLength(WorkOrderDelivery.DeliveryNotesMaxLength);

        builder.Property(delivery => delivery.FailedReason)
            .HasMaxLength(WorkOrderDelivery.FailedReasonMaxLength);

        builder.Property(delivery => delivery.CreatedAtUtc)
            .IsRequired();

        builder.Property(delivery => delivery.UpdatedAtUtc)
            .IsRequired();

        builder.HasOne(delivery => delivery.WorkOrder)
            .WithMany()
            .HasForeignKey(delivery => delivery.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(delivery => delivery.AssignedToUser)
            .WithMany()
            .HasForeignKey(delivery => delivery.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(delivery => delivery.WorkOrderId)
            .IsUnique();
        builder.HasIndex(delivery => delivery.AssignedToUserId);
        builder.HasIndex(delivery => delivery.Status);
        builder.HasIndex(delivery => delivery.CreatedAtUtc);
    }
}
