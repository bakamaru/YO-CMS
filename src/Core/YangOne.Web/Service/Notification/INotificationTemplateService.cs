using YangOne.Data;
using YangOne.Data.Crud;
using YangOne.Web.Model.Notification;
using YangOne.Web.ViewModel.Notification;

namespace YangOne.Web.Service.Notification
{
    public interface INotificationTemplateService
    {
        CrudService<NotificationTemplate> NotificationTemplateCrudService { get; set; }

        Task<NotificationTemplateListResponse> GetListAsync(NotificationTemplateQueryModel query);
        Task<NotificationTemplateFormViewModel> GetByIdAsync(long id);
        Task<long> SaveAsync(NotificationTemplateFormViewModel model, long userId);
        Task<bool> DeleteAsync(long id, long userId);
        Task<NotificationTemplatePreviewResponse> PreviewAsync(NotificationTemplatePreviewRequest request);
        Task<NotificationTemplateValidateResponse> ValidateAsync(NotificationTemplateValidateRequest request);
        Task<long> CloneAsync(long id, long userId);
        Task<NotificationTemplateFormViewModel> TestSendAsync(long id, object payload, long userId);
        Task<List<NotificationTemplateListViewModel>> GetByChannelAsync(string channel);
    }
}
