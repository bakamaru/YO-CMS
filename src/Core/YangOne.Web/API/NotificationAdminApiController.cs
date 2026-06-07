using Dapper;
using Microsoft.AspNetCore.Mvc;
using YangOne.Data;
using YangOne.Log;
using YangOne.Web.API;
using YangOne.Web.Service.Notification;
using YangOne.Web.ViewModel.Notification;

namespace YangOne.Web.API
{
    [Route("api/admin/notifications")]
    public class NotificationAdminApiController : BaseApiController
    {
        private readonly INotificationEventService _eventService;
        private readonly INotificationTemplateService _templateService;
        private readonly INotificationRuleService _ruleService;

        public NotificationAdminApiController(
            INotificationEventService eventService,
            INotificationTemplateService templateService,
            INotificationRuleService ruleService,
            ILogger logger)
        {
            _eventService = eventService;
            _templateService = templateService;
            _ruleService = ruleService;
        }

        [HttpGet("dashboard")]
        public async Task<dynamic> GetDashboard()
        {
            try
            {
                var dbFactory = DbFactoryProvider.GetFactory();
                using var db = dbFactory.GetConnection();

                var totalEvents = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM NotificationEvent WHERE IsDeleted = 0");
                var activeRules = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM NotificationRule WHERE IsDeleted = 0 AND IsEnabled = 1");
                var pendingOutbox = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM AppEventOutbox WHERE IsDeleted = 0 AND Status IN ('Pending', 'Failed') AND RetryCount < MaxRetryCount");
                var failedNotifications = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM NotificationSendLog WHERE Status = 'Failed'");
                var emailsSentToday = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM NotificationSendLog WHERE Channel = 'Email' AND CAST(AddedOn AS DATE) = CAST(SYSUTCDATETIME() AS DATE)");
                var inAppSentToday = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM NotificationSendLog WHERE Channel = 'InApp' AND CAST(AddedOn AS DATE) = CAST(SYSUTCDATETIME() AS DATE)");
                var deadLetterCount = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM AppEventOutbox WHERE IsDeleted = 0 AND Status = 'DeadLetter'");

                var recentEvents = await db.QueryAsync<RecentEventViewModel>(@"
SELECT TOP 5 EventKey, Name, 
       (SELECT COUNT(*) FROM AppEventOutbox WHERE EventKey = ne.EventKey) AS TriggerCount,
       (SELECT TOP 1 AddedOn FROM AppEventOutbox WHERE EventKey = ne.EventKey ORDER BY AddedOn DESC) AS LastTriggeredOn
FROM NotificationEvent ne
WHERE IsDeleted = 0
ORDER BY LastTriggeredOn DESC");

                var recentFailed = await db.QueryAsync<RecentFailedSendViewModel>(@"
SELECT TOP 5 EventKey, Channel, Receiver, ErrorMessage, AddedOn
FROM NotificationSendLog
WHERE Status = 'Failed'
ORDER BY AddedOn DESC");

                var topTriggered = await db.QueryAsync<TopTriggeredEventViewModel>(@"
SELECT TOP 5 ne.EventKey, ne.Name, COUNT(o.AppEventOutboxId) AS TriggerCount
FROM NotificationEvent ne
LEFT JOIN AppEventOutbox o ON o.EventKey = ne.EventKey
WHERE ne.IsDeleted = 0
GROUP BY ne.EventKey, ne.Name
ORDER BY TriggerCount DESC");

                return SuccessResponse("Dashboard data retrieved", new
                {
                    totalEvents,
                    activeRules,
                    pendingOutbox,
                    failedNotifications,
                    emailsSentToday,
                    inAppSentToday,
                    deadLetterCount,
                    recentEvents = recentEvents.ToList(),
                    recentFailedSends = recentFailed.ToList(),
                    topTriggeredEvents = topTriggered.ToList()
                });
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }
    }
}
