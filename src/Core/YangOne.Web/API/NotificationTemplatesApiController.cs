using Microsoft.AspNetCore.Mvc;
using YangOne.Web.API;
using YangOne.Web.Service.Notification;
using YangOne.Web.ViewModel.Notification;

namespace YangOne.Web.API
{
    [Route("api/admin/notification-templates")]
    public class NotificationTemplatesApiController : BaseApiController
    {
        private readonly INotificationTemplateService _templateService;

        public NotificationTemplatesApiController(INotificationTemplateService templateService)
        {
            _templateService = templateService;
        }

        [HttpGet]
        public async Task<dynamic> GetAll([FromQuery] NotificationTemplateQueryModel query)
        {
            try
            {
                var result = await _templateService.GetListAsync(query);
                return SuccessResponse("Templates retrieved", result);
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpGet("by-channel/{channel}")]
        public async Task<dynamic> GetByChannel(string channel)
        {
            try
            {
                var result = await _templateService.GetByChannelAsync(channel);
                return SuccessResponse("Templates retrieved", result);
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
                var result = await _templateService.GetByIdAsync(id);
                if (result == null) return NotAuthorizedResponse("Template not found");
                return SuccessResponse("Template retrieved", result);
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpPost]
        public async Task<dynamic> Create([FromBody] NotificationTemplateFormViewModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                var id = await _templateService.SaveAsync(model, userId);
                return SuccessResponse("Template created", new { id });
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<dynamic> Update(long id, [FromBody] NotificationTemplateFormViewModel model)
        {
            try
            {
                model.NotificationTemplateId = id;
                var userId = GetCurrentUserId();
                await _templateService.SaveAsync(model, userId);
                return SuccessResponse("Template updated", null);
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
                await _templateService.DeleteAsync(id, userId);
                return SuccessResponse("Template deleted", null);
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpPost("preview")]
        public async Task<dynamic> Preview([FromBody] NotificationTemplatePreviewRequest request)
        {
            try
            {
                var result = await _templateService.PreviewAsync(request);
                return SuccessResponse("Preview generated", result);
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpPost("validate")]
        public async Task<dynamic> Validate([FromBody] NotificationTemplateValidateRequest request)
        {
            try
            {
                var result = await _templateService.ValidateAsync(request);
                return SuccessResponse("Validation complete", result);
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
                var newId = await _templateService.CloneAsync(id, userId);
                return SuccessResponse("Template cloned", new { id = newId });
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpPost("{id}/test-send")]
        public async Task<dynamic> TestSend(long id, [FromBody] dynamic body)
        {
            try
            {
                var payload = body.payload;
                var userId = GetCurrentUserId();
                var result = await _templateService.TestSendAsync(id, payload, userId);
                return SuccessResponse("Test send executed", result);
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        private long GetCurrentUserId() => 1;
    }
}
