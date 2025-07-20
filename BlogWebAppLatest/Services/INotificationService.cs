public interface INotificationService
{
    Task SendNotificationToAllAsync(string title);
    Task SendNotificationToClientAsync(string body, string userId, int notificationId);
    Task SendNotificationToGroupAsync(string body, string groupName);
}