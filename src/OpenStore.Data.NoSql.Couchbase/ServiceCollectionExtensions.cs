using System;
using System.Linq;
using System.Reflection;
using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Application.Crud;
using OpenStore.Data.NoSql.Couchbase.Crud;
using OpenStore.Domain;

namespace OpenStore.Data.NoSql.Couchbase;

// todo: support outbox
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCouchbaseDataInfrastructure(this IServiceCollection services, Action<ClusterOptions> couchbaseSettingsBuilder, string bucketname, params Assembly[] assemblies)
    {
        services.Configure(couchbaseSettingsBuilder);
        services.AddCouchbase(couchbaseSettingsBuilder);
        return AddCouchbaseServices(services, bucketname, assemblies);
    }

    public static IServiceCollection AddCouchbaseDataInfrastructure(this IServiceCollection services, IConfiguration configuration, params Assembly[] assemblies)
    {
        services.Configure<CouchbaseDatabaseSettings>(configuration);
        services.AddCouchbase(configuration);
        return AddCouchbaseServices(services, configuration.GetValue<string>("BucketName"), assemblies);
    }

    private static IServiceCollection AddCouchbaseServices(IServiceCollection services, string bucketName, params Assembly[] assemblies)
    {
        services.AddSingleton<CouchbaseDatabaseSettings>(new CouchbaseDatabaseSettings(bucketName));
        // services.AddScoped<IOutBoxService, CouchbaseOutBoxService>();
        // services
        //     .AddScoped<ICouchbaseUnitOfWork>(sp => new CouchbaseUnitOfWork(sp.GetRequiredService<IBucketProvider>()))
        //     .AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ICouchbaseUnitOfWork>());

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
            .AddScoped(typeof(ICouchbaseRepository<>), typeof(CouchbaseRepository<>))
            .AddScoped(typeof(IRepository<>), typeof(CouchbaseRepository<>))
            .AddScoped(typeof(ICrudRepository<>), typeof(CouchbaseCrudRepository<>))
            // .AddScoped(typeof(ITransactionalRepository<>), typeof(CouchbaseRepository<>))
            // .AddScoped(typeof(IQueryableRepository<>), typeof(CouchbaseRepository<>))
            ;
            
        // Crud services
        services
            .AddScoped(typeof(ICrudService<,>), typeof(CouchbaseCrudService<,>))
            ;

        return services;
    }
}