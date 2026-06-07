namespace YangOne.AppEvent.Dto
{
    public class OutboxEventDto
    {
        public long AppEventOutboxId { get; set; }

        public string EventKey { get; set; } = "";
        public string EventName { get; set; }
        public string ModuleName { get; set; }

        public long? TenantId { get; set; }
        public long? ActorUserId { get; set; }

        public string CorrelationId { get; set; }
        public string IdempotencyKey { get; set; }

        public string PayloadJson { get; set; } = "";
        public string HeadersJson { get; set; }

        public string Status { get; set; } = "";
        public int Priority { get; set; }
        public int RetryCount { get; set; }
        public int MaxRetryCount { get; set; }
    }
}
