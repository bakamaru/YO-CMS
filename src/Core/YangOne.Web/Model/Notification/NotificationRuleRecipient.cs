using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YangOne.Data.Crud.Attribute;

namespace YangOne.Web.Model.Notification
{
    [Table("NotificationRuleRecipient")]
    public class NotificationRuleRecipient
    {
        [Key]
        public long NotificationRuleRecipientId { get; set; }

        public long NotificationRuleId { get; set; }

        [Required]
        [StringLength(50)]
        public string RecipientType { get; set; } = "";

        public string RecipientValue { get; set; } = "";

        public int SortOrder { get; set; } = 0;

        public bool IsDeleted { get; set; }

        public DateTime AddedOn { get; set; }

        public long AddedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public long? UpdatedBy { get; set; }
    }
}
