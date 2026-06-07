using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YangOne.AppEvent;
using YangOne.DI;
using YangOne.Web.Service.Notification;

namespace YangOne.Web.AppEvent
{
    public class AppEventWebRegistrar : IServiceRegistrar
    {
        public void Register(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<INotificationRuleEngine, NotificationRuleEngine>();
            services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
            services.AddScoped<IAppEventProcessor, NotificationAppEventProcessor>();
            services.AddScoped<IExternalEventTransport, DirectSqlOutboxEventTransport>();
            services.AddHostedService<AppEventOutboxWorker>();
        }

        public void Update(IServiceCollection serviceCollection)
        {
        }
    }
}
