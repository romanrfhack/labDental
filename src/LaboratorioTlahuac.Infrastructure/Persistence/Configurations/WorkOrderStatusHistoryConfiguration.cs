using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LaboratorioTlahuac.Domain.Security.Entities;
using LaboratorioTlahuac.Domain.WorkOrders.Entities;

namespace LaboratorioTlahuac.Infrastructure.Persistence.Configurations;

public sealed class WorkOrderStatusHistoryConfiguration : IEntityTypeConfiguration<WorkOrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<WorkOrderStatusHistory> builder)
    {
        builder.ToTable("WorkOrderStatusHistory");

        builder.HasKey(history => history.Id);

        builder.Property(history => history.FromStatus)
            .HasConversion<string>()
            .HasMaxLength(40);

        builder.Property(history => history.ToStatus)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(history => history.Notes)
            .HasMaxLength(WorkOrderStatusHistory.NotesMaxLength);

        builder.Property(history => history.ChangedAtUtc)
            .IsRequired();

        builder.HasOne(history => history.WorkOrder)
            .WithMany(workOrder => workOrder.StatusHistory)
            .HasForeignKey(history => history.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(history => history.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(history => history.WorkOrderId);
        builder.HasIndex(history => history.ChangedAtUtc);
    }
}
