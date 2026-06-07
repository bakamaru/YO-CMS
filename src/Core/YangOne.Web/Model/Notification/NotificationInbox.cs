using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YangOne.Data.Crud.Attribute;

namespace YangOne.Web.Model.Notification
{
    [Table("NotificationInbox")]
    public class NotificationInbox
    {
        [Key]
        public long NotificationInboxId { get; set; }

        public long UserId { get; set; }

        public long? NotificationEventId { get; set; }

        public long? NotificationRuleId { get; set; }

        public long? NotificationTemplateId { get; set; }

        [StringLength(200)]
        public string EventKey { get; set; } = "";

        [StringLength(50)]
        public string Channel { get; set; } = "";

        [StringLength(256)]
        public string Title { get; set; } = "";

        public string Message { get; set; } = "";

        [StringLength(50)]
        public string Severity { get; set; } = "Info";

        public string Url { get; set; } = "";

        public string PayloadJson { get; set; } = "";

        public bool IsRead { get; set; } = false;

        public DateTime? ReadOn { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime AddedOn { get; set; }

        public long AddedBy { get; set; }
    }
}
