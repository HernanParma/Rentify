using Application.Interfaces.HttpClients;
using Application.Interfaces.IServices;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Services;

public class ReservationReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ReservationReminderBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRemindersAsync();
            }
            catch { /* continuar en próximo ciclo */ }

            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }

    private async Task ProcessRemindersAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var reservationService = scope.ServiceProvider.GetRequiredService<IReservationService>();

        if (reservationService is not ReservationService svc)
            return;

        var now = DateTime.Now;
        var windowStart = now.AddHours(23);
        var windowEnd = now.AddHours(25);

        var upcomingPickups = await context.Reservations
            .Where(r => r.ReservationStatusId == 2
                && r.StartTime >= windowStart
                && r.StartTime <= windowEnd)
            .ToListAsync();

        foreach (var reservation in upcomingPickups)
        {
            if (await context.ReservationReminders.AnyAsync(r =>
                r.ReservationId == reservation.ReservationId && r.ReminderType == "PickupReminder"))
                continue;

            await svc.SendNotificationAsync(reservation, "PickupReminder");
            await svc.TryRecordReminderAsync(reservation.ReservationId, "PickupReminder");
        }

        var upcomingReturns = await context.Reservations
            .Where(r => r.ReservationStatusId == 3
                && r.EndTime >= windowStart
                && r.EndTime <= windowEnd)
            .ToListAsync();

        foreach (var reservation in upcomingReturns)
        {
            if (await context.ReservationReminders.AnyAsync(r =>
                r.ReservationId == reservation.ReservationId && r.ReminderType == "ReturnReminder"))
                continue;

            await svc.SendNotificationAsync(reservation, "ReturnReminder");
            await svc.TryRecordReminderAsync(reservation.ReservationId, "ReturnReminder");
        }
    }
}
