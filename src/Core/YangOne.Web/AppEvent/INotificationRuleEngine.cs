using YangOne.AppEvent.Dto;

namespace YangOne.Web.AppEvent
{
    public interface INotificationRuleEngine
    {
        Task<IReadOnlyList<NotificationJob>> BuildNotificationsAsync(
            OutboxEventDto appEvent,
            CancellationToken cancellationToken = default);
    }
}
