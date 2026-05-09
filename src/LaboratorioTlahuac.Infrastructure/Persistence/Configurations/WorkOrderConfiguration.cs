using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LaboratorioTlahuac.Domain.Customers.Entities;
using LaboratorioTlahuac.Domain.Security.Entities;
using LaboratorioTlahuac.Domain.WorkOrders.Entities;

namespace LaboratorioTlahuac.Infrastructure.Persistence.Configurations;

public sealed class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("WorkOrders");

        builder.HasKey(workOrder => workOrder.Id);

        builder.Property(workOrder => workOrder.OrderNumber)
            .HasMaxLength(WorkOrder.OrderNumberMaxLength)
            .IsRequired();

        builder.Property(workOrder => workOrder.PatientName)
            .HasMaxLength(WorkOrder.PatientNameMaxLength)
            .IsRequired();

        builder.Property(workOrder => workOrder.ReceivedDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(workOrder => workOrder.ReferenceNumber)
            .HasMaxLength(WorkOrder.ReferenceNumberMaxLength);

        builder.Property(workOrder => workOrder.WorkDescription)
            .HasMaxLength(WorkOrder.WorkDescriptionMaxLength)
            .IsRequired();

        builder.Property(workOrder => workOrder.DentalColor)
            .HasMaxLength(WorkOrder.DentalColorMaxLength);

        builder.Property(workOrder => workOrder.FirstTrialDate)
            .HasColumnType("date");

        builder.Property(workOrder => workOrder.SecondTrialDate)
            .HasColumnType("date");

        builder.Property(workOrder => workOrder.DeliveryDate)
            .HasColumnType("date");

        builder.Property(workOrder => workOrder.Status)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(workOrder => workOrder.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(workOrder => workOrder.Notes)
            .HasMaxLength(WorkOrder.NotesMaxLength);

        builder.Property(workOrder => workOrder.CreatedAtUtc)
            .IsRequired();

        builder.Property(workOrder => workOrder.UpdatedAtUtc)
            .IsRequired();

        builder.HasOne(workOrder => workOrder.Customer)
            .WithMany(customer => customer.WorkOrders)
            .HasForeignKey(workOrder => workOrder.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(workOrder => workOrder.InternalDoctor)
            .WithMany(internalDoctor => internalDoctor.WorkOrders)
            .HasForeignKey(workOrder => workOrder.InternalDoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(workOrder => workOrder.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(workOrder => workOrder.UpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(workOrder => workOrder.OrderNumber)
            .IsUnique();
        builder.HasIndex(workOrder => workOrder.CustomerId);
        builder.HasIndex(workOrder => workOrder.InternalDoctorId);
        builder.HasIndex(workOrder => workOrder.Status);
        builder.HasIndex(workOrder => workOrder.ReceivedDate);
        builder.HasIndex(workOrder => workOrder.DeliveryDate);
        builder.HasIndex(workOrder => workOrder.PatientName);
    }
}
