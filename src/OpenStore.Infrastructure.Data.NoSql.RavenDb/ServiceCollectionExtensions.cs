using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenStore.Application;
using OpenStore.Application.Crud;
using OpenStore.Domain;
using OpenStore.Infrastructure.Data.NoSql.RavenDb.Crud;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace OpenStore.Infrastructure.Data.NoSql.RavenDb
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRavenDbDataInfrastructure(this IServiceCollection services, Action<RavenDatabaseSettings> ravenDbSettingsBuilder, params Assembly[] assemblies)
        {
            services.Configure(ravenDbSettingsBuilder);
            return AddRavenServices(services, assemblies);
        }

        public static IServiceCollection AddRavenDbDataInfrastructure(this IServiceCollection services, IConfiguration configuration, params Assembly[] assemblies)
        {
            services.Configure<RavenDatabaseSettings>(configuration);
            return AddRavenServices(services, assemblies);
        }

        private static IServiceCollection AddRavenServices(IServiceCollection services, params Assembly[] assemblies)
        {
            services.AddSingleton<IDocumentStore>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<RavenDatabaseSettings>>().Value;
                var store = new DocumentStore
                {
                    Urls = settings.Urls,
                    Database = settings.DatabaseName,
                    Certificate = new X509Certificate2(settings.CertPath, settings.CertPass)
                };

                store.Initialize();

                return store;
            });

            services.AddScoped<IAsyncDocumentSession>(sp => sp.GetService<IDocumentStore>().OpenAsyncSession());

            services.AddScoped<IOutBoxService, RavenOutBoxService>();
            services.AddScoped<IOutBoxStoreService, RavenOutBoxStoreService>();
            services
                .AddScoped<IRavenUnitOfWork>(sp => new RavenUnitOfWork(sp.GetRequiredService<IAsyncDocumentSession>()))
                .AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IRavenUnitOfWork>());

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
                .AddScoped(typeof(IRavenRepository<>), typeof(RavenRepository<>))
                .AddScoped(typeof(IRepository<>), typeof(RavenRepository<>))
                .AddScoped(typeof(ITransactionalRepository<>), typeof(RavenRepository<>))
                .AddScoped(typeof(ICrudRepository<>), typeof(RavenCrudRepository<>))
                ;

            // Crud services
            services
                .AddScoped(typeof(ICrudService<,>), typeof(RavenCrudService<,>))
                ;

            return services;
        }
    }
}