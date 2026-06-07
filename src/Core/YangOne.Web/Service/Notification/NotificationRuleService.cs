using Dapper;
using System.Text.Json;
using YangOne.Data;
using YangOne.Data.Crud;
using YangOne.Log;
using YangOne.Web.Model.Notification;
using YangOne.Web.ViewModel.Notification;

namespace YangOne.Web.Service.Notification
{
    public class NotificationRuleService : INotificationRuleService
    {
        private readonly ILogger _logger;
        public CrudService<NotificationRule> NotificationRuleCrudService { get; set; } = new CrudService<NotificationRule>();
        public CrudService<NotificationRuleRecipient> NotificationRuleRecipientCrudService { get; set; } = new CrudService<NotificationRuleRecipient>();
        public CrudService<NotificationRuleChannel> NotificationRuleChannelCrudService { get; set; } = new CrudService<NotificationRuleChannel>();

        public NotificationRuleService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<NotificationRuleListResponse> GetListAsync(NotificationRuleQueryModel query)
        {
            var where = new DynamicParameters();
            string conditions = "WHERE nr.IsDeleted = 0";

            if (!string.IsNullOrWhiteSpace(query.EventKey))
            {
                conditions += " AND ne.EventKey = @EventKey";
                where.Add("EventKey", query.EventKey);
            }

            if (query.IsEnabled.HasValue)
            {
                conditions += " AND nr.IsEnabled = @IsEnabled";
                where.Add("IsEnabled", query.IsEnabled.Value);
            }

            string countSql = $@"
SELECT COUNT(*) 
FROM NotificationRule nr
LEFT JOIN NotificationEvent ne ON ne.NotificationEventId = nr.NotificationEventId
{conditions}";

            string dataSql = $@"
SELECT nr.NotificationRuleId, nr.Name, ne.EventKey, nr.Priority,
       (SELECT STRING_AGG(rc.Channel, ', ') FROM NotificationRuleChannel rc WHERE rc.NotificationRuleId = nr.NotificationRuleId AND rc.IsDeleted = 0) AS Channels,
       (SELECT STRING_AGG(rr.RecipientType, ', ') FROM NotificationRuleRecipient rr WHERE rr.NotificationRuleId = nr.NotificationRuleId AND rr.IsDeleted = 0) AS Recipients,
       nr.IsEnabled, nr.AddedOn
FROM NotificationRule nr
LEFT JOIN NotificationEvent ne ON ne.NotificationEventId = nr.NotificationEventId
{conditions}
ORDER BY nr.Priority, nr.AddedOn DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var dbFactory = DbFactoryProvider.GetFactory();
            using var db = dbFactory.GetConnection();

            var total = await db.ExecuteScalarAsync<int>(countSql, where);
            where.Add("Offset", (query.Page - 1) * query.PageSize);
            where.Add("PageSize", query.PageSize);

            var items = await db.QueryAsync<NotificationRuleListViewModel>(dataSql, where);

            return new NotificationRuleListResponse
            {
                Items = items.ToList(),
                TotalCount = total,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }

        public async Task<NotificationRuleFormViewModel> GetByIdAsync(long id)
        {
            var entity = NotificationRuleCrudService.Get(id);
            if (entity == null) return null;

            var dbFactory = DbFactoryProvider.GetFactory();
            using var db = dbFactory.GetConnection();

            var recipients = await db.QueryAsync<NotificationRuleRecipientViewModel>(
                "SELECT * FROM NotificationRuleRecipient WHERE NotificationRuleId = @Id AND IsDeleted = 0 ORDER BY SortOrder",
                new { Id = id });

            var channels = await db.QueryAsync<NotificationRuleChannelViewModel>(
                "SELECT * FROM NotificationRuleChannel WHERE NotificationRuleId = @Id AND IsDeleted = 0 ORDER BY SortOrder",
                new { Id = id });

            return new NotificationRuleFormViewModel
            {
                NotificationRuleId = entity.NotificationRuleId,
                NotificationEventId = entity.NotificationEventId,
                Name = entity.Name,
                Priority = entity.Priority,
                IsEnabled = entity.IsEnabled,
                ConditionJson = entity.ConditionJson,
                DelaySeconds = entity.DelaySeconds,
                MaxSendPerUserPerDay = entity.MaxSendPerUserPerDay,
                StopProcessingAfterMatch = entity.StopProcessingAfterMatch,
                QuietHoursEnabled = entity.QuietHoursEnabled,
                QuietHoursStart = entity.QuietHoursStart,
                QuietHoursEnd = entity.QuietHoursEnd,
                Recipients = recipients.ToList(),
                Channels = channels.ToList()
            };
        }

        public async Task<long> SaveAsync(NotificationRuleFormViewModel model, long userId)
        {
            var entity = new NotificationRule
            {
                NotificationRuleId = model.NotificationRuleId,
                NotificationEventId = model.NotificationEventId,
                Name = model.Name,
                Priority = model.Priority,
                IsEnabled = model.IsEnabled,
                ConditionJson = model.ConditionJson,
                DelaySeconds = model.DelaySeconds,
                MaxSendPerUserPerDay = model.MaxSendPerUserPerDay,
                StopProcessingAfterMatch = model.StopProcessingAfterMatch,
                QuietHoursEnabled = model.QuietHoursEnabled,
                QuietHoursStart = model.QuietHoursStart,
                QuietHoursEnd = model.QuietHoursEnd,
                AddedBy = userId,
                UpdatedBy = userId
            };

            if (model.NotificationRuleId == 0)
            {
                entity.AddedOn = DateTime.UtcNow;
                var ruleId = NotificationRuleCrudService.Insert(entity) ?? 0;

                // Save recipients
                if (model.Recipients != null)
                {
                    for (int i = 0; i < model.Recipients.Count; i++)
                    {
                        var r = model.Recipients[i];
                        var recipient = new NotificationRuleRecipient
                        {
                            NotificationRuleId = ruleId,
                            RecipientType = r.RecipientType,
                            RecipientValue = r.RecipientValue,
                            SortOrder = i,
                            AddedBy = userId,
                            AddedOn = DateTime.UtcNow
                        };
                        NotificationRuleRecipientCrudService.Insert(recipient);
                    }
                }

                // Save channels
                if (model.Channels != null)
                {
                    for (int i = 0; i < model.Channels.Count; i++)
                    {
                        var c = model.Channels[i];
                        var channel = new NotificationRuleChannel
                        {
                            NotificationRuleId = ruleId,
                            Channel = c.Channel,
                            NotificationTemplateId = c.NotificationTemplateId,
                            IsRequired = c.IsRequired,
                            SortOrder = i,
                            AddedBy = userId,
                            AddedOn = DateTime.UtcNow
                        };
                        NotificationRuleChannelCrudService.Insert(channel);
                    }
                }

                return ruleId;
            }
            else
            {
                entity.UpdatedOn = DateTime.UtcNow;
                NotificationRuleCrudService.Update(entity);

                // Update recipients - soft delete old and insert new
                var oldRecipients = NotificationRuleRecipientCrudService.GetList(new { NotificationRuleId = model.NotificationRuleId, IsDeleted = false });
                foreach (var old in oldRecipients)
                {
                    old.IsDeleted = true;
                    old.UpdatedOn = DateTime.UtcNow;
                    old.UpdatedBy = userId;
                    NotificationRuleRecipientCrudService.Update(old);
                }

                if (model.Recipients != null)
                {
                    for (int i = 0; i < model.Recipients.Count; i++)
                    {
                        var r = model.Recipients[i];
                        var recipient = new NotificationRuleRecipient
                        {
                            NotificationRuleId = model.NotificationRuleId,
                            RecipientType = r.RecipientType,
                            RecipientValue = r.RecipientValue,
                            SortOrder = i,
                            AddedBy = userId,
                            AddedOn = DateTime.UtcNow
                        };
                        NotificationRuleRecipientCrudService.Insert(recipient);
                    }
                }

                // Update channels - soft delete old and insert new
                var oldChannels = NotificationRuleChannelCrudService.GetList(new { NotificationRuleId = model.NotificationRuleId, IsDeleted = false });
                foreach (var old in oldChannels)
                {
                    old.IsDeleted = true;
                    old.UpdatedOn = DateTime.UtcNow;
                    old.UpdatedBy = userId;
                    NotificationRuleChannelCrudService.Update(old);
                }

                if (model.Channels != null)
                {
                    for (int i = 0; i < model.Channels.Count; i++)
                    {
                        var c = model.Channels[i];
                        var channel = new NotificationRuleChannel
                        {
                            NotificationRuleId = model.NotificationRuleId,
                            Channel = c.Channel,
                            NotificationTemplateId = c.NotificationTemplateId,
                            IsRequired = c.IsRequired,
                            SortOrder = i,
                            AddedBy = userId,
                            AddedOn = DateTime.UtcNow
                        };
                        NotificationRuleChannelCrudService.Insert(channel);
                    }
                }

                return model.NotificationRuleId;
            }
        }

        public async Task<bool> DeleteAsync(long id, long userId)
        {
            var entity = NotificationRuleCrudService.Get(id);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.UpdatedOn = DateTime.UtcNow;
            entity.UpdatedBy = userId;
            NotificationRuleCrudService.Update(entity);

            // Soft delete recipients and channels
            var dbFactory = DbFactoryProvider.GetFactory();
            using var db = dbFactory.GetConnection();

            await db.ExecuteAsync("UPDATE NotificationRuleRecipient SET IsDeleted = 1 WHERE NotificationRuleId = @Id", new { Id = id });
            await db.ExecuteAsync("UPDATE NotificationRuleChannel SET IsDeleted = 1 WHERE NotificationRuleId = @Id", new { Id = id });

            return true;
        }

        public async Task<bool> ToggleStatusAsync(long id, bool isEnabled, long userId)
        {
            var entity = NotificationRuleCrudService.Get(id);
            if (entity == null) return false;

            entity.IsEnabled = isEnabled;
            entity.UpdatedOn = DateTime.UtcNow;
            entity.UpdatedBy = userId;
            NotificationRuleCrudService.Update(entity);
            return true;
        }

        public async Task<long> CloneAsync(long id, long userId)
        {
            var model = await GetByIdAsync(id);
            if (model == null) return 0;

            model.NotificationRuleId = 0;
            model.Name = model.Name + " (Copy)";
            model.IsEnabled = false;

            return await SaveAsync(model, userId);
        }

        public async Task<ConditionEvaluationResponse> EvaluateConditionAsync(ConditionEvaluationRequest request)
        {
            // Simple condition evaluation - in production use a proper rule engine
            var response = new ConditionEvaluationResponse { Matched = true };

            if (string.IsNullOrWhiteSpace(request.ConditionJson))
            {
                return response;
            }

            try
            {
                var condition = JsonSerializer.Deserialize<ConditionJson>(request.ConditionJson);
                var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(request.PayloadJson);

                if (condition?.All != null)
                {
                    foreach (var c in condition.All)
                    {
                        if (payload != null && payload.TryGetValue(c.Field, out var value))
                        {
                            var valStr = value?.ToString() ?? "";
                            bool matched = c.Operator switch
                            {
                                "eq" => valStr == c.Value,
                                "neq" => valStr != c.Value,
                                "gt" => CompareValues(valStr, c.Value) > 0,
                                "gte" => CompareValues(valStr, c.Value) >= 0,
                                "lt" => CompareValues(valStr, c.Value) < 0,
                                "lte" => CompareValues(valStr, c.Value) <= 0,
                                "contains" => valStr.Contains(c.Value),
                                _ => true
                            };

                            if (!matched)
                            {
                                response.Matched = false;
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
                response.Matched = true; // Default to matched on error
            }

            return response;
        }

        public async Task<RecipientPreviewResponse> PreviewRecipientsAsync(RecipientPreviewRequest request)
        {
            var response = new RecipientPreviewResponse();
            var users = new List<RecipientUserViewModel>();

            var dbFactory = DbFactoryProvider.GetFactory();
            using var db = dbFactory.GetConnection();

            foreach (var r in request.Recipients)
            {
                switch (r.RecipientType)
                {
                    case "ActorUser":
                        if (request.ActorUserId > 0)
                        {
                            var user = await db.QueryFirstOrDefaultAsync<RecipientUserViewModel>(
                                "SELECT UserId, FullName, Email, PhoneNumber FROM [User] WHERE UserId = @UserId",
                                new { UserId = request.ActorUserId });
                            if (user != null) users.Add(user);
                        }
                        break;

                    case "Role":
                        var roleUsers = await db.QueryAsync<RecipientUserViewModel>(@"
SELECT u.UserId, u.FullName, u.Email, u.PhoneNumber
FROM [User] u
INNER JOIN UserRole ur ON ur.UserId = u.UserId
INNER JOIN Role r ON r.RoleId = ur.RoleId
WHERE r.Name = @RoleName AND u.IsDeleted = 0", new { RoleName = r.RecipientValue });
                        users.AddRange(roleUsers);
                        break;

                    case "SpecificUser":
                        if (long.TryParse(r.RecipientValue, out var specificUserId))
                        {
                            var user = await db.QueryFirstOrDefaultAsync<RecipientUserViewModel>(
                                "SELECT UserId, FullName, Email, PhoneNumber FROM [User] WHERE UserId = @UserId",
                                new { UserId = specificUserId });
                            if (user != null) users.Add(user);
                        }
                        break;
                }
            }

            // Deduplicate
            response.Recipients = users.GroupBy(u => u.UserId).Select(g => g.First()).ToList();
            response.TotalRecipients = response.Recipients.Count;

            return response;
        }

        public async Task<NotificationRuleFormViewModel> TestAsync(long id, object payload, long actorUserId)
        {
            var model = await GetByIdAsync(id);
            if (model == null) return null;

            // Test would trigger the rule engine - placeholder
            return model;
        }

        private int CompareValues(string val1, string val2)
        {
            if (decimal.TryParse(val1, out var d1) && decimal.TryParse(val2, out var d2))
            {
                return d1.CompareTo(d2);
            }
            return string.Compare(val1, val2, StringComparison.Ordinal);
        }

        private class ConditionJson
        {
            public List<ConditionItem> All { get; set; } = new();
        }

        private class ConditionItem
        {
            public string Field { get; set; } = "";
            public string Operator { get; set; } = "";
            public string Value { get; set; } = "";
        }
    }
}
