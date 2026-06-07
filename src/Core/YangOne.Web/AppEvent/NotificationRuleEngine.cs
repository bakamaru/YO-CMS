using YangOne.AppEvent.Dto;
using YangOne.Web.Service.Notification;

namespace YangOne.Web.AppEvent
{
    public class NotificationRuleEngine : INotificationRuleEngine
    {
        private readonly INotificationRuleService _ruleService;

        public NotificationRuleEngine(INotificationRuleService ruleService)
        {
            _ruleService = ruleService;
        }

        public async Task<IReadOnlyList<NotificationJob>> BuildNotificationsAsync(
            OutboxEventDto appEvent,
            CancellationToken cancellationToken = default)
        {
            // Placeholder - returns empty list
            // In production, this would:
            // 1. Find rules matching the event key
            // 2. Evaluate conditions
            // 3. Resolve recipients
            // 4. Build notification jobs for each channel/template combo
            return Array.Empty<NotificationJob>();
        }
    }
}
