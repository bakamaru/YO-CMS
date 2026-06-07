namespace YangOne.Web.ViewModel.Notification
{
    public class AppEventOutboxListViewModel
    {
        public long AppEventOutboxId { get; set; }
        public string EventKey { get; set; } = "";
        public string EventName { get; set; } = "";
        public string ModuleName { get; set; } = "";
        public string Status { get; set; } = "";
        public int Priority { get; set; }
        public int RetryCount { get; set; }
        public int MaxRetryCount { get; set; }
        public DateTime? NextRetryOn { get; set; }
        public DateTime AddedOn { get; set; }
        public string LockedBy { get; set; } = "";
    }

    public class AppEventOutboxQueryModel
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string EventKey { get; set; } = "";
        public string Status { get; set; } = "";
    }

    public class AppEventOutboxDetailViewModel
    {
        public long AppEventOutboxId { get; set; }
        public string EventKey { get; set; } = "";
        public string EventName { get; set; } = "";
        public string ModuleName { get; set; } = "";
        public long? TenantId { get; set; }
        public long? ActorUserId { get; set; }
        public string CorrelationId { get; set; } = "";
        public string IdempotencyKey { get; set; } = "";
        public string PayloadJson { get; set; } = "";
        public string HeadersJson { get; set; } = "";
        public string Status { get; set; } = "";
        public int Priority { get; set; }
        public int RetryCount { get; set; }
        public int MaxRetryCount { get; set; }
        public DateTime? NextRetryOn { get; set; }
        public string LockedBy { get; set; } = "";
        public DateTime? LockedOn { get; set; }
        public DateTime? ProcessedOn { get; set; }
        public string ErrorMessage { get; set; } = "";
        public DateTime AddedOn { get; set; }
    }
}
