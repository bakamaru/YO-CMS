using Dapper;
using System.Text.Json;
using System.Text.RegularExpressions;
using YangOne.Data;
using YangOne.Data.Crud;
using YangOne.Log;
using YangOne.Web.Model.Notification;
using YangOne.Web.ViewModel.Notification;

namespace YangOne.Web.Service.Notification
{
    public class NotificationTemplateService : INotificationTemplateService
    {
        private readonly ILogger _logger;
        public CrudService<NotificationTemplate> NotificationTemplateCrudService { get; set; } = new CrudService<NotificationTemplate>();

        public NotificationTemplateService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<NotificationTemplateListResponse> GetListAsync(NotificationTemplateQueryModel query)
        {
            var where = new DynamicParameters();
            string conditions = "WHERE IsDeleted = 0";

            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                conditions += " AND (TemplateKey LIKE @Keyword OR Name LIKE @Keyword)";
                where.Add("Keyword", $"%{query.Keyword}%");
            }

            if (!string.IsNullOrWhiteSpace(query.Channel))
            {
                conditions += " AND Channel = @Channel";
                where.Add("Channel", query.Channel);
            }

            if (query.IsActive.HasValue)
            {
                conditions += " AND IsActive = @IsActive";
                where.Add("IsActive", query.IsActive.Value);
            }

            string countSql = $"SELECT COUNT(*) FROM NotificationTemplate {conditions}";
            string dataSql = $@"
SELECT NotificationTemplateId, TemplateKey, Name, Channel, LanguageCode, IsDefault, IsActive, Version, AddedOn
FROM NotificationTemplate
{conditions}
ORDER BY AddedOn DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var dbFactory = DbFactoryProvider.GetFactory();
            using var db = dbFactory.GetConnection();

            var total = await db.ExecuteScalarAsync<int>(countSql, where);
            where.Add("Offset", (query.Page - 1) * query.PageSize);
            where.Add("PageSize", query.PageSize);

            var items = await db.QueryAsync<NotificationTemplateListViewModel>(dataSql, where);

            return new NotificationTemplateListResponse
            {
                Items = items.ToList(),
                TotalCount = total,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }

        public async Task<NotificationTemplateFormViewModel> GetByIdAsync(long id)
        {
            var entity = NotificationTemplateCrudService.Get(id);
            if (entity == null) return null;

            return new NotificationTemplateFormViewModel
            {
                NotificationTemplateId = entity.NotificationTemplateId,
                TemplateKey = entity.TemplateKey,
                Name = entity.Name,
                Channel = entity.Channel,
                LanguageCode = entity.LanguageCode,
                SubjectTemplate = entity.SubjectTemplate,
                BodyTemplate = entity.BodyTemplate,
                SamplePayloadJson = entity.SamplePayloadJson,
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive
            };
        }

        public async Task<long> SaveAsync(NotificationTemplateFormViewModel model, long userId)
        {
            var entity = new NotificationTemplate
            {
                NotificationTemplateId = model.NotificationTemplateId,
                TemplateKey = model.TemplateKey,
                Name = model.Name,
                Channel = model.Channel,
                LanguageCode = model.LanguageCode,
                SubjectTemplate = model.SubjectTemplate,
                BodyTemplate = model.BodyTemplate,
                SamplePayloadJson = model.SamplePayloadJson,
                IsDefault = model.IsDefault,
                IsActive = model.IsActive,
                AddedBy = userId,
                UpdatedBy = userId
            };

            if (model.NotificationTemplateId == 0)
            {
                entity.AddedOn = DateTime.UtcNow;
                entity.Version = 1;
                return NotificationTemplateCrudService.Insert(entity) ?? 0;
            }
            else
            {
                entity.UpdatedOn = DateTime.UtcNow;
                entity.Version++;
                NotificationTemplateCrudService.Update(entity);
                return model.NotificationTemplateId;
            }
        }

        public async Task<bool> DeleteAsync(long id, long userId)
        {
            var entity = NotificationTemplateCrudService.Get(id);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.UpdatedOn = DateTime.UtcNow;
            entity.UpdatedBy = userId;
            NotificationTemplateCrudService.Update(entity);
            return true;
        }

