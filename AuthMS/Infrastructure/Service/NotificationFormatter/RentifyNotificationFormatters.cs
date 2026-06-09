using Application.Dtos.Notification;
using Application.Interfaces.IServices;
using Domain.Entities;
using System.Text.Json;

namespace Infrastructure.Service.NotificationFormatter;

public abstract class RentifyFormatterBase : INotificationFormatter
{
    protected static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    protected abstract NotificationType HandledType { get; }
    protected abstract string Subject { get; }

    public bool CanHandle(NotificationType type) => type == HandledType;

    public string GetSubject(NotificationType type) => Subject;

    public Task<string> FormatAsync(Notification n, User user)
    {
        var dto = JsonSerializer.Deserialize<ReservationNotificationPayload>(n.Payload!, JsonOpts)
                  ?? new ReservationNotificationPayload();

        return Task.FromResult(BuildHtml(user, dto));
    }

    protected abstract string BuildHtml(User user, ReservationNotificationPayload dto);

    protected static string RentifyHeader() =>
        @"<h2 style='color:#0ea5e9;margin-top:0;'>🚗 Rentify</h2>";

    protected static string Footer() =>
        @"<hr style='border:none;border-top:1px solid #eee;margin:30px 0;'>
          <p style='font-size:12px;color:#666;text-align:center;'>
            Mensaje automático de Rentify. No respondas a este correo.
          </p>";

    protected static string DetailsBox(ReservationNotificationPayload dto) => $@"
        <div style='background:#f8fafc;padding:15px;border-radius:8px;margin:20px 0;'>
          <p><strong>Vehículo:</strong> {dto.VehicleBrand} {dto.VehicleModel} ({dto.Plate})</p>
          <p><strong>Retiro:</strong> {dto.PickupBranchName} — {dto.StartTime:dd/MM/yyyy HH:mm}</p>
          <p><strong>Devolución:</strong> {dto.DropOffBranchName} — {dto.EndTime:dd/MM/yyyy HH:mm}</p>
          <p><strong>Total:</strong> ${dto.TotalCost:N2}</p>
          <p><strong>Reserva:</strong> #{dto.ReservationId.ToString()[..8]}</p>
        </div>";
}

public class ReservationConfirmedFormatter : RentifyFormatterBase
{
    protected override NotificationType HandledType => NotificationType.ReservationConfirmed;
    protected override string Subject => "Rentify — Reserva confirmada";

    protected override string BuildHtml(User user, ReservationNotificationPayload dto) => $@"
        <html><body style='font-family:Arial,sans-serif;color:#333;'>
        <div style='max-width:600px;margin:0 auto;padding:20px;'>
          {RentifyHeader()}
          <p>Hola <strong>{user.FirstName}</strong>,</p>
          <p>✅ Tu reserva fue <strong>confirmada</strong>. ¡Ya podés retirar el vehículo!</p>
          {DetailsBox(dto)}
          {Footer()}
        </div></body></html>";
}

public class PaymentConfirmedFormatter : RentifyFormatterBase
{
    protected override NotificationType HandledType => NotificationType.PaymentConfirmed;
    protected override string Subject => "Rentify — Pago acreditado";

    protected override string BuildHtml(User user, ReservationNotificationPayload dto) => $@"
        <html><body style='font-family:Arial,sans-serif;color:#333;'>
        <div style='max-width:600px;margin:0 auto;padding:20px;'>
          {RentifyHeader()}
          <p>Hola <strong>{user.FirstName}</strong>,</p>
          <p>💳 Recibimos tu pago de <strong>${dto.TotalCost:N2}</strong>.</p>
          {(string.IsNullOrEmpty(dto.TransactionId) ? "" : $"<p><strong>Transacción:</strong> {dto.TransactionId}</p>")}
          {DetailsBox(dto)}
          {Footer()}
        </div></body></html>";
}

public class ReservationCancelledFormatter : RentifyFormatterBase
{
    protected override NotificationType HandledType => NotificationType.ReservationCancelled;
    protected override string Subject => "Rentify — Reserva cancelada";

    protected override string BuildHtml(User user, ReservationNotificationPayload dto) => $@"
        <html><body style='font-family:Arial,sans-serif;color:#333;'>
        <div style='max-width:600px;margin:0 auto;padding:20px;'>
          {RentifyHeader()}
          <p>Hola <strong>{user.FirstName}</strong>,</p>
          <p>Tu reserva fue <strong>cancelada</strong>.</p>
          {DetailsBox(dto)}
          {Footer()}
        </div></body></html>";
}

public class PickupReminderFormatter : RentifyFormatterBase
{
    protected override NotificationType HandledType => NotificationType.PickupReminder;
    protected override string Subject => "Rentify — Recordatorio de retiro";

    protected override string BuildHtml(User user, ReservationNotificationPayload dto) => $@"
        <html><body style='font-family:Arial,sans-serif;color:#333;'>
        <div style='max-width:600px;margin:0 auto;padding:20px;'>
          {RentifyHeader()}
          <p>Hola <strong>{user.FirstName}</strong>,</p>
          <p>⏰ Recordatorio: tu alquiler comienza el <strong>{dto.StartTime:dd/MM/yyyy a las HH:mm}</strong>.</p>
          <p>Presentate en <strong>{dto.PickupBranchName}</strong> con tu DNI y licencia de conducir.</p>
          {DetailsBox(dto)}
          {Footer()}
        </div></body></html>";
}

public class ReturnReminderFormatter : RentifyFormatterBase
{
    protected override NotificationType HandledType => NotificationType.ReturnReminder;
    protected override string Subject => "Rentify — Recordatorio de devolución";

    protected override string BuildHtml(User user, ReservationNotificationPayload dto) => $@"
        <html><body style='font-family:Arial,sans-serif;color:#333;'>
        <div style='max-width:600px;margin:0 auto;padding:20px;'>
          {RentifyHeader()}
          <p>Hola <strong>{user.FirstName}</strong>,</p>
          <p>⏰ Recordatorio: debés devolver el vehículo el <strong>{dto.EndTime:dd/MM/yyyy a las HH:mm}</strong>.</p>
          <p>Sucursal de devolución: <strong>{dto.DropOffBranchName}</strong>.</p>
          {DetailsBox(dto)}
          {Footer()}
        </div></body></html>";
}

public class RentalCompletedFormatter : RentifyFormatterBase
{
    protected override NotificationType HandledType => NotificationType.RentalCompleted;
    protected override string Subject => "Rentify — Alquiler finalizado";

    protected override string BuildHtml(User user, ReservationNotificationPayload dto) => $@"
        <html><body style='font-family:Arial,sans-serif;color:#333;'>
        <div style='max-width:600px;margin:0 auto;padding:20px;'>
          {RentifyHeader()}
          <p>Hola <strong>{user.FirstName}</strong>,</p>
          <p>✅ Registramos la devolución de tu vehículo.</p>
          {(dto.ActualReturnTime.HasValue ? $"<p><strong>Hora real de devolución:</strong> {dto.ActualReturnTime:dd/MM/yyyy HH:mm}</p>" : "")}
          {DetailsBox(dto)}
          <p>¡Gracias por elegir Rentify!</p>
          {Footer()}
        </div></body></html>";
}
