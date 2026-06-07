using System.ComponentModel.DataAnnotations;

namespace YangOne.Web.ViewModel.Notification
{
    public class NotificationPreferenceViewModel
    {
        public long NotificationPreferenceId { get; set; }
        public long UserId { get; set; }
        public bool EmailEnabled { get; set; } = true;
        public bool SmsEnabled { get; set; } = true;
        public bool PushEnabled { get; set; } = true;
        public bool InAppEnabled { get; set; } = true;
        public bool QuietHoursEnabled { get; set; } = false;
        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }
        public List<string> MutedCategories { get; set; } = new();
    }

    public class NotificationPreferenceFormViewModel
    {
        public long NotificationPreferenceId { get; set; }
        public long UserId { get; set; }

        public bool EmailEnabled { get; set; } = true;
        public bool SmsEnabled { get; set; } = true;
        public bool PushEnabled { get; set; } = true;
        public bool InAppEnabled { get; set; } = true;
        public bool QuietHoursEnabled { get; set; } = false;
        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }
        public List<string> MutedCategories { get; set; } = new();
    }
}
