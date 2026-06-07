using YangOne.AppEvent.Dto;

namespace YangOne.AppEvent
{
    public interface IOutboxEventStore
    {
        Task<long> AddAsync(
            OutboxEventCreateDto dto,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<OutboxEventDto>> GetPendingAsync(
            int take,
            CancellationToken cancellationToken = default);

        Task MarkProcessingAsync(
            long outboxEventId,
            string lockedBy,
            CancellationToken cancellationToken = default);

        Task MarkCompletedAsync(
            long outboxEventId,
            CancellationToken cancellationToken = default);

        Task MarkFailedAsync(
            long outboxEventId,
            string errorMessage,
            DateTime nextRetryOn,
            CancellationToken cancellationToken = default);
    }
}
