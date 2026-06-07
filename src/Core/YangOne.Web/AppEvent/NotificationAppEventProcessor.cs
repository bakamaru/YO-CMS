using YangOne.AppEvent;
using YangOne.AppEvent.Dto;

namespace YangOne.Web.AppEvent
{
    public class NotificationAppEventProcessor : IAppEventProcessor
    {
        private readonly INotificationRuleEngine _ruleEngine;
        private readonly INotificationDispatcher _dispatcher;

        public NotificationAppEventProcessor(
            INotificationRuleEngine ruleEngine,
            INotificationDispatcher dispatcher)
        {
            _ruleEngine = ruleEngine;
            _dispatcher = dispatcher;
        }

        public async Task ProcessAsync(
            OutboxEventDto appEvent,
            CancellationToken cancellationToken = default)
        {
            var notificationJobs = await _ruleEngine.BuildNotificationsAsync(
                appEvent,
                cancellationToken);

            foreach (var job in notificationJobs)
            {
                await _dispatcher.DispatchAsync(job, cancellationToken);
            }
        }
    }
}
