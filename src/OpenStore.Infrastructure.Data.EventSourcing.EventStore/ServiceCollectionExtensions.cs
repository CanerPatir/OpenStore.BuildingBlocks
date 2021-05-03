using System;
using System.Linq;
using System.Reflection;
using EventStore.ClientAPI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Domain.EventSourcing;

namespace OpenStore.Infrastructure.Data.EventSourcing.EventStore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventStoreDataInfrastructure(this IServiceCollection services, Action<ConnectionSettings> eventStoreSettingsBuilder, params Assembly[] assemblies)
        {
            services.Configure(eventStoreSettingsBuilder);
            return AddDefaultServices(services, assemblies);
        }

        public static IServiceCollection AddEventStoreDataInfrastructure(this IServiceCollection services, IConfigurationSection configurationSection, params Assembly[] assemblies)
        {
            services.Configure<ConnectionSettings>(configurationSection);
            return AddDefaultServices(services, assemblies);
        }

        private static IServiceCollection AddDefaultServices(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.AddSingleton<ISerializer, Serializer>();
            services.AddSingleton<Func<IEventStoreConnection>>(sp => () => EventStoreConnection.Create(sp.GetRequiredService<ConnectionSettings>()));
            services.AddSingleton<IEventStoreStorageConnectionProvider>(sp => new EventStoreStorageConnectionProvider(sp.GetRequiredService<Func<IEventStoreConnection>>()));

            if (assemblies != null && assemblies.Any())
            {
                // add repositories automatically
                services.Scan(scan =>
                {
                    scan
                        .FromAssemblies(assemblies)
                        //
                        .AddClasses(classes => classes.AssignableTo(typeof(IEventSourcingRepository<,>)))
                        .AsImplementedInterfaces()
                        .WithScopedLifetime()
                        ;
                });
            }

            // for generic resolve
            services
                .AddScoped(typeof(ISession<,>), typeof(Session<,>))
                .AddScoped(typeof(IEventStorageProvider<>), typeof(EventStoreEventStorageProvider<>))
                .AddScoped(typeof(ISnapshotStorageProvider<,>), typeof(EventStoreSnapshotStorageProvider<,>))
                .AddScoped(typeof(IEventSourcingRepository<,>), typeof(EventSourcingRepository<,>))
                //.AddScoped(typeof(IRepository<>), sp => sp.GetService(typeof(IEventSourcingRepository<,>)))
                ;

            return services;
        }
    }
}