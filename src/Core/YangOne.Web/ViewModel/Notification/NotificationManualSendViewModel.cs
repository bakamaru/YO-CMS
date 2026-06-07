using System.ComponentModel.DataAnnotations;

namespace YangOne.Web.ViewModel.Notification
{
    public class ManualSendDirectRequest
    {
        public List<string> Channels { get; set; } = new();
        public string RecipientType { get; set; } = "";
        public string RecipientValue { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Message { get; set; } = "";
        public string Url { get; set; } = "";
        public string Severity { get; set; } = "Info";
        public bool SendNow { get; set; } = true;
        public DateTime? ScheduledOn { get; set; }
    }

    public class ManualTriggerEventRequest
    {
        [Required]
        public string EventKey { get; set; } = "";

        public long ActorUserId { get; set; }

        public long? TenantId { get; set; }

        public string PayloadJson { get; set; } = "";
    }

    public class ManualSendTemplateRequest
    {
        public long NotificationTemplateId { get; set; }
        public List<string> Channels { get; set; } = new();
        public List<ManualSendRecipientViewModel> Recipients { get; set; } = new();
        public string PayloadJson { get; set; } = "";
    }

    public class ManualSendRecipientViewModel
    {
        public string RecipientType { get; set; } = "";
        public string RecipientValue { get; set; } = "";
    }
}
