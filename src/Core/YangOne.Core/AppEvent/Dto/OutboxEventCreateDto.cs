namespace YangOne.AppEvent.Dto
{
    public class OutboxEventCreateDto
    {
        public string EventKey { get; set; } = "";
        public string EventName { get; set; }
        public string ModuleName { get; set; }

        public long? TenantId { get; set; }
        public long? ActorUserId { get; set; }

        public string CorrelationId { get; set; }
        public string IdempotencyKey { get; set; }

        public string PayloadJson { get; set; } = "";
        public string HeadersJson { get; set; }

        public int Priority { get; set; } = 100;
        public int MaxRetryCount { get; set; } = 5;
    }
}
