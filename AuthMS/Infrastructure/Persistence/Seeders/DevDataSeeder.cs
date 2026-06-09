using Application.Interfaces.IServices.ICryptographyService;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Persistence.Seeders
{
    public static class DevDataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services, IHostEnvironment env)
        {
            if (!env.IsDevelopment()) return;

            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var crypto = scope.ServiceProvider.GetRequiredService<ICryptographyService>();

            if (await db.Users.AnyAsync(u => u.Email == "demo@rentify.com"))
                return;

            var hashedPassword = await crypto.HashPassword("Demo123!");

            db.Users.Add(new User
            {
                Role = UserRoles.Customer,
                IsActive = true,
                IsEmailVerified = true,
                FirstName = "Demo",
                LastName = "Usuario",
                Email = "demo@rentify.com",
                Dni = "12345678",
                Password = hashedPassword,
                ImageUrl = "https://icons.veryicon.com/png/o/internet--web/prejudice/user-128.png",
            });

            db.Users.Add(new User
            {
                Role = UserRoles.Admin,
                IsActive = true,
                IsEmailVerified = true,
                FirstName = "Admin",
                LastName = "Rentify",
                Email = "admin@rentify.com",
                Dni = "87654321",
                Password = hashedPassword,
                ImageUrl = "https://icons.veryicon.com/png/o/internet--web/prejudice/user-128.png",
            });

            db.Users.Add(new User
            {
                Role = UserRoles.Employee,
                IsActive = true,
                IsEmailVerified = true,
                FirstName = "Empleado",
                LastName = "Sede",
                Email = "empleado@rentify.com",
                Dni = "11223344",
                Password = hashedPassword,
                ImageUrl = "https://icons.veryicon.com/png/o/internet--web/prejudice/user-128.png",
            });

            await db.SaveChangesAsync();
        }
    }
}
