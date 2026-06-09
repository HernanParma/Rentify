using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class ReservationStatusMapping : IEntityTypeConfiguration<ReservationStatus>
{
    public void Configure(EntityTypeBuilder<ReservationStatus> builder)
    {
        builder.ToTable("ReservationStatuses");
        builder.HasKey(s => s.ReservationStatusId);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(50);
    }
}
