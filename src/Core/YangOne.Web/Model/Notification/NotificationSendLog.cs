using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YangOne.Data.Crud.Attribute;

namespace YangOne.Web.Model.Notification
{
    [Table("NotificationSendLog")]
    public class NotificationSendLog
    {
        [Key]
        public long NotificationSendLogId { get; set; }

        public long? NotificationEventId { get; set; }

        public long? NotificationRuleId { get; set; }

        public long? NotificationTemplateId { get; set; }

        public long UserId { get; set; }

        [StringLength(200)]
        public string EventKey { get; set; } = "";

        [StringLength(50)]
        public string Channel { get; set; } = "";

        [StringLength(256)]
        public string TemplateKey { get; set; } = "";

        [StringLength(100)]
        public string Provider { get; set; } = "";

        [StringLength(50)]
        public string Status { get; set; } = "";

        public string Receiver { get; set; } = "";

        public string Subject { get; set; } = "";

        public string Message { get; set; } = "";

        public string ErrorMessage { get; set; } = "";

        public string PayloadJson { get; set; } = "";

        public string RenderedSubject { get; set; } = "";

        public string RenderedBody { get; set; } = "";

        public int RetryCount { get; set; } = 0;

        public DateTime? SentOn { get; set; }

        public DateTime AddedOn { get; set; }

        public long AddedBy { get; set; }
    }
}
