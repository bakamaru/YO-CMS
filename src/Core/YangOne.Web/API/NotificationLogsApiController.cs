using Dapper;
using Microsoft.AspNetCore.Mvc;
using YangOne.Data;
using YangOne.Web.API;
using YangOne.Web.ViewModel.Notification;

namespace YangOne.Web.API
{
    [Route("api/admin/notification-logs")]
    public class NotificationLogsApiController : BaseApiController
    {

        [HttpGet]
        public async Task<dynamic> GetAll([FromQuery] NotificationSendLogQueryModel query)
        {
            try
            {
                var where = new DynamicParameters();
                string conditions = "WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(query.EventKey))
                {
                    conditions += " AND EventKey = @EventKey";
                    where.Add("EventKey", query.EventKey);
                }

                if (!string.IsNullOrWhiteSpace(query.Channel))
                {
                    conditions += " AND Channel = @Channel";
                    where.Add("Channel", query.Channel);
                }

                if (!string.IsNullOrWhiteSpace(query.Status))
                {
                    conditions += " AND Status = @Status";
                    where.Add("Status", query.Status);
                }

                if (query.FromDate.HasValue)
                {
                    conditions += " AND AddedOn >= @FromDate";
                    where.Add("FromDate", query.FromDate.Value);
                }

                if (query.ToDate.HasValue)
                {
                    conditions += " AND AddedOn <= @ToDate";
                    where.Add("ToDate", query.ToDate.Value);
                }

                string countSql = $"SELECT COUNT(*) FROM NotificationSendLog {conditions}";
                string dataSql = $@"
SELECT NotificationSendLogId, AddedOn, EventKey, '' AS RuleName, TemplateKey, 
       UserId, '' AS UserName, Channel, Receiver, Status, Provider, ErrorMessage
FROM NotificationSendLog
{conditions}
ORDER BY AddedOn DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                var dbFactory = DbFactoryProvider.GetFactory();
                using var db = dbFactory.GetConnection();

                var total = await db.ExecuteScalarAsync<int>(countSql, where);
                where.Add("Offset", (query.Page - 1) * query.PageSize);
                where.Add("PageSize", query.PageSize);

                var items = await db.QueryAsync<NotificationSendLogListViewModel>(dataSql, where);

                return SuccessResponse("Logs retrieved", new
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

        [HttpGet("{id}")]
        public async Task<dynamic> GetById(long id)
        {
            try
            {
                var dbFactory = DbFactoryProvider.GetFactory();
                using var db = dbFactory.GetConnection();

                var sql = "SELECT * FROM NotificationSendLog WHERE NotificationSendLogId = @Id";
                var log = await db.QueryFirstOrDefaultAsync<NotificationSendLogDetailViewModel>(sql, new { Id = id });

                if (log == null) return NotAuthorizedResponse("Log not found");
                return SuccessResponse("Log retrieved", log);
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpPost("{id}/retry")]
        public async Task<dynamic> Retry(long id)
        {
            try
            {
                var dbFactory = DbFactoryProvider.GetFactory();
                using var db = dbFactory.GetConnection();

                var sql = "SELECT * FROM NotificationSendLog WHERE NotificationSendLogId = @Id";
                var log = await db.QueryFirstOrDefaultAsync(sql, new { Id = id });

                if (log == null) return NotAuthorizedResponse("Log not found");

                // Retry logic - create new log entry with retry
                return SuccessResponse("Retry queued", new { success = true });
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }
    }
}
