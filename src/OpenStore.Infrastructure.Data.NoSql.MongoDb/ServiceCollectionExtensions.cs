using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OpenStore.Application;
using OpenStore.Application.Crud;
using OpenStore.Domain;
using OpenStore.Infrastructure.Data.NoSql.MongoDb.Crud;

namespace OpenStore.Infrastructure.Data.NoSql.MongoDb
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoDbDataInfrastructure(this IServiceCollection services, Action<MongoClientSettings> ravenDbSettingsBuilder, string databaseName, params Assembly[] assemblies)
        {
            services.Configure(ravenDbSettingsBuilder);
            return AddMongoServices(services, databaseName, assemblies);
        }

        public static IServiceCollection AddMongoDbDataInfrastructure(this IServiceCollection services, IConfiguration configuration, params Assembly[] assemblies)
        {
            services.Configure<MongoClientSettings>(configuration);
            return AddMongoServices(services, configuration.GetValue<string>("DatabaseName"), assemblies);
        }

        private static IServiceCollection AddMongoServices(IServiceCollection services, string databaseName, params Assembly[] assemblies)
        {
            services.AddSingleton(sp => new MongoClient(sp.GetRequiredService<IOptions<MongoClientSettings>>().Value));
            services.AddSingleton(sp => sp.GetRequiredService<MongoClient>().GetDatabase(databaseName));

            services.AddScoped<IOutBoxService, MongoOutBoxService>();
            services
                .AddScoped<IMongoUnitOfWork>(sp => new MongoUnitOfWork(sp.GetRequiredService<IMongoDatabase>()))
                .AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IMongoUnitOfWork>());

            if (assemblies != null && assemblies.Any())
            {
                // add repositories automatically
                services.Scan(scan =>
                {
                    scan
                        .FromAssemblies(assemblies)
                        //
                        .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
                        .AsImplementedInterfaces()
                        .WithScopedLifetime()
                        ;

                    scan
                        .FromAssemblies(assemblies)
                        //
                        .AddClasses(classes => classes.AssignableTo(typeof(ICrudService<,>)))
                        .AsImplementedInterfaces()
                        .WithScopedLifetime()
                        ;
                    
                    scan
                        .FromAssemblies(assemblies)
                        //
                        .AddClasses(classes => classes.AssignableTo(typeof(ICrudRepository<>)))
                        .AsImplementedInterfaces()
                        .WithScopedLifetime()
                        ;
                });
            }


            // for generic resolve
            services
                .AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>))
                .AddScoped(typeof(IRepository<>), typeof(MongoRepository<>))
                .AddScoped(typeof(ITransactionalRepository<>), typeof(MongoRepository<>))
                .AddScoped(typeof(ICrudRepository<>), typeof(MongoCrudRepository<>))
                ;

            // Crud services
            services
                .AddScoped(typeof(ICrudService<,>), typeof(MongoCrudService<,>))
                ;

            return services;
        }
    }
}