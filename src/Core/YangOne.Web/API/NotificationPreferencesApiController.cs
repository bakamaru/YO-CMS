using Dapper;
using Microsoft.AspNetCore.Mvc;
using YangOne.Data;
using YangOne.Identity.Extensions;
using YangOne.Web.API;
using YangOne.Web.Model.Notification;
using YangOne.Web.ViewModel.Notification;

namespace YangOne.Web.API
{
    [Route("api/admin/notification-preferences")]
    public class NotificationPreferencesAdminApiController : BaseApiController
    {
        [HttpGet("users/{userId}")]
        public async Task<dynamic> GetByUserId(long userId)
        {
            try
            {
                var dbFactory = DbFactoryProvider.GetFactory();
                using var db = dbFactory.GetConnection();

                var sql = "SELECT * FROM NotificationPreference WHERE UserId = @UserId AND IsDeleted = 0";
                var pref = await db.QueryFirstOrDefaultAsync<NotificationPreferenceViewModel>(sql, new { UserId = userId });

                if (pref == null)
                {
                    pref = new NotificationPreferenceViewModel { UserId = userId };
                }

                return SuccessResponse("Preferences retrieved", pref);
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpPut("users/{userId}")]
        public async Task<dynamic> UpdateByUserId(long userId, [FromBody] NotificationPreferenceFormViewModel model)
        {
            try
            {
                var currentUserId = User.Identity.GetIdentityUserId();

                var dbFactory = DbFactoryProvider.GetFactory();
                using var db = dbFactory.GetConnection();

                var existing = await db.QueryFirstOrDefaultAsync("SELECT NotificationPreferenceId FROM NotificationPreference WHERE UserId = @UserId AND IsDeleted = 0", new { UserId = userId });

                if (existing != null)
                {
                    var sql = @"UPDATE NotificationPreference SET 
                        EmailEnabled = @EmailEnabled, SmsEnabled = @SmsEnabled, PushEnabled = @PushEnabled, InAppEnabled = @InAppEnabled,
                        QuietHoursEnabled = @QuietHoursEnabled, QuietHoursStart = @QuietHoursStart, QuietHoursEnd = @QuietHoursEnd,
                        MutedCategoriesJson = @MutedCategoriesJson, UpdatedOn = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
                        WHERE UserId = @UserId";
                    
                    await db.ExecuteAsync(sql, new
                    {
                        UserId = userId,
                        model.EmailEnabled,
                        model.SmsEnabled,
                        model.PushEnabled,
                        model.InAppEnabled,
                        model.QuietHoursEnabled,
                        model.QuietHoursStart,
                        model.QuietHoursEnd,
                        MutedCategoriesJson = System.Text.Json.JsonSerializer.Serialize(model.MutedCategories),
                        UpdatedBy = currentUserId
                    });
                }
                else
                {
                    var sql = @"INSERT INTO NotificationPreference 
                        (UserId, EmailEnabled, SmsEnabled, PushEnabled, InAppEnabled, QuietHoursEnabled, QuietHoursStart, QuietHoursEnd, MutedCategoriesJson, AddedOn, AddedBy)
                        VALUES (@UserId, @EmailEnabled, @SmsEnabled, @PushEnabled, @InAppEnabled, @QuietHoursEnabled, @QuietHoursStart, @QuietHoursEnd, @MutedCategoriesJson, SYSUTCDATETIME(), @AddedBy)";
                    
                    await db.ExecuteAsync(sql, new
                    {
                        UserId = userId,
                        model.EmailEnabled,
                        model.SmsEnabled,
                        model.PushEnabled,
                        model.InAppEnabled,
                        model.QuietHoursEnabled,
                        model.QuietHoursStart,
                        model.QuietHoursEnd,
                        MutedCategoriesJson = System.Text.Json.JsonSerializer.Serialize(model.MutedCategories),
                        AddedBy = currentUserId
                    });
                }

                return SuccessResponse("Preferences updated", null);
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }
    }

    [Route("api/notifications/preferences")]
    public class NotificationPreferencesUserApiController : BaseApiController
    {
        [HttpGet("my")]
        public async Task<dynamic> GetMy()
        {
            try
            {
                var userId = GetCurrentUserId();

                var dbFactory = DbFactoryProvider.GetFactory();
                using var db = dbFactory.GetConnection();

                var sql = "SELECT * FROM NotificationPreference WHERE UserId = @UserId AND IsDeleted = 0";
                var pref = await db.QueryFirstOrDefaultAsync<NotificationPreferenceViewModel>(sql, new { UserId = userId });

                if (pref == null)
                {
                    pref = new NotificationPreferenceViewModel { UserId = userId };
                }

                return SuccessResponse("Preferences retrieved", pref);
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        [HttpPut("my")]
        public async Task<dynamic> UpdateMy([FromBody] NotificationPreferenceFormViewModel model)
        {
            try
            {
                var userId = GetCurrentUserId();

                var dbFactory = DbFactoryProvider.GetFactory();
                using var db = dbFactory.GetConnection();

                var existing = await db.QueryFirstOrDefaultAsync("SELECT NotificationPreferenceId FROM NotificationPreference WHERE UserId = @UserId AND IsDeleted = 0", new { UserId = userId });

                if (existing != null)
                {
                    var sql = @"UPDATE NotificationPreference SET 
                        EmailEnabled = @EmailEnabled, SmsEnabled = @SmsEnabled, PushEnabled = @PushEnabled, InAppEnabled = @InAppEnabled,
                        QuietHoursEnabled = @QuietHoursEnabled, QuietHoursStart = @QuietHoursStart, QuietHoursEnd = @QuietHoursEnd,
                        MutedCategoriesJson = @MutedCategoriesJson, UpdatedOn = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
                        WHERE UserId = @UserId";
                    
                    await db.ExecuteAsync(sql, new
                    {
                        UserId = userId,
                        model.EmailEnabled,
                        model.SmsEnabled,
                        model.PushEnabled,
                        model.InAppEnabled,
                        model.QuietHoursEnabled,
                        model.QuietHoursStart,
                        model.QuietHoursEnd,
                        MutedCategoriesJson = System.Text.Json.JsonSerializer.Serialize(model.MutedCategories),
                        UpdatedBy = userId
                    });
                }
                else
                {
                    var sql = @"INSERT INTO NotificationPreference 
                        (UserId, EmailEnabled, SmsEnabled, PushEnabled, InAppEnabled, QuietHoursEnabled, QuietHoursStart, QuietHoursEnd, MutedCategoriesJson, AddedOn, AddedBy)
                        VALUES (@UserId, @EmailEnabled, @SmsEnabled, @PushEnabled, @InAppEnabled, @QuietHoursEnabled, @QuietHoursStart, @QuietHoursEnd, @MutedCategoriesJson, SYSUTCDATETIME(), @AddedBy)";
                    
                    await db.ExecuteAsync(sql, new
                    {
                        UserId = userId,
                        model.EmailEnabled,
                        model.SmsEnabled,
                        model.PushEnabled,
                        model.InAppEnabled,
                        model.QuietHoursEnabled,
                        model.QuietHoursStart,
                        model.QuietHoursEnd,
                        MutedCategoriesJson = System.Text.Json.JsonSerializer.Serialize(model.MutedCategories),
                        AddedBy = userId
                    });
                }

                return SuccessResponse("Preferences updated", null);
            }
            catch (Exception ex)
            {
                return ExceptionResponse(ex);
            }
        }

        private long GetCurrentUserId() => 1;
    }
}
