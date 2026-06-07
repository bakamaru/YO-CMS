using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YangOne.Data.Crud.Attribute;

namespace YangOne.Web.Model.Notification
{
    [Table("NotificationPreference")]
    public class NotificationPreference
    {
        [Key]
        public long NotificationPreferenceId { get; set; }

        public long UserId { get; set; }

        public bool EmailEnabled { get; set; } = true;

        public bool SmsEnabled { get; set; } = true;

        public bool PushEnabled { get; set; } = true;

        public bool InAppEnabled { get; set; } = true;

        public bool QuietHoursEnabled { get; set; } = false;

        public TimeSpan? QuietHoursStart { get; set; }

        public TimeSpan? QuietHoursEnd { get; set; }

        public string MutedCategoriesJson { get; set; } = "";

        public bool IsDeleted { get; set; }

        public DateTime AddedOn { get; set; }

        public long AddedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public long? UpdatedBy { get; set; }
    }
}
