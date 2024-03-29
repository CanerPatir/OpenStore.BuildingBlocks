using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenStore.Infrastructure.Tasks.InMemory.Queued;
using OpenStore.Infrastructure.Tasks.InMemory.Scheduled;

namespace OpenStore.Infrastructure.Tasks.InMemory;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenStoreInMemoryBackgroundTasks(this IServiceCollection services)
    {
        var channel = Channel.CreateBounded<Func<IServiceProvider, CancellationToken, Task>>(new BoundedChannelOptions(1)
        {
            SingleReader = true
        });
        return services
                .AddSingleton<ITaskManager, InMemoryTaskManager>(sp => new InMemoryTaskManager(channel.Writer))
                .AddHostedService(sp => new InMemoryQueuedJobHost(sp, channel.Reader))
            ;
    }

    public static IServiceCollection AddOpenStoreRecurringJob<TRecurringJob>(this IServiceCollection services, string cronExpression)
        where TRecurringJob : class, IRecurringJob
    {
        services.AddScoped<TRecurringJob>();
        return services.AddHostedService(sp => new RecurringHostedService<TRecurringJob>(
                cronExpression,
                sp.GetRequiredService<IServiceScopeFactory>(),
                sp.GetRequiredService<ILogger<RecurringHostedService<TRecurringJob>>>()
            )
        );
    }
}