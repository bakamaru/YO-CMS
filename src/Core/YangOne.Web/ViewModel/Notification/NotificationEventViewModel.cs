using System.ComponentModel.DataAnnotations;

namespace YangOne.Web.ViewModel.Notification
{
    public class NotificationEventListResponse
    {
        public List<NotificationEventListViewModel> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class NotificationEventListViewModel
    {
        public long NotificationEventId { get; set; }
        public string EventKey { get; set; } = "";
        public string Name { get; set; } = "";
        public string ModuleName { get; set; } = "";
        public bool IsActive { get; set; }
        public int RulesCount { get; set; }
        public DateTime? LastTriggeredOn { get; set; }
        public DateTime AddedOn { get; set; }
    }

    public class NotificationEventFormViewModel
    {
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
    }

    public class NotificationEventQueryModel
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string Keyword { get; set; } = "";
        public string ModuleName { get; set; } = "";
        public bool? IsActive { get; set; }
    }
}
