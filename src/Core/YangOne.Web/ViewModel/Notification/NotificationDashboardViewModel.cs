namespace YangOne.Web.ViewModel.Notification
{
    public class NotificationDashboardViewModel
    {
        public int TotalEvents { get; set; }
        public int ActiveRules { get; set; }
        public int PendingOutbox { get; set; }
        public int FailedNotifications { get; set; }
        public int EmailsSentToday { get; set; }
        public int InAppSentToday { get; set; }
        public int DeadLetterCount { get; set; }
        public List<RecentEventViewModel> RecentEvents { get; set; } = new();
        public List<RecentFailedSendViewModel> RecentFailedSends { get; set; } = new();
        public List<TopTriggeredEventViewModel> TopTriggeredEvents { get; set; } = new();
    }

    public class RecentEventViewModel
    {
        public string EventKey { get; set; } = "";
        public string Name { get; set; } = "";
        public int TriggerCount { get; set; }
        public DateTime LastTriggeredOn { get; set; }
    }

    public class RecentFailedSendViewModel
    {
        public string EventKey { get; set; } = "";
        public string Channel { get; set; } = "";
        public string Receiver { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
        public DateTime AddedOn { get; set; }
    }

    public class TopTriggeredEventViewModel
    {
        public string EventKey { get; set; } = "";
        public string Name { get; set; } = "";
        public int TriggerCount { get; set; }
    }
}
