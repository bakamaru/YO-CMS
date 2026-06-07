using Dapper;
using YangOne.Data;
using YangOne.Data.Crud;
using YangOne.Log;
using YangOne.Web.Model.Notification;
using YangOne.Web.ViewModel.Notification;

namespace YangOne.Web.Service.Notification
{
    public class NotificationEventService : INotificationEventService
    {
        private readonly ILogger _logger;
        public CrudService<NotificationEvent> NotificationEventCrudService { get; set; } = new CrudService<NotificationEvent>();

        public NotificationEventService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<NotificationEventListResponse> GetListAsync(NotificationEventQueryModel query)
        {
            var where = new DynamicParameters();
            string conditions = "WHERE IsDeleted = 0";

            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                conditions += " AND (EventKey LIKE @Keyword OR Name LIKE @Keyword OR ModuleName LIKE @Keyword)";
                where.Add("Keyword", $"%{query.Keyword}%");
            }

            if (!string.IsNullOrWhiteSpace(query.ModuleName))
            {
                conditions += " AND ModuleName = @ModuleName";
                where.Add("ModuleName", query.ModuleName);
            }

            if (query.IsActive.HasValue)
            {
                conditions += " AND IsActive = @IsActive";
                where.Add("IsActive", query.IsActive.Value);
            }

            string countSql = $"SELECT COUNT(*) FROM NotificationEvent {conditions}";
            string dataSql = $@"
SELECT NotificationEventId, EventKey, Name, ModuleName, IsActive, 
       (SELECT COUNT(*) FROM NotificationRule WHERE NotificationEventId = ne.NotificationEventId AND IsDeleted = 0) AS RulesCount,
       (SELECT TOP 1 AddedOn FROM AppEventOutbox WHERE EventKey = ne.EventKey ORDER BY AddedOn DESC) AS LastTriggeredOn,
       AddedOn
FROM NotificationEvent ne
{conditions}
ORDER BY AddedOn DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var dbFactory = DbFactoryProvider.GetFactory();
            using var db = dbFactory.GetConnection();

            var total = await db.ExecuteScalarAsync<int>(countSql, where);
            where.Add("Offset", (query.Page - 1) * query.PageSize);
            where.Add("PageSize", query.PageSize);

            var items = await db.QueryAsync<NotificationEventListViewModel>(dataSql, where);

            return new NotificationEventListResponse
            {
                Items = items.ToList(),
                TotalCount = total,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }

        public async Task<NotificationEventFormViewModel> GetByIdAsync(long id)
        {
            var entity = NotificationEventCrudService.Get(id);
            if (entity == null) return null;

            return new NotificationEventFormViewModel
            {
                NotificationEventId = entity.NotificationEventId,
                EventKey = entity.EventKey,
                Name = entity.Name,
                ModuleName = entity.ModuleName,
                Description = entity.Description,
                SamplePayloadJson = entity.SamplePayloadJson,
                IsActive = entity.IsActive
            };
        }

        public async Task<long> SaveAsync(NotificationEventFormViewModel model, long userId)
        {
            var entity = new NotificationEvent
            {
                NotificationEventId = model.NotificationEventId,
                EventKey = model.EventKey,
                Name = model.Name,
                ModuleName = model.ModuleName,
                Description = model.Description,
                SamplePayloadJson = model.SamplePayloadJson,
                IsActive = model.IsActive,
                AddedBy = userId,
                UpdatedBy = userId
            };

            if (model.NotificationEventId == 0)
            {
                entity.AddedOn = DateTime.UtcNow;
                return NotificationEventCrudService.Insert(entity) ?? 0;
            }
            else
            {
                entity.UpdatedOn = DateTime.UtcNow;
                NotificationEventCrudService.Update(entity);
                return model.NotificationEventId;
            }
        }

        public async Task<bool> DeleteAsync(long id, long userId)
        {
            var entity = NotificationEventCrudService.Get(id);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.UpdatedOn = DateTime.UtcNow;
            entity.UpdatedBy = userId;
            NotificationEventCrudService.Update(entity);
            return true;
        }

        public async Task<bool> ToggleStatusAsync(long id, bool isActive, long userId)
        {
            var entity = NotificationEventCrudService.Get(id);
            if (entity == null) return false;

            entity.IsActive = isActive;
            entity.UpdatedOn = DateTime.UtcNow;
            entity.UpdatedBy = userId;
            NotificationEventCrudService.Update(entity);
            return true;
        }

        public async Task<NotificationEventFormViewModel> TestTriggerAsync(long id, object payload, long actorUserId)
        {
            var entity = NotificationEventCrudService.Get(id);
            if (entity == null) return null;

            // This will be implemented when we have the rule engine
            return new NotificationEventFormViewModel
            {
                NotificationEventId = entity.NotificationEventId,
                EventKey = entity.EventKey,
                Name = entity.Name,
                ModuleName = entity.ModuleName,
                IsActive = entity.IsActive
            };
        }
    }
}
