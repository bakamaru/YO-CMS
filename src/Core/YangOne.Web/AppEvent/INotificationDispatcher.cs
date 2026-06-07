namespace YangOne.Web.AppEvent
{
    public interface INotificationDispatcher
    {
        Task DispatchAsync(
            NotificationJob job,
            CancellationToken cancellationToken = default);
    }
}
