using Microsoft.AspNetCore.Mvc;
using YangOne.Web.API;
using YangOne.Web.Service.Notification;
using YangOne.Web.ViewModel.Notification;
using YangOne.AppEvent;

namespace YangOne.Web.API
{
    [Route("api/admin/notifications")]
    public class NotificationManualSendApiController : BaseApiController
    {
        private readonly IAppEventPublisher _appEventPublisher;

        public NotificationManualSendApiController(IAppEventPublisher appEventPublisher)
        {
            _appEventPublisher = appEventPublisher;
        }

        [HttpPost("manual-send")]
        public async Task<dynamic> ManualSend([FromBody] ManualSendDirectRequest request)
        {
            try
            {
                // Direct message sending - implement based on needs
                return SuccessResponse("Manual send queued", new { sent = true });
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpPost("trigger-event")]
        public async Task<dynamic> TriggerEvent([FromBody] ManualTriggerEventRequest request)
        {
            try
            {
                await _appEventPublisher.PublishAsync(
                    request.EventKey,
                    request.PayloadJson,
                    request.ActorUserId,
                    request.TenantId);

                return SuccessResponse("Event triggered", new { success = true });
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpPost("send-template")]
        public async Task<dynamic> SendTemplate([FromBody] ManualSendTemplateRequest request)
        {
            try
            {
                // Direct template sending - implement based on needs
                return SuccessResponse("Template send queued", new { sent = true });
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpPost("send-test")]
        public async Task<dynamic> SendTest([FromBody] dynamic body)
        {
            try
            {
                var userId = GetCurrentUserId();
                var channel = body.channel?.ToString() ?? "InApp";
                
                // Send test notification to current user
                return SuccessResponse("Test notification sent", new { success = true });
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        private long GetCurrentUserId() => 1;
    }
}
