namespace YangOne.AppEvent
{
    public interface IAppEventPublisher
    {
        Task PublishAsync(
            string eventKey,
            object payload,
            long? actorUserId = null,
            long? tenantId = null,
            string correlationId = null,
            CancellationToken cancellationToken = default);
    }
}
