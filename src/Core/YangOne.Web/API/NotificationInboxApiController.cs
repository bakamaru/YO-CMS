using Dapper;
using Microsoft.AspNetCore.Mvc;
using YangOne.Data;
using YangOne.Web.API;
using YangOne.Web.ViewModel.Notification;

namespace YangOne.Web.API
{
    [Route("api/admin/notification-inbox")]
    public class NotificationInboxApiController : BaseApiController
    {
        [HttpGet]
        public async Task<dynamic> GetAll([FromQuery] NotificationInboxQueryModel query)
        {
            try
            {
                var where = new DynamicParameters();
                string conditions = "WHERE IsDeleted = 0";

                if (query.UserId.HasValue)
                {
                    conditions += " AND UserId = @UserId";
                    where.Add("UserId", query.UserId.Value);
                }

                if (query.IsRead.HasValue)
                {
                    conditions += " AND IsRead = @IsRead";
                    where.Add("IsRead", query.IsRead.Value);
                }

                if (!string.IsNullOrWhiteSpace(query.EventKey))
                {
                    conditions += " AND EventKey = @EventKey";
                    where.Add("EventKey", query.EventKey);
                }

                string countSql = $"SELECT COUNT(*) FROM NotificationInbox {conditions}";
                string dataSql = $@"
SELECT NotificationInboxId, UserId, '' AS UserName, Title, Message, EventKey, Channel, Severity, IsRead, AddedOn
FROM NotificationInbox
{conditions}
ORDER BY AddedOn DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                var dbFactory = DbFactoryProvider.GetFactory();
                using var db = dbFactory.GetConnection();

                var total = await db.ExecuteScalarAsync<int>(countSql, where);
                where.Add("Offset", (query.Page - 1) * query.PageSize);
                where.Add("PageSize", query.PageSize);

                var items = await db.QueryAsync<NotificationInboxListViewModel>(dataSql, where);

                return SuccessResponse("Inbox retrieved", new
                {
                    Items = items.ToList(),
                    TotalCount = total,
                    Page = query.Page,
                    PageSize = query.PageSize
                });
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }
    }

    [Route("api/notifications")]
    public class NotificationUserApiController : BaseApiController
    {
        [HttpGet("my")]
        public async Task<dynamic> GetMy([FromQuery] int page = 1, int pageSize = 20)
        {
            try
            {
                var userId = GetCurrentUserId();

                var dbFactory = DbFactoryProvider.GetFactory();
                using var db = dbFactory.GetConnection();

                string countSql = "SELECT COUNT(*) FROM NotificationInbox WHERE UserId = @UserId AND IsDeleted = 0";
                string dataSql = @"
SELECT NotificationInboxId, Title, Message, EventKey, Channel, Severity, Url, IsRead, AddedOn
FROM NotificationInbox
WHERE UserId = @UserId AND IsDeleted = 0
ORDER BY AddedOn DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                var total = await db.ExecuteScalarAsync<int>(countSql, new { UserId = userId });
                var items = await db.QueryAsync<MyNotificationViewModel>(dataSql, new { UserId = userId, Offset = (page - 1) * pageSize, PageSize = pageSize });

                return SuccessResponse("My notifications retrieved", new
                {
                    Items = items.ToList(),
                    TotalCount = total,
                    Page = page,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpGet("my/unread-count")]
        public async Task<dynamic> GetUnreadCount()
        {
            try
            {
                var userId = GetCurrentUserId();

                var dbFactory = DbFactoryProvider.GetFactory();
                using var db = dbFactory.GetConnection();

                var count = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM NotificationInbox WHERE UserId = @UserId AND IsDeleted = 0 AND IsRead = 0", new { UserId = userId });

                return SuccessResponse("Unread count retrieved", new { UnreadCount = count });
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpPatch("{id}/read")]
        public async Task<dynamic> MarkRead(long id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var dbFactory = DbFactoryProvider.GetFactory();
                using var db = dbFactory.GetConnection();

                await db.ExecuteAsync("UPDATE NotificationInbox SET IsRead = 1, ReadOn = SYSUTCDATETIME() WHERE NotificationInboxId = @Id AND UserId = @UserId", new { Id = id, UserId = userId });

                return SuccessResponse("Marked as read", null);
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpPatch("read-all")]
        public async Task<dynamic> MarkAllRead()
        {
            try
            {
                var userId = GetCurrentUserId();

                var dbFactory = DbFactoryProvider.GetFactory();
                using var db = dbFactory.GetConnection();

                await db.ExecuteAsync("UPDATE NotificationInbox SET IsRead = 1, ReadOn = SYSUTCDATETIME() WHERE UserId = @UserId AND IsDeleted = 0 AND IsRead = 0", new { UserId = userId });

                return SuccessResponse("All marked as read", null);
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

                var dbFactory = DbFactoryProvider.GetFactory();
                using var db = dbFactory.GetConnection();

                await db.ExecuteAsync("UPDATE NotificationInbox SET IsDeleted = 1 WHERE NotificationInboxId = @Id AND UserId = @UserId", new { Id = id, UserId = userId });

                return SuccessResponse("Notification deleted", null);
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        private long GetCurrentUserId() => 1;
    }
}
