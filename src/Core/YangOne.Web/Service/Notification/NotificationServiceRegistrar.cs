using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YangOne.DI;
using YangOne.Web.Service.Notification;

namespace YangOne.Web.Service.Notification
{
    public class NotificationServiceRegistrar : IServiceRegistrar
    {
        public void Register(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<INotificationEventService, NotificationEventService>();
            services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
            services.AddScoped<INotificationRuleService, NotificationRuleService>();
        }

        public void Update(IServiceCollection serviceCollection)
        {
        }
    }
}
