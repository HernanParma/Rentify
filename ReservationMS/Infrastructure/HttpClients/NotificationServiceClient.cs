using System.Net.Http.Json;
using Application.Interfaces.HttpClients;
using Microsoft.Extensions.Logging;

namespace Infrastructure.HttpClients;

public class NotificationServiceClient : INotificationServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationServiceClient> _logger;

    public NotificationServiceClient(HttpClient httpClient, ILogger<NotificationServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task EnqueueEventAsync(int userId, string eventType, object payload)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/notifications/events", new
            {
                userId,
                eventType,
                payload
            });
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo encolar notificación {EventType} para usuario {UserId}", eventType, userId);
        }
    }
}
