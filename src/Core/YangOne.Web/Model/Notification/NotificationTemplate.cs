using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YangOne.Data.Crud.Attribute;

namespace YangOne.Web.Model.Notification
{
    [Table("NotificationTemplate")]
    public class NotificationTemplate
    {
        [Key]
        public long NotificationTemplateId { get; set; }

        [Required]
        [StringLength(200)]
        public string TemplateKey { get; set; } = "";

        [Required]
        [StringLength(256)]
        public string Name { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string Channel { get; set; } = "";

        [StringLength(10)]
        public string LanguageCode { get; set; } = "en";

        public string SubjectTemplate { get; set; } = "";

        public string BodyTemplate { get; set; } = "";

        public string SamplePayloadJson { get; set; } = "";

        public bool IsDefault { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; }

        public int Version { get; set; } = 1;

        public DateTime AddedOn { get; set; }

        public long AddedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public long? UpdatedBy { get; set; }
    }
}
