namespace YangOne.Web.AppEvent
{
    public class NotificationJob
    {
        public string Channel { get; set; } = "";
        public string TemplateKey { get; set; } = "";
        public object Data { get; set; }
        public string Recipient { get; set; } = "";
    }
}
