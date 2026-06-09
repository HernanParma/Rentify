using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seeders;

public static class VehicleSeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vehicle>().HasData(
            new Vehicle { VehicleId = new Guid("11111111-1111-1111-1111-111111111101"), Brand = "Toyota", Model = "Corolla", Year = 2023, Plate = "AB123CD", VehicleStatusId = 1, PricePerDay = 48000m, BranchOfficeId = 1, Insurance = "Allianz Full" },
            new Vehicle { VehicleId = new Guid("11111111-1111-1111-1111-111111111102"), Brand = "Ford", Model = "Ranger", Year = 2022, Plate = "AC234DE", VehicleStatusId = 1, PricePerDay = 65000m, BranchOfficeId = 1, Insurance = "Sancor Premium" },
            new Vehicle { VehicleId = new Guid("11111111-1111-1111-1111-111111111103"), Brand = "Volkswagen", Model = "Gol", Year = 2021, Plate = "AD345EF", VehicleStatusId = 2, PricePerDay = 35000m, BranchOfficeId = 1, Insurance = "La Caja Básica" },

            new Vehicle { VehicleId = new Guid("22222222-2222-2222-2222-222222222201"), Brand = "Honda", Model = "Civic", Year = 2024, Plate = "AE456FG", VehicleStatusId = 1, PricePerDay = 52000m, BranchOfficeId = 2, Insurance = "Allianz Full" },
            new Vehicle { VehicleId = new Guid("22222222-2222-2222-2222-222222222202"), Brand = "Chevrolet", Model = "Onix", Year = 2023, Plate = "AF567GH", VehicleStatusId = 1, PricePerDay = 40000m, BranchOfficeId = 2, Insurance = "Sancor Premium" },
            new Vehicle { VehicleId = new Guid("22222222-2222-2222-2222-222222222203"), Brand = "Fiat", Model = "Cronos", Year = 2022, Plate = "AG678HI", VehicleStatusId = 3, PricePerDay = 38000m, BranchOfficeId = 2, Insurance = "La Caja Básica" },

            new Vehicle { VehicleId = new Guid("33333333-3333-3333-3333-333333333301"), Brand = "Renault", Model = "Kwid", Year = 2023, Plate = "AH789IJ", VehicleStatusId = 1, PricePerDay = 32000m, BranchOfficeId = 3, Insurance = "Allianz Full" },
            new Vehicle { VehicleId = new Guid("33333333-3333-3333-3333-333333333302"), Brand = "Peugeot", Model = "208", Year = 2022, Plate = "AI890JK", VehicleStatusId = 1, PricePerDay = 42000m, BranchOfficeId = 3, Insurance = "Sancor Premium" },
            new Vehicle { VehicleId = new Guid("33333333-3333-3333-3333-333333333303"), Brand = "Toyota", Model = "Hilux", Year = 2023, Plate = "AJ901KL", VehicleStatusId = 1, PricePerDay = 70000m, BranchOfficeId = 3, Insurance = "Allianz Full" },

            new Vehicle { VehicleId = new Guid("44444444-4444-4444-4444-444444444401"), Brand = "Nissan", Model = "Versa", Year = 2023, Plate = "AK012LM", VehicleStatusId = 1, PricePerDay = 41000m, BranchOfficeId = 4, Insurance = "La Caja Básica" },
            new Vehicle { VehicleId = new Guid("44444444-4444-4444-4444-444444444402"), Brand = "Jeep", Model = "Renegade", Year = 2022, Plate = "AL123MN", VehicleStatusId = 2, PricePerDay = 55000m, BranchOfficeId = 4, Insurance = "Sancor Premium" },
            new Vehicle { VehicleId = new Guid("44444444-4444-4444-4444-444444444403"), Brand = "Volkswagen", Model = "Amarok", Year = 2021, Plate = "AM234NO", VehicleStatusId = 1, PricePerDay = 68000m, BranchOfficeId = 4, Insurance = "Allianz Full" },

            new Vehicle { VehicleId = new Guid("55555555-5555-5555-5555-555555555501"), Brand = "Fiat", Model = "Argo", Year = 2024, Plate = "AN345OP", VehicleStatusId = 1, PricePerDay = 39000m, BranchOfficeId = 5, Insurance = "La Caja Básica" },
            new Vehicle { VehicleId = new Guid("55555555-5555-5555-5555-555555555502"), Brand = "Ford", Model = "EcoSport", Year = 2022, Plate = "AO456PQ", VehicleStatusId = 1, PricePerDay = 44000m, BranchOfficeId = 5, Insurance = "Sancor Premium" },
            new Vehicle { VehicleId = new Guid("55555555-5555-5555-5555-555555555503"), Brand = "Chevrolet", Model = "Cruze", Year = 2023, Plate = "AP567QR", VehicleStatusId = 1, PricePerDay = 47000m, BranchOfficeId = 5, Insurance = "Allianz Full" }
        );
    }
}
