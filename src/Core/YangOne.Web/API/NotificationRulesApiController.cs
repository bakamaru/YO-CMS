using Microsoft.AspNetCore.Mvc;
using YangOne.Web.API;
using YangOne.Web.Service.Notification;
using YangOne.Web.ViewModel.Notification;

namespace YangOne.Web.API
{
    [Route("api/admin/notification-rules")]
    public class NotificationRulesApiController : BaseApiController
    {
        private readonly INotificationRuleService _ruleService;

        public NotificationRulesApiController(INotificationRuleService ruleService)
        {
            _ruleService = ruleService;
        }

        [HttpGet]
        public async Task<dynamic> GetAll([FromQuery] NotificationRuleQueryModel query)
        {
            try
            {
                var result = await _ruleService.GetListAsync(query);
                return SuccessResponse("Rules retrieved", result);
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<dynamic> GetById(long id)
        {
            try
            {
                var result = await _ruleService.GetByIdAsync(id);
                if (result == null) return NotAuthorizedResponse("Rule not found");
                return SuccessResponse("Rule retrieved", result);
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpPost]
        public async Task<dynamic> Create([FromBody] NotificationRuleFormViewModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                var id = await _ruleService.SaveAsync(model, userId);
                return SuccessResponse("Rule created", new { id });
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<dynamic> Update(long id, [FromBody] NotificationRuleFormViewModel model)
        {
            try
            {
                model.NotificationRuleId = id;
                var userId = GetCurrentUserId();
                await _ruleService.SaveAsync(model, userId);
                return SuccessResponse("Rule updated", null);
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpDelete("{id}")]
        public async Task<dynamic> Delete(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _ruleService.DeleteAsync(id, userId);
                return SuccessResponse("Rule deleted", null);
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<dynamic> ToggleStatus(long id, [FromBody] dynamic body)
        {
            try
            {
                bool isEnabled = body.isEnabled;
                var userId = GetCurrentUserId();
                await _ruleService.ToggleStatusAsync(id, isEnabled, userId);
                return SuccessResponse("Status updated", null);
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpPost("{id}/clone")]
        public async Task<dynamic> Clone(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var newId = await _ruleService.CloneAsync(id, userId);
                return SuccessResponse("Rule cloned", new { id = newId });
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpPost("evaluate-condition")]
        public async Task<dynamic> EvaluateCondition([FromBody] ConditionEvaluationRequest request)
        {
            try
            {
                var result = await _ruleService.EvaluateConditionAsync(request);
                return SuccessResponse("Condition evaluated", result);
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpPost("preview-recipients")]
        public async Task<dynamic> PreviewRecipients([FromBody] RecipientPreviewRequest request)
        {
            try
            {
                var result = await _ruleService.PreviewRecipientsAsync(request);
                return SuccessResponse("Recipients previewed", result);
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpPost("{id}/test")]
        public async Task<dynamic> Test(long id, [FromBody] dynamic body)
        {
            try
            {
                var payload = body.payload;
                long actorUserId = body.actorUserId ?? GetCurrentUserId();
                var result = await _ruleService.TestAsync(id, payload, actorUserId);
                return SuccessResponse("Test executed", result);
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        private long GetCurrentUserId() => 1;
    }
}
