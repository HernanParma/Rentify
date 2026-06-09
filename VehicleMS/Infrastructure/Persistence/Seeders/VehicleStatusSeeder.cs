using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seeders;

public static class VehicleStatusSeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VehicleStatus>().HasData(
            new VehicleStatus { VehicleStatusId = 1, Name = "Available" },
            new VehicleStatus { VehicleStatusId = 2, Name = "Rented" },
            new VehicleStatus { VehicleStatusId = 3, Name = "Maintenance" }
        );
    }
}
