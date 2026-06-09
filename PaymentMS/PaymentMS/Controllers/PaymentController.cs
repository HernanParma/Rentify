using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.IServices;
using Application.Dtos.Response;
using Domain.Entities;
using Application.Interfaces.IServices.IReservationServices;
using Application.Interfaces.ICommand;
using Infrastructure.HttpClients.Dtos;
using System.Text.Json;

namespace PaymentMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ICreatePaymentService _createPaymentService;
        private readonly IGetPaymentService _getPaymentService;
        private readonly IUpdatePaymentStatusService _updatePaymentService;
        private readonly MercadoPagoService _mercadoPagoService;
        private readonly IPaymentCalculationService _paymentCalculationService;
        private readonly IReservationServiceClient _reservationServiceClient;
        private readonly IPaymentCommand _paymentCommand;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            ICreatePaymentService createPaymentService,
            IGetPaymentService getPaymentService,
            IUpdatePaymentStatusService updatePaymentService,
            MercadoPagoService mercadoPagoService,
            IPaymentCalculationService paymentCalculationService,
            IReservationServiceClient reservationServiceClient,
            IPaymentCommand paymentCommand,
            ILogger<PaymentController> logger)
        {
            _createPaymentService = createPaymentService;
            _getPaymentService = getPaymentService;
            _updatePaymentService = updatePaymentService;
            _mercadoPagoService = mercadoPagoService;
            _paymentCalculationService = paymentCalculationService;
            _reservationServiceClient = reservationServiceClient;
            _paymentCommand = paymentCommand;
            _logger = logger;
        }

        [HttpGet("reservation/{id}")]
        public async Task<IActionResult> GetReservationForPaymentById(Guid id)
        {
            var reservation = await _reservationServiceClient.GetReservationAsync(id);
            if (reservation == null)
                return NotFound();
            return Ok(reservation);
        }

        [HttpPost("from-reservation")]
        public async Task<IActionResult> CreatePaymentFromReservation([FromBody] ReservationSummaryResponse dto)
        {
            try
            {
                var (totalAmount, lateFee) = _paymentCalculationService.CalculateAmount(dto);
                var title = $"Pago de la Reserva del vehículo";
                var paymentId = Guid.NewGuid();
                var checkoutUrl = await _mercadoPagoService.CreatePreferenceAsync(title, totalAmount, paymentId, lateFee);

                var payment = new Payment
                {
                    PaymentId = paymentId,
                    ReservationId = dto.ReservationId,
                    Amount = totalAmount,
                    Date = DateTime.UtcNow,
                    PaymentMethodId = 1,
                    PaymentStatusId = 1
                };

                await _createPaymentService.SavePayment(payment);

                return Ok(new
                {
                    checkoutUrl,
                    paymentId = payment.PaymentId,
                    mock = _mercadoPagoService.UseMockPayments
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear preferencia de pago");
                return BadRequest(new { message = "No se pudo iniciar el pago. Verificá la configuración de Mercado Pago." });
            }
        }

        [HttpPost("mock-complete/{localPaymentId:guid}")]
        public async Task<IActionResult> CompleteMockPayment(Guid localPaymentId)
        {
            if (!_mercadoPagoService.UseMockPayments)
                return BadRequest(new { message = "Los pagos simulados no están activos." });

            try
            {
                var payment = await _getPaymentService.GetPaymentByIdAsync(localPaymentId);
                if (payment == null)
                    return NotFound(new { message = "Pago no encontrado." });

                if (payment.PaymentStatusId == 2)
                    return Ok(new { message = "El pago ya fue confirmado.", reservationId = payment.ReservationId });

                payment.PaymentStatusId = 2;
                var confirmation = new Application.Dtos.Request.PaymentConfirmationRequest
                {
                    TotalAmount = payment.Amount,
                    LateFee = 0,
                    PaymentGateway = "Mock",
                    TransactionId = $"MOCK-{localPaymentId:N}"
                };

                try
                {
                    await _reservationServiceClient.ConfirmPayment(payment.ReservationId, confirmation);
                }
                catch (Exception notifyEx)
                {
                    _logger.LogError(notifyEx, "Error notificando al microservicio de reservas (mock)");
                    return BadRequest(new { message = "Pago registrado pero no se pudo confirmar la reserva. Verificá que ReservationMS esté activo (puerto 5055)." });
                }

                await _paymentCommand.UpdatePaymentAsync(payment);
                return Ok(new
                {
                    paymentId = localPaymentId,
                    reservationId = payment.ReservationId,
                    status = "approved",
                    amount = payment.Amount,
                    mock = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar pago simulado");
                return StatusCode(500, new { message = "Error al confirmar el pago simulado." });
            }
        }

        [HttpPost("verify/{mercadoPagoPaymentId:long}")]
        public async Task<IActionResult> VerifyPayment(long mercadoPagoPaymentId)
        {
            try
            {
                var paymentInfoMP = await _mercadoPagoService.GetPaymentInfoAsync(mercadoPagoPaymentId);
                if (paymentInfoMP == null)
                    return NotFound("No se encontró información del pago en MercadoPago");

                var referenceData = JsonSerializer.Deserialize<PaymentReferenceData>(paymentInfoMP.ExternalReference);
                var paymentId = referenceData.PaymentId;
                var lateFee = referenceData.LateFee;

                var payment = await _getPaymentService.GetPaymentByIdAsync(paymentId);
                if (payment == null)
                    return NotFound("Pago no encontrado en la base de datos local.");

                if (payment.PaymentStatusId == 2 || payment.PaymentStatusId == 3)
                    return Ok("El pago ya fue procesado.");

                if (paymentInfoMP.Status == "approved")
                {
                    payment.PaymentStatusId = 2;
                    var confirmation = new Application.Dtos.Request.PaymentConfirmationRequest
                    {
                        TotalAmount = payment.Amount,
                        LateFee = lateFee,
                        PaymentGateway = "MercadoPago",
                        TransactionId = paymentInfoMP.TransactionId
                    };

                    try
                    {
                        await _reservationServiceClient.ConfirmPayment(payment.ReservationId, confirmation);
                    }
                    catch (Exception notifyEx)
                    {
                        _logger.LogError(notifyEx, "Error notificando al microservicio de reservas");
                    }
                }
                else if (paymentInfoMP.Status == "rejected" || paymentInfoMP.Status == "cancelled")
                {
                    payment.PaymentStatusId = 3;
                }

                await _paymentCommand.UpdatePaymentAsync(payment);
                return Ok(new
                {
                    paymentId,
                    reservationId = payment.ReservationId,
                    status = paymentInfoMP.Status,
                    amount = payment.Amount,
                    lateFee
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar notificación de pago");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("pago-exitoso")]
        public async Task<IActionResult> PagoExitoso([FromQuery(Name = "payment_id")] long paymentId)
        {
            return await VerifyPayment(paymentId);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPaymentById(Guid id)
        {
            var payment = await _getPaymentService.GetPaymentResponseDtoById(id);
            if (payment == null)
                return NotFound();
            return Ok(payment);
        }
    }
}
