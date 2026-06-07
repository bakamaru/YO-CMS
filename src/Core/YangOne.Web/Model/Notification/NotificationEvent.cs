using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YangOne.Data.Crud.Attribute;

namespace YangOne.Web.Model.Notification
{
    [Table("NotificationEvent")]
    public class NotificationEvent
    {
        [Key]
        public long NotificationEventId { get; set; }

        [Required]
        [StringLength(200)]
        public string EventKey { get; set; } = "";

        [Required]
        [StringLength(256)]
        public string Name { get; set; } = "";

        [StringLength(100)]
        public string ModuleName { get; set; } = "";

        public string Description { get; set; } = "";

        public string SamplePayloadJson { get; set; } = "";

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; }

        public DateTime AddedOn { get; set; }

        public long AddedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public long? UpdatedBy { get; set; }
    }
}
