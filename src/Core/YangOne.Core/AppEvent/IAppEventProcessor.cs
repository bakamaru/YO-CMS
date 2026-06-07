using YangOne.AppEvent.Dto;

namespace YangOne.AppEvent
{
    public interface IAppEventProcessor
    {
        Task ProcessAsync(
            OutboxEventDto appEvent,
            CancellationToken cancellationToken = default);
    }
}
