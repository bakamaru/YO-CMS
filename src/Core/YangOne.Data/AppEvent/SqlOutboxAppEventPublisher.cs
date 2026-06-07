using System.Text.Json;
using YangOne.AppEvent;
using YangOne.AppEvent.Dto;

namespace YangOne.Data.AppEvent
{
    public class SqlOutboxAppEventPublisher : IAppEventPublisher
    {
        private readonly IOutboxEventStore _outboxEventStore;

        public SqlOutboxAppEventPublisher(IOutboxEventStore outboxEventStore)
        {
            _outboxEventStore = outboxEventStore;
        }

        public async Task PublishAsync(
            string eventKey,
            object payload,
            long? actorUserId = null,
            long? tenantId = null,
            string correlationId = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(eventKey))
            {
                throw new ArgumentException("Event key is required.", nameof(eventKey));
            }

            var payloadJson = JsonSerializer.Serialize(payload);

            var idempotencyKey = $"{eventKey}:{correlationId}:{actorUserId}:{payloadJson.GetHashCode()}";

            await _outboxEventStore.AddAsync(new OutboxEventCreateDto
            {
                EventKey = eventKey,
                TenantId = tenantId,
                ActorUserId = actorUserId,
                CorrelationId = correlationId,
                IdempotencyKey = idempotencyKey,
                PayloadJson = payloadJson,
                Priority = 100,
                MaxRetryCount = 5
            }, cancellationToken);
        }
    }
}
