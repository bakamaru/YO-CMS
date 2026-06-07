using YangOne.AppEvent;
using YangOne.AppEvent.Dto;

namespace YangOne.Web.AppEvent
{
    public class DirectSqlOutboxEventTransport : IExternalEventTransport
    {
        private readonly IAppEventProcessor _processor;

        public DirectSqlOutboxEventTransport(IAppEventProcessor processor)
        {
            _processor = processor;
        }

        public Task PublishAsync(
            OutboxEventDto appEvent,
            CancellationToken cancellationToken = default)
        {
            return _processor.ProcessAsync(appEvent, cancellationToken);
        }
    }
}
