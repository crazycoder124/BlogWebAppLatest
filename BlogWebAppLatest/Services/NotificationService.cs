using BlogWebApp.Hubs;
using BlogWebApp.Models;
using BlogWebApp.ViewModel;
using BlogWebAppLatest.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ApplicationDbContext dbContext;

    public NotificationService(IHubContext<NotificationHub> hubContext, ApplicationDbContext dbContext  )
    {
        _hubContext = hubContext;
        this.dbContext = dbContext;
    }

    public async Task SendNotificationToAllAsync(string message)
    {
        // Extra logic here

        await _hubContext.Clients.All.SendAsync("ReceivedNotification", message);
    }

    public async Task SendNotificationToClientAsync(string body, string userId, int notificationId)
    {
        // Extra logic here
        var hubConnections = dbContext.HubConnections.Where(con => con.UserId == userId).ToList();
        var unreadNotificationCount = dbContext.Notifications.Where(a => !a.IsRead && a.ForUserId == userId).Count();
        var notifications = (from noti in dbContext.Notifications
                             join blog in dbContext.Blogs on noti.BlogId equals blog.Id
                             join user in dbContext.Users on blog.AuthorId.ToString() equals user.Id
                             where blog.AuthorId == Guid.Parse(userId) && !noti.IsRead
                             orderby noti.CreatedAt descending
                             select new NotificationVm
                             {
                                 Id = noti.Id,
                                 Title = noti.Title,
                                 Body = noti.Body,
                                 Username = user.DisplayName,
                                 Url = user.ProfileUrl,
                                 IsRead = noti.IsRead,
                                 BlogId = blog.Id,
                                 NotificationDate = noti.CreatedAt
                             }).ToList();

        if (notifications.Count() > 0)
        {
            notifications[0].TotalNotification = unreadNotificationCount;

        }

        foreach (var hubConnection in hubConnections)
        {
            //await Clients.Client(hubConnection.ConnectionId).SendAsync("ReceivedPersonalNotification", message, userid);
            //await Clients.Client(hubConnection.ConnectionId).SendAsync("ReceivedPersonalNotification", notifications);
            await _hubContext.Clients.Client(hubConnection.ConnectionId).SendAsync("ReceivedPersonalNotification", notifications);
        }
    }

    public async Task SendNotificationToGroupAsync(string message, string groupName)
    {
        var hubConnections = dbContext.HubConnections.Join(dbContext.Users, c => c.Username, o => o.DisplayName, (c, o) => new { c.Username, c.ConnectionId }).ToList();
        foreach (var hubConnection in hubConnections)
        {
            string username = hubConnection.Username;
            //await Clients.Client(hubConnection.ConnectionId).SendAsync("ReceivedPersonalNotification", message, username);
            await _hubContext.Clients.Group(hubConnection.ConnectionId).SendAsync("ReceivedPersonalNotification", message,username);   
            //Call Send Email function here

        }
    }
}