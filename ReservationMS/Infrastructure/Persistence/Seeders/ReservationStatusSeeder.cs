using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seeders;

public static class ReservationStatusSeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReservationStatus>().HasData(
            new ReservationStatus { ReservationStatusId = 1, Name = "Pending" },
            new ReservationStatus { ReservationStatusId = 2, Name = "Confirmed" },
            new ReservationStatus { ReservationStatusId = 3, Name = "Active" },
            new ReservationStatus { ReservationStatusId = 4, Name = "Completed" },
            new ReservationStatus { ReservationStatusId = 5, Name = "Cancelled" }
        );
    }
}
