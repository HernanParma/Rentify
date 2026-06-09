namespace Application.Interfaces.HttpClients;

public interface INotificationServiceClient
{
    Task EnqueueEventAsync(int userId, string eventType, object payload);
}
