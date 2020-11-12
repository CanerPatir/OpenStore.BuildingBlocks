using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Application;
using OpenStore.Application.Crud;
using OpenStore.Domain;
using OpenStore.Infrastructure.Data.EntityFramework.Crud;

namespace OpenStore.Infrastructure.Data.EntityFramework
{
    public static class ServiceCollectionExtensions
    {
        public static EntityFrameworkDataSource GetActiveDataSource(this IConfiguration configuration)
        {
            var activeConnection = configuration["ActiveConnection"];
            return Enum.Parse<EntityFrameworkDataSource>(activeConnection);
        }

        public static string GetActiveConnectionString(this IConfiguration configuration)
        {
            var dataSource = configuration.GetActiveDataSource();
            return configuration.GetConnectionString(dataSource.ToString());
        }

        public static IServiceCollection AddOpenStoreEfCore<TDbContext, TDbContextImplementation>(this IServiceCollection services, IConfiguration configuration, Action<DbContextOptionsBuilder> optionsBuilder = null)
            where TDbContext : DbContext
            where TDbContextImplementation : TDbContext
        {
            var dataSource = configuration.GetActiveDataSource();
            var connStr = configuration.GetActiveConnectionString();

            return services.AddOpenStoreEfCore<TDbContext, TDbContextImplementation>(connStr, dataSource, optionsBuilder);
        }
        
        public static IServiceCollection AddOpenStoreEfCore<TDbContext, TDbContextImplementation>(this IServiceCollection services, string connStr, EntityFrameworkDataSource dataSource,
            Action<DbContextOptionsBuilder> optionsBuilder = null)
            where TDbContext : DbContext
            where TDbContextImplementation : TDbContext
        {
            services.AddDbContextPool<TDbContext, TDbContextImplementation>(options =>
            {
                switch (dataSource)
                {
                    case EntityFrameworkDataSource.SqLite:
                        options.UseSqlite(connStr);
                        break;
                    case EntityFrameworkDataSource.PostgreSql:
                        options.UseNpgsql(connStr);
                        break;
                    case EntityFrameworkDataSource.MySql:
                        options.UseMySql(connStr, ServerVersion.AutoDetect(connStr));
                        break;
                    case EntityFrameworkDataSource.MsSql:
                        options.UseSqlServer(connStr);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                optionsBuilder?.Invoke(options);
            });

            AddOpenStoreEfCoreDefaults<TDbContext>(services);

            return services;
        }

        private static void AddOpenStoreEfCoreDefaults<TDbContext>(IServiceCollection services)
            where TDbContext : DbContext
        {
            // add repositories automatically
            services.Scan(scan =>
            {
                scan
                    .FromAssemblyOf<TDbContext>()
                    //
                    .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                    ;
                
                scan
                    .FromAssemblyOf<TDbContext>()
                    //
                    .AddClasses(classes => classes.AssignableTo(typeof(ICrudService<,>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                    ;
                
                scan
                    .FromAssemblyOf<TDbContext>()
                    //
                    .AddClasses(classes => classes.AssignableTo(typeof(ICrudRepository<>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                    ;
                
            });

            //
            services.AddScoped<IOutBoxService, EntityFrameworkOutBoxService>();
            services
                .AddScoped<IEntityFrameworkCoreUnitOfWork>(sp => new EntityFrameworkUnitOfWork<TDbContext>(sp.GetRequiredService<TDbContext>()))
                .AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IEntityFrameworkCoreUnitOfWork>());

            // for generic resolve
            services
                .AddScoped(typeof(IRepository<>), typeof(EntityFrameworkRepository<>))
                .AddScoped(typeof(IEntityFrameworkRepository<>), typeof(EntityFrameworkRepository<>))
                .AddScoped(typeof(ITransactionalRepository<>), typeof(EntityFrameworkRepository<>))
                .AddScoped(typeof(ICrudRepository<>), typeof(EntityFrameworkCrudRepository<>))
                ;

            // Crud services
            services
                .AddScoped(typeof(ICrudService<,>), typeof(EntityFrameworkCrudService<,>));
        }
    }
}