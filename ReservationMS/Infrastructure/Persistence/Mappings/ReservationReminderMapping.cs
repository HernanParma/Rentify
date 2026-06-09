using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class ReservationReminderMapping : IEntityTypeConfiguration<ReservationReminder>
{
    public void Configure(EntityTypeBuilder<ReservationReminder> builder)
    {
        builder.ToTable("ReservationReminders");
        builder.HasKey(r => r.ReservationReminderId);
        builder.HasIndex(r => new { r.ReservationId, r.ReminderType }).IsUnique();
        builder.Property(r => r.ReminderType).HasMaxLength(50);
    }
}
