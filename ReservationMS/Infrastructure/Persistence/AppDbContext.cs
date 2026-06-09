using Domain.Entities;
using Infrastructure.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<ReservationStatus> ReservationStatuses { get; set; }
    public DbSet<ReservationReminder> ReservationReminders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        ReservationStatusSeeder.Seed(modelBuilder);
    }
}
