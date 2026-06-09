using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seeders;

public static class BranchOfficeSeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BranchOffice>().HasData(
            new BranchOffice
            {
                BranchOfficeId = 1,
                Name = "Rentify Puerto Madero",
                Address = "Av. Alicia Moreau de Justo 1150, CABA",
                Phone = "+54 11 4000-1001",
                Hours = "Lun-Dom 08:00-22:00",
                Latitude = -34.6101,
                Longitude = -58.3614,
                IsActive = true
            },
            new BranchOffice
            {
                BranchOfficeId = 2,
                Name = "Rentify Palermo",
                Address = "Av. Santa Fe 4200, Palermo, CABA",
                Phone = "+54 11 4000-1002",
                Hours = "Lun-Dom 08:00-20:00",
                Latitude = -34.5865,
                Longitude = -58.4208,
                IsActive = true
            },
            new BranchOffice
            {
                BranchOfficeId = 3,
                Name = "Rentify Recoleta",
                Address = "Av. Del Libertador 1473, Recoleta, CABA",
                Phone = "+54 11 4000-1003",
                Hours = "Lun-Vie 09:00-19:00",
                Latitude = -34.5875,
                Longitude = -58.3972,
                IsActive = true
            },
            new BranchOffice
            {
                BranchOfficeId = 4,
                Name = "Rentify Microcentro",
                Address = "Av. Corrientes 1234, Microcentro, CABA",
                Phone = "+54 11 4000-1004",
                Hours = "Lun-Sab 07:00-21:00",
                Latitude = -34.6037,
                Longitude = -58.3816,
                IsActive = true
            },
            new BranchOffice
            {
                BranchOfficeId = 5,
                Name = "Rentify Belgrano",
                Address = "Av. Cabildo 2200, Belgrano, CABA",
                Phone = "+54 11 4000-1005",
                Hours = "Lun-Dom 09:00-18:00",
                Latitude = -34.5592,
                Longitude = -58.4590,
                IsActive = true
            }
        );
    }
}
