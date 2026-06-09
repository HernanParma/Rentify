using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class VehicleMapping : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");
        builder.HasKey(v => v.VehicleId);
        builder.Property(v => v.VehicleId).HasColumnType("uniqueidentifier");
        builder.Property(v => v.Brand).IsRequired().HasMaxLength(100);
        builder.Property(v => v.Model).IsRequired().HasMaxLength(100);
        builder.Property(v => v.Plate).IsRequired().HasMaxLength(20);
        builder.Property(v => v.PricePerDay).HasColumnType("decimal(18,2)");
        builder.Property(v => v.Insurance).IsRequired().HasMaxLength(200);

        builder.HasOne(v => v.VehicleStatus)
            .WithMany(s => s.Vehicles)
            .HasForeignKey(v => v.VehicleStatusId);
    }
}
