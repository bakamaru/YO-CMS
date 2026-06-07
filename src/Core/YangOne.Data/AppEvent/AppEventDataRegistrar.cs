using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YangOne.AppEvent;
using YangOne.DI;

namespace YangOne.Data.AppEvent
{
    public class AppEventDataRegistrar : IServiceRegistrar
    {
        public void Register(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IOutboxEventStore, SqlOutboxEventStore>();
            services.AddScoped<IAppEventPublisher, SqlOutboxAppEventPublisher>();
        }

        public void Update(IServiceCollection serviceCollection)
        {
        }
    }
}
