using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace OpenStore.Infrastructure.Messaging.InMemory;

/// <summary>
/// Don't use in-memory messaging in distributed deployment mode
/// Not guaranteed consistency  
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInMemoryProducer(this IServiceCollection services)
    {
        services.AddSingleton<ChannelFactory>();
        services.AddSingleton<IOpenStoreProducer, InMemoryProducer>();

        return services;
    }

    public static IServiceCollection AddInMemoryConsumer<TConsumer, TMessage>(this IServiceCollection services, int retry = 0)
        where TConsumer : class, IOpenStoreConsumer<TMessage>
        where TMessage : class
    {
        services.AddSingleton<ChannelFactory>();
        services.AddScoped<IOpenStoreConsumer<TMessage>, TConsumer>();
        services.AddHostedService<InMemoryConsumerHost<TMessage>>(sp =>
        {
            var channelFactory = sp.GetRequiredService<ChannelFactory>();
            AsyncRetryPolicy retryPolicy = null;
            if (retry > 0)
            {
                retryPolicy = Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(retry, retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    );
            }

            return new InMemoryConsumerHost<TMessage>(
                channelFactory.CreateOrGet<TMessage>().Reader,
                sp.GetRequiredService<IServiceScopeFactory>(),
                retryPolicy,
                sp.GetRequiredService<ILogger<InMemoryConsumerHost<TMessage>>>());
        });
        return services;
    }
}