using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YangOne.Data.Crud.Attribute;

namespace YangOne.Web.Model.Notification
{
    [Table("NotificationRuleChannel")]
    public class NotificationRuleChannel
    {
        [Key]
        public long NotificationRuleChannelId { get; set; }

        public long NotificationRuleId { get; set; }

        [Required]
        [StringLength(50)]
        public string Channel { get; set; } = "";

        public long NotificationTemplateId { get; set; }

        public bool IsRequired { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        public bool IsDeleted { get; set; }

        public DateTime AddedOn { get; set; }

        public long AddedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public long? UpdatedBy { get; set; }
    }
}
