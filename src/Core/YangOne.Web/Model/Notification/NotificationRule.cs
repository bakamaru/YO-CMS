using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YangOne.Data.Crud.Attribute;

namespace YangOne.Web.Model.Notification
{
    [Table("NotificationRule")]
    public class NotificationRule
    {
        [Key]
        public long NotificationRuleId { get; set; }

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

        public bool IsDeleted { get; set; }

        public DateTime AddedOn { get; set; }

        public long AddedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public long? UpdatedBy { get; set; }
    }
}
