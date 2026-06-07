using Microsoft.AspNetCore.Mvc;
using YangOne.Web.API;
using YangOne.Web.Service.Notification;
using YangOne.Web.ViewModel.Notification;

namespace YangOne.Web.API
{
    [Route("api/admin/notification-events")]
    public class NotificationEventsApiController : BaseApiController
    {
        private readonly INotificationEventService _eventService;

        public NotificationEventsApiController(INotificationEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet]
        public async Task<dynamic> GetAll([FromQuery] NotificationEventQueryModel query)
        {
            try
            {
                var result = await _eventService.GetListAsync(query);
                return SuccessResponse("Events retrieved", result);
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
                var result = await _eventService.GetByIdAsync(id);
                if (result == null) return NotAuthorizedResponse("Event not found");
                return SuccessResponse("Event retrieved", result);
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpPost]
        public async Task<dynamic> Create([FromBody] NotificationEventFormViewModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                var id = await _eventService.SaveAsync(model, userId);
                return SuccessResponse("Event created", new { id });
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<dynamic> Update(long id, [FromBody] NotificationEventFormViewModel model)
        {
            try
            {
                model.NotificationEventId = id;
                var userId = GetCurrentUserId();
                await _eventService.SaveAsync(model, userId);
                return SuccessResponse("Event updated", null);
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
                await _eventService.DeleteAsync(id, userId);
                return SuccessResponse("Event deleted", null);
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
                bool isActive = body.isActive;
                var userId = GetCurrentUserId();
                await _eventService.ToggleStatusAsync(id, isActive, userId);
                return SuccessResponse("Status updated", null);
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpPost("{id}/test-trigger")]
        public async Task<dynamic> TestTrigger(long id, [FromBody] dynamic body)
        {
            try
            {
                var payload = body.payload;
                long actorUserId = body.actorUserId ?? GetCurrentUserId();
                var result = await _eventService.TestTriggerAsync(id, payload, actorUserId);
                return SuccessResponse("Test trigger executed", result);
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        private long GetCurrentUserId()
        {
            // TODO: Implement proper user ID extraction from claims
            return 1;
        }
    }
}
