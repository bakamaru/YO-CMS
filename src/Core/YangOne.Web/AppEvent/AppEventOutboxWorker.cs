using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YangOne.AppEvent;

namespace YangOne.Web.AppEvent
{
    public class AppEventOutboxWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppEventOutboxWorker> _logger;
        private readonly string _workerId = $"{Environment.MachineName}-{Guid.NewGuid():N}";

        public AppEventOutboxWorker(
            IServiceProvider serviceProvider,
            ILogger<AppEventOutboxWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();

                var store = scope.ServiceProvider.GetRequiredService<IOutboxEventStore>();
                var processor = scope.ServiceProvider.GetRequiredService<IAppEventProcessor>();

                var events = await store.GetPendingAsync(50, stoppingToken);

                foreach (var appEvent in events)
                {
                    try
                    {
                        await store.MarkProcessingAsync(
                            appEvent.AppEventOutboxId,
                            _workerId,
                            stoppingToken);

                        await processor.ProcessAsync(appEvent, stoppingToken);

                        await store.MarkCompletedAsync(
                            appEvent.AppEventOutboxId,
                            stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Failed to process outbox event {OutboxEventId}",
                            appEvent.AppEventOutboxId);

                        var nextRetryOn = DateTime.UtcNow.AddSeconds(
                            CalculateRetryDelaySeconds(appEvent.RetryCount));

                        await store.MarkFailedAsync(
                            appEvent.AppEventOutboxId,
                            ex.Message,
                            nextRetryOn,
                            stoppingToken);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        private static int CalculateRetryDelaySeconds(int retryCount)
        {
            return retryCount switch
            {
                0 => 10,
                1 => 30,
                2 => 60,
                3 => 300,
                _ => 900
            };
        }
    }
}
