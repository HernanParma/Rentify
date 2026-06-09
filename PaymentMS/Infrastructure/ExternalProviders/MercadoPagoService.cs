using MercadoPago.Config;
using MercadoPago.Client.Preference;
using MercadoPago.Resource.Preference;
using System.Text.Json;
using Infrastructure.HttpClients.Dtos;
using MercadoPago.Client.Payment;
using Microsoft.Extensions.Configuration;

public class MercadoPagoService
{
    private readonly string _accessToken;
    private readonly string _backUrlBase;
    private readonly bool _forceMock;

    public bool UseMockPayments { get; }

    public MercadoPagoService(IConfiguration configuration)
    {
        _accessToken = configuration["MercadoPago:AccessToken"]?.Trim() ?? string.Empty;
        _backUrlBase = configuration["MercadoPago:BackUrlBase"]?.Trim() ?? "http://localhost:5173";
        _forceMock = bool.TryParse(configuration["MercadoPago:UseMockPayments"], out var mock) && mock;
        UseMockPayments = _forceMock || IsPlaceholderToken(_accessToken);
        if (!UseMockPayments)
            MercadoPagoConfig.AccessToken = _accessToken;
    }

    private static bool IsPlaceholderToken(string token) =>
        string.IsNullOrWhiteSpace(token)
        || token.Equals("MOCK", StringComparison.OrdinalIgnoreCase)
        || token.Contains("-111111-", StringComparison.Ordinal);

    public async Task<string> CreatePreferenceAsync(string title, decimal amount, Guid paymentId, decimal lateFee)
    {
        if (UseMockPayments)
            return $"{_backUrlBase}/pago/simulado?localPaymentId={paymentId}";

        try
        {
            var client = new PreferenceClient();

            var referenceData = new PaymentReferenceData
            {
                PaymentId = paymentId,
                LateFee = lateFee
            };

            var externalReference = JsonSerializer.Serialize(referenceData);

            var request = new PreferenceRequest
            {
                Items = new List<PreferenceItemRequest>
                {
                    new PreferenceItemRequest
                    {
                        Id = Guid.NewGuid().ToString(),
                        Title = title,
                        Quantity = 1,
                        CurrencyId = "ARS",
                        UnitPrice = amount,
                    }
                },
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = $"{_backUrlBase}/pago/exito",
                    Failure = $"{_backUrlBase}/pago/fallo",
                    Pending = $"{_backUrlBase}/pago/pendiente"
                },
                AutoReturn = "approved",
                ExternalReference = externalReference
            };

            Preference preference = await client.CreateAsync(request);
            return preference.InitPoint;
        }
        catch (Exception ex) when (ex.Message.Contains("401", StringComparison.Ordinal)
            || ex.Message.Contains("invalid access token", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Token de Mercado Pago inválido. Configurá tu Access Token de prueba en PaymentMS " +
                "(user-secrets: MercadoPago:AccessToken) o activá pagos simulados (MercadoPago:UseMockPayments=true).");
        }
    }

    public async Task<MercadoPagoPaymentInfo> GetPaymentInfoAsync(long paymentId)
    {
        var client = new PaymentClient();
        var payment = await client.GetAsync(paymentId);

        return new MercadoPagoPaymentInfo
        {
            Status = payment.Status,
            TransactionId = payment.Id.ToString(),
            ExternalReference = payment.ExternalReference
        };
    }
}
