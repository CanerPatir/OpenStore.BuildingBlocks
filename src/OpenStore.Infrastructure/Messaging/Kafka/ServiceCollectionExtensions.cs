using System;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace OpenStore.Infrastructure.Messaging.Kafka
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKafkaProducer(this IServiceCollection services, Action<KafkaProducerOptions> optionsBuilder)
        {
            services.Configure(optionsBuilder);
            AddProducerDefaults(services);

            return services;
        }

        public static IServiceCollection AddKafkaProducer(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            services.Configure<KafkaProducerOptions>(configurationSection);
            AddProducerDefaults(services);

            return services;
        }

        private static void AddProducerDefaults(IServiceCollection services)
        {
            services.AddSingleton<IProducerFactory, ProducerFactory>(sp =>
            {
                var kafkaProducerOptions = sp.GetRequiredService<IOptions<KafkaProducerOptions>>().Value;
                var config = new ProducerConfig
                {
                    BootstrapServers = string.Join(",", kafkaProducerOptions),
                    CompressionType = kafkaProducerOptions.CompressionType,
                };

                return new ProducerFactory(config);
            });
            services.AddSingleton<IOpenStoreProducer, KafkaProducer>();
        }

        public static IServiceCollection AddKafkaConsumer<TConsumer, TMessage>(this IServiceCollection services, string topic, Action<KafkaConsumerOptions> optionsBuilder, int retry = 0)
            where TConsumer : class, IOpenStoreConsumer<TMessage>
            where TMessage : class
        {
            services.Configure(optionsBuilder);
            AddConsumerDefaults<TConsumer, TMessage>(services, topic, retry);

            return services;
        }

        public static IServiceCollection AddKafkaConsumer<TConsumer, TMessage>(this IServiceCollection services, string topic, IConfigurationSection configurationSection, int retry = 0)
            where TConsumer : class, IOpenStoreConsumer<TMessage>
            where TMessage : class
        {
            services.Configure<KafkaConsumerOptions>(configurationSection);
            AddConsumerDefaults<TConsumer, TMessage>(services, topic, retry);

            return services;
        }

        private static void AddConsumerDefaults<TConsumer, TMessage>(IServiceCollection services, string topic, int retry)
            where TConsumer : class, IOpenStoreConsumer<TMessage>
            where TMessage : class
        {
            services.AddScoped<IOpenStoreConsumer<TMessage>, TConsumer>();
            services.AddHostedService<KafkaConsumerHost<TMessage>>(sp =>
            {
                var kafkaConsumerOptions = sp.GetRequiredService<IOptions<KafkaConsumerOptions>>().Value;
                var config = new ConsumerConfig
                {
                    GroupId = typeof(TConsumer).FullName,
                    BootstrapServers = string.Join(",", kafkaConsumerOptions),
                    AutoOffsetReset = kafkaConsumerOptions.AutoOffsetReset,
                    EnableAutoCommit = false
                };

                var consumer = new ConsumerBuilder<Ignore, TMessage>(config)
                    .SetValueDeserializer(new DefaultMessageSerializer<TMessage>())
                    .Build();
                consumer.Subscribe(topic);

                AsyncRetryPolicy retryPolicy = null;
                if (retry > 0)
                {
                    retryPolicy = Policy
                        .Handle<Exception>()
                        .WaitAndRetryAsync(retry, retryAttempt =>
                            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                        );
                }

                return new KafkaConsumerHost<TMessage>(
                    sp.GetRequiredService<IServiceScopeFactory>(),
                    consumer,
                    retryPolicy,
                    sp.GetRequiredService<ILogger<KafkaConsumerHost<TMessage>>>()
                );
            });
        }
    }
}