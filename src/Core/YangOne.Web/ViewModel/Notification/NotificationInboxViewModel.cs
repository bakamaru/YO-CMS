namespace YangOne.Web.ViewModel.Notification
{
    public class NotificationInboxListViewModel
    {
        public long NotificationInboxId { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; } = "";
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string EventKey { get; set; } = "";
        public string Channel { get; set; } = "";
        public string Severity { get; set; } = "";
        public bool IsRead { get; set; }
        public DateTime AddedOn { get; set; }
    }

    public class NotificationInboxQueryModel
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public long? UserId { get; set; }
        public bool? IsRead { get; set; }
        public string EventKey { get; set; } = "";
    }

    public class MyNotificationViewModel
    {
        public long NotificationInboxId { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string EventKey { get; set; } = "";
        public string Channel { get; set; } = "";
        public string Severity { get; set; } = "";
        public string Url { get; set; } = "";
        public bool IsRead { get; set; }
        public DateTime AddedOn { get; set; }
    }

    public class UnreadCountViewModel
    {
        public int UnreadCount { get; set; }
    }
}
