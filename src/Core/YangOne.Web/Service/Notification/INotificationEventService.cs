using YangOne.Data;
using YangOne.Data.Crud;
using YangOne.Web.Model.Notification;
using YangOne.Web.ViewModel.Notification;

namespace YangOne.Web.Service.Notification
{
    public interface INotificationEventService
    {
        CrudService<NotificationEvent> NotificationEventCrudService { get; set; }

        Task<NotificationEventListResponse> GetListAsync(NotificationEventQueryModel query);
        Task<NotificationEventFormViewModel> GetByIdAsync(long id);
        Task<long> SaveAsync(NotificationEventFormViewModel model, long userId);
        Task<bool> DeleteAsync(long id, long userId);
        Task<bool> ToggleStatusAsync(long id, bool isActive, long userId);
        Task<NotificationEventFormViewModel> TestTriggerAsync(long id, object payload, long actorUserId);
    }
}
