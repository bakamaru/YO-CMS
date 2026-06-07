using YangOne.AppEvent.Dto;

namespace YangOne.AppEvent
{
    public interface IExternalEventTransport
    {
        Task PublishAsync(
            OutboxEventDto appEvent,
            CancellationToken cancellationToken = default);
    }
}
