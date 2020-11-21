using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Infrastructure.Tasks.InMemory.Queued;
using OpenStore.Infrastructure.Tasks.InMemory.Scheduled;

namespace OpenStore.Infrastructure.Tasks.InMemory
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInMemoryBackgroundTasks(this IServiceCollection services)
        {
            var channel = Channel.CreateBounded<Func<IServiceProvider, CancellationToken, Task>>(new BoundedChannelOptions(1)
            {
                SingleReader = true
            });
            return services
                    .AddSingleton<ITaskManager, InMemoryTaskManager>(sp => new InMemoryTaskManager(channel.Writer))
                    .AddHostedService(sp => new InMemoryQueuedJobHost(sp, channel.Reader))
                    .AddHostedService<InMemorySchedulerHost>()
                ;
        }

        public static IServiceCollection AddRecurringHostedService<TSchedule>(this IServiceCollection services)
            where TSchedule : RecurringHostedService
        {
            return services.AddHostedService<TSchedule>();
        }
    }
}