using System.ComponentModel.DataAnnotations;

namespace YangOne.Web.ViewModel.Notification
{
    public class NotificationTemplateListResponse
    {
        public List<NotificationTemplateListViewModel> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class NotificationTemplateListViewModel
    {
        public long NotificationTemplateId { get; set; }
        public string TemplateKey { get; set; } = "";
        public string Name { get; set; } = "";
        public string Channel { get; set; } = "";
        public string LanguageCode { get; set; } = "";
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public int Version { get; set; }
        public DateTime AddedOn { get; set; }
    }

    public class NotificationTemplateFormViewModel
    {
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
    }

    public class NotificationTemplateQueryModel
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string Channel { get; set; } = "";
        public string Keyword { get; set; } = "";
        public bool? IsActive { get; set; }
    }

    public class NotificationTemplatePreviewRequest
    {
        public string SubjectTemplate { get; set; } = "";
        public string BodyTemplate { get; set; } = "";
        public string PayloadJson { get; set; } = "";
    }

    public class NotificationTemplatePreviewResponse
    {
        public string Subject { get; set; } = "";
        public string Body { get; set; } = "";
        public List<string> MissingVariables { get; set; } = new();
        public List<string> UsedVariables { get; set; } = new();
    }

    public class NotificationTemplateValidateRequest
    {
        public string SubjectTemplate { get; set; } = "";
        public string BodyTemplate { get; set; } = "";
        public string AvailableVariablesJson { get; set; } = "";
    }

    public class NotificationTemplateValidateResponse
    {
        public bool IsValid { get; set; }
        public List<string> UsedVariables { get; set; } = new();
        public List<string> MissingVariables { get; set; } = new();
        public List<string> AvailableVariables { get; set; } = new();
    }
}
