using YangOne.Web.AppEvent;

namespace YangOne.Web.AppEvent
{
    public class NotificationDispatcher : INotificationDispatcher
    {
        public async Task DispatchAsync(
            NotificationJob job,
            CancellationToken cancellationToken = default)
        {
            // Placeholder - in production this would:
            // 1. Render template with payload
            // 2. Send via appropriate channel (Email, SMS, Push, InApp, WhatsApp)
            // 3. Log send attempt
            // 4. Handle retries
            await Task.CompletedTask;
        }
    }
}
