using BlogWebApp.Hubs;
using BlogWebApp.Models;
using BlogWebApp.SubscribeTableDependencies;
using Microsoft.AspNetCore.SignalR;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.Enums;
using TableDependency.SqlClient.Base.EventArgs;


namespace BlogWebApp.SubscribeTableDependencies
{
    public class SubscribeNotificationTableDependency : ISubscribeTableDependency
    {
        //private const DmlTriggerType UpdateOf = DmlTriggerType.Insert | DmlTriggerType.Update;
        SqlTableDependency<Notification> tableDependency;
        //NotificationHub notificationHub;
        private readonly IServiceProvider _serviceProvider;

        public SubscribeNotificationTableDependency(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void SubscribeTableDependency(string connectionString)
        {
            tableDependency = new SqlTableDependency<Notification>(connectionString);
            //tableDependency = new SqlTableDependency<Notification>(connectionString, "Notifications",DmlTriggerType.Insert);
            tableDependency.OnChanged += TableDependency_OnChanged;
            tableDependency.OnError += TableDependency_OnError;
            tableDependency.OnStatusChanged += TableDependency_OnStatusChanged;
            try
            {
                tableDependency.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("TableDependency failed to start: " + ex.Message);
            }

         
        }

        private void TableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
        {
            Console.WriteLine($"{nameof(Notification)} SqlTableDependency error: {e.Error.Message}");
        }
        private void TableDependency_OnStatusChanged(object sender, StatusChangedEventArgs e)
        {
           Console.WriteLine($"{nameof(Notification)} SqlTableDependency error: {e.Status}");
        }

        private async void TableDependency_OnChanged(object sender, RecordChangedEventArgs<Notification> e)
        {
            if (e.ChangeType != ChangeType.None)
            {
                var notification = e.Entity;
                using (var scope = _serviceProvider.CreateScope())
                {
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    if (notification.MessageType == "All")
                    {
                        await notificationService.SendNotificationToAllAsync(notification.Title);
                    }
                    else if (notification.MessageType == "Personal")
                    {
                        await notificationService.SendNotificationToClientAsync(notification.Body, notification.ForUserId, notification.Id);
                    }
                    else if (notification.MessageType == "Group")
                    {
                        await notificationService.SendNotificationToGroupAsync(notification.Body, notification.Username);
                    }
                }
            }
        }
    }           
}
