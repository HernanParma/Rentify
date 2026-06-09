using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class VehicleStatusMapping : IEntityTypeConfiguration<VehicleStatus>
{
    public void Configure(EntityTypeBuilder<VehicleStatus> builder)
    {
        builder.ToTable("VehicleStatuses");
        builder.HasKey(s => s.VehicleStatusId);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(50);
    }
}
