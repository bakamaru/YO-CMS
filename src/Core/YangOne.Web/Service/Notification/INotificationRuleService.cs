using YangOne.Data;
using YangOne.Web.Model.Notification;
using YangOne.Web.ViewModel.Notification;

namespace YangOne.Web.Service.Notification
{
    public interface INotificationRuleService
    {
        CrudService<NotificationRule> NotificationRuleCrudService { get; set; }
        CrudService<NotificationRuleRecipient> NotificationRuleRecipientCrudService { get; set; }
        CrudService<NotificationRuleChannel> NotificationRuleChannelCrudService { get; set; }

        Task<NotificationRuleListResponse> GetListAsync(NotificationRuleQueryModel query);
        Task<NotificationRuleFormViewModel> GetByIdAsync(long id);
        Task<long> SaveAsync(NotificationRuleFormViewModel model, long userId);
        Task<bool> DeleteAsync(long id, long userId);
        Task<bool> ToggleStatusAsync(long id, bool isEnabled, long userId);
        Task<long> CloneAsync(long id, long userId);
        Task<ConditionEvaluationResponse> EvaluateConditionAsync(ConditionEvaluationRequest request);
        Task<RecipientPreviewResponse> PreviewRecipientsAsync(RecipientPreviewRequest request);
        Task<NotificationRuleFormViewModel> TestAsync(long id, object payload, long actorUserId);
    }
}