        public async Task<NotificationTemplatePreviewResponse> PreviewAsync(NotificationTemplatePreviewRequest request)
        {
            var response = new NotificationTemplatePreviewResponse();

            var variables = ExtractVariables(request.SubjectTemplate + request.BodyTemplate);
            response.UsedVariables = variables;

            var payload = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(request.PayloadJson))
            {
                try
                {
                    payload = JsonSerializer.Deserialize<Dictionary<string, object>>(request.PayloadJson) ?? new();
                }
                catch { }
            }

            response.Subject = RenderTemplate(request.SubjectTemplate, payload);
            response.Body = RenderTemplate(request.BodyTemplate, payload);

            foreach (var v in variables)
            {
                if (!payload.ContainsKey(v))
                {
                    response.MissingVariables.Add(v);
                }
            }

            return response;
        }

        public async Task<NotificationTemplateValidateResponse> ValidateAsync(NotificationTemplateValidateRequest request)
        {
            var response = new NotificationTemplateValidateResponse();

            var variables = ExtractVariables(request.SubjectTemplate + request.BodyTemplate);
            response.UsedVariables = variables;

            var available = new List<string>();
            if (!string.IsNullOrWhiteSpace(request.AvailableVariablesJson))
            {
                try
                {
                    available = JsonSerializer.Deserialize<List<string>>(request.AvailableVariablesJson) ?? new();
                }
                catch { }
            }

            response.AvailableVariables = available;

            foreach (var v in variables)
            {
                if (!available.Contains(v))
                {
                    response.MissingVariables.Add(v);
                }
            }

            response.IsValid = response.MissingVariables.Count == 0;
            return response;
        }

        public async Task<long> CloneAsync(long id, long userId)
        {
            var entity = NotificationTemplateCrudService.Get(id);
            if (entity == null) return 0;

            var clone = new NotificationTemplate
            {
                TemplateKey = entity.TemplateKey + "_copy_" + DateTime.UtcNow.Ticks,
                Name = entity.Name + " (Copy)",
                Channel = entity.Channel,
                LanguageCode = entity.LanguageCode,
                SubjectTemplate = entity.SubjectTemplate,
                BodyTemplate = entity.BodyTemplate,
                SamplePayloadJson = entity.SamplePayloadJson,
                IsDefault = false,
                IsActive = entity.IsActive,
                Version = 1,
                AddedBy = userId,
                AddedOn = DateTime.UtcNow
            };

            return NotificationTemplateCrudService.Insert(clone) ?? 0;
        }

        public async Task<NotificationTemplateFormViewModel> TestSendAsync(long id, object payload, long userId)
        {
            var entity = NotificationTemplateCrudService.Get(id);
            if (entity == null) return null;

            return new NotificationTemplateFormViewModel
            {
                NotificationTemplateId = entity.NotificationTemplateId,
                TemplateKey = entity.TemplateKey,
                Name = entity.Name,
                Channel = entity.Channel
            };
        }

        public async Task<List<NotificationTemplateListViewModel>> GetByChannelAsync(string channel)
        {
            var dbFactory = DbFactoryProvider.GetFactory();
            using var db = dbFactory.GetConnection();

            var sql = "SELECT NotificationTemplateId, TemplateKey, Name, Channel, LanguageCode, IsDefault, IsActive, Version, AddedOn FROM NotificationTemplate WHERE IsDeleted = 0 AND Channel = @Channel AND IsActive = 1 ORDER BY IsDefault DESC, Name";
            var items = await db.QueryAsync<NotificationTemplateListViewModel>(sql, new { Channel = channel });

            return items.ToList();
        }

        private List<string> ExtractVariables(string template)
        {
            var variables = new List<string>();
            var matches = Regex.Matches(template, @"\{\{(\w+)\}\}");
            foreach (Match m in matches)
            {
                if (m.Groups.Count > 1)
                {
                    variables.Add(m.Groups[1].Value);
                }
            }
            return variables.Distinct().ToList();
        }

        private string RenderTemplate(string template, Dictionary<string, object> payload)
        {
            var result = template;
            foreach (var kvp in payload)
            {
                result = result.Replace($"{{{{{kvp.Key}}}}}", kvp.Value?.ToString() ?? "");
            }
            return result;
        }
    }
}
