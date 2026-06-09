using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class ReservationMapping : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations");
        builder.HasKey(r => r.ReservationId);
        builder.Property(r => r.ReservationId).HasColumnType("uniqueidentifier");
        builder.Property(r => r.VehicleId).HasColumnType("uniqueidentifier");
        builder.Property(r => r.HourlyRateSnapshot).HasColumnType("decimal(18,2)");
        builder.Property(r => r.TotalCost).HasColumnType("decimal(18,2)");

        builder.HasOne(r => r.ReservationStatus)
            .WithMany(s => s.Reservations)
            .HasForeignKey(r => r.ReservationStatusId);
    }
}
