namespace YangOne.Web.ViewModel.Notification
{
    public class NotificationSendLogListViewModel
    {
        public long NotificationSendLogId { get; set; }
        public DateTime AddedOn { get; set; }
        public string EventKey { get; set; } = "";
        public string RuleName { get; set; } = "";
        public string TemplateKey { get; set; } = "";
        public long UserId { get; set; }
        public string UserName { get; set; } = "";
        public string Channel { get; set; } = "";
        public string Receiver { get; set; } = "";
        public string Status { get; set; } = "";
        public string Provider { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
    }

    public class NotificationSendLogQueryModel
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string EventKey { get; set; } = "";
        public string Channel { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class NotificationSendLogDetailViewModel
    {
        public long NotificationSendLogId { get; set; }
        public DateTime AddedOn { get; set; }
        public string EventKey { get; set; } = "";
        public string RuleName { get; set; } = "";
        public string TemplateKey { get; set; } = "";
        public long UserId { get; set; }
        public string UserName { get; set; } = "";
        public string Channel { get; set; } = "";
        public string Receiver { get; set; } = "";
        public string Status { get; set; } = "";
        public string Provider { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
        public string PayloadJson { get; set; } = "";
        public string RenderedSubject { get; set; } = "";
        public string RenderedBody { get; set; } = "";
        public int RetryCount { get; set; }
        public DateTime? SentOn { get; set; }
    }
}
