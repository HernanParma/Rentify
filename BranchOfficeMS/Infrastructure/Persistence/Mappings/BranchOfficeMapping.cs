using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class BranchOfficeMapping : IEntityTypeConfiguration<BranchOffice>
{
    public void Configure(EntityTypeBuilder<BranchOffice> builder)
    {
        builder.ToTable("BranchOffices");
        builder.HasKey(b => b.BranchOfficeId);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Address).IsRequired().HasMaxLength(300);
        builder.Property(b => b.Phone).IsRequired().HasMaxLength(50);
        builder.Property(b => b.Hours).IsRequired().HasMaxLength(100);
        builder.Property(b => b.Latitude).IsRequired();
        builder.Property(b => b.Longitude).IsRequired();
        builder.Property(b => b.IsActive).IsRequired();
    }
}
