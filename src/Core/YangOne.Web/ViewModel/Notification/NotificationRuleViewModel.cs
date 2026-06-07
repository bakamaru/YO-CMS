using System.ComponentModel.DataAnnotations;

namespace YangOne.Web.ViewModel.Notification
{
    public class NotificationRuleListResponse
    {
        public List<NotificationRuleListViewModel> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class NotificationRuleListViewModel
    {
        public long NotificationRuleId { get; set; }
        public string Name { get; set; } = "";
        public string EventKey { get; set; } = "";
        public int Priority { get; set; }
        public string Channels { get; set; } = "";
        public string Recipients { get; set; } = "";
        public bool IsEnabled { get; set; }
        public DateTime AddedOn { get; set; }
    }

    public class NotificationRuleFormViewModel
    {
        public long NotificationRuleId { get; set; }

        [Required]
        public long NotificationEventId { get; set; }

        [Required]
        [StringLength(256)]
        public string Name { get; set; } = "";

        public int Priority { get; set; } = 100;

        public bool IsEnabled { get; set; } = true;

        public string ConditionJson { get; set; } = "";

        public int DelaySeconds { get; set; } = 0;

        public int MaxSendPerUserPerDay { get; set; } = 5;

        public bool StopProcessingAfterMatch { get; set; } = false;

        public bool QuietHoursEnabled { get; set; } = false;

        public TimeSpan? QuietHoursStart { get; set; }

        public TimeSpan? QuietHoursEnd { get; set; }

        public List<NotificationRuleRecipientViewModel> Recipients { get; set; } = new();

        public List<NotificationRuleChannelViewModel> Channels { get; set; } = new();
    }

    public class NotificationRuleRecipientViewModel
    {
        public long NotificationRuleRecipientId { get; set; }
        public string RecipientType { get; set; } = "";
        public string RecipientValue { get; set; } = "";
        public int SortOrder { get; set; }
    }

    public class NotificationRuleChannelViewModel
    {
        public long NotificationRuleChannelId { get; set; }
        public string Channel { get; set; } = "";
        public long NotificationTemplateId { get; set; }
        public bool IsRequired { get; set; } = true;
        public int SortOrder { get; set; }
    }

    public class NotificationRuleQueryModel
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string EventKey { get; set; } = "";
        public bool? IsEnabled { get; set; }
    }

    public class ConditionEvaluationRequest
    {
        public string ConditionJson { get; set; } = "";
        public string PayloadJson { get; set; } = "";
    }

    public class ConditionEvaluationResponse
    {
        public bool Matched { get; set; }
    }

    public class RecipientPreviewRequest
    {
        public string EventKey { get; set; } = "";
        public long ActorUserId { get; set; }
        public long? TenantId { get; set; }
        public List<NotificationRuleRecipientViewModel> Recipients { get; set; } = new();
    }

    public class RecipientPreviewResponse
    {
        public int TotalRecipients { get; set; }
        public List<RecipientUserViewModel> Recipients { get; set; } = new();
    }

    public class RecipientUserViewModel
    {
        public long UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
    }
}
