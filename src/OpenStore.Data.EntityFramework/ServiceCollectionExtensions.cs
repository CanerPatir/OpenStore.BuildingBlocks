using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Application;
using OpenStore.Application.Crud;
using OpenStore.Domain;
using OpenStore.Data.EntityFramework.Crud;
using OpenStore.Data.EntityFramework.OutBox;
using OpenStore.Data.OutBox;

// ReSharper disable MemberCanBePrivate.Global

namespace OpenStore.Data.EntityFramework
{
    /// <summary>
    /// config json example:
    ///   "ActiveConnection": "SqLite",
    ///   "ConnectionStrings": {
    ///       "SqLite": "Data Source=app.db"
    ///   },
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        private const int OutBoxFetchSize = 2000; // todo: make configurable
        
        public static EntityFrameworkDataSource GetActiveDataSource(this IConfiguration configuration)
        {
            var activeConnection = configuration["Data:ActiveConnection"];
            return Enum.Parse<EntityFrameworkDataSource>(activeConnection);
        }

        public static string GetActiveConnectionString(this IConfiguration configuration)
        {
            var dataSource = configuration.GetActiveDataSource();
            return configuration.GetConnectionString($"Data:{dataSource}");
        }

        internal static bool GetOutBoxEnabled(this IConfiguration configuration) => bool.Parse(configuration["Data:OutBoxEnabled"]);

        public static IServiceCollection AddOpenStoreEfCore<TDbContext, TDbContextImplementation>(this IServiceCollection services,
            IConfiguration configuration,
            string migrationAssembly = null,
            Action<DbContextOptionsBuilder> optionsBuilder = null,
            Assembly[] assemblies = null)
            where TDbContext : DbContext
            where TDbContextImplementation : TDbContext
        {
            var dataSource = configuration.GetActiveDataSource();
            var connStr = configuration.GetActiveConnectionString();
            var outBoxEnabled = configuration.GetOutBoxEnabled();

            return services.AddOpenStoreEfCore<TDbContext, TDbContextImplementation>(connStr, dataSource, outBoxEnabled, migrationAssembly, optionsBuilder);
        }

        public static IServiceCollection AddOpenStoreEfCore<TDbContext, TDbContextImplementation>(
            this IServiceCollection services,
            string connStr,
            EntityFrameworkDataSource dataSource,
            bool outBoxEnabled,
            string migrationAssembly = null,
            Action<DbContextOptionsBuilder> optionsBuilder = null,
            Assembly[] assemblies = null)
            where TDbContext : DbContext
            where TDbContextImplementation : TDbContext
        {
            services.AddDbContextPool<TDbContext, TDbContextImplementation>((sp, options) =>
            {
                switch (dataSource)
                {
                    case EntityFrameworkDataSource.SqLite:
                        options.UseSqlite(connStr, dbOpts =>
                        {
                            if (!string.IsNullOrWhiteSpace(migrationAssembly))
                            {
                                dbOpts.MigrationsAssembly(migrationAssembly);
                            }
                        });
                        break;
                    case EntityFrameworkDataSource.PostgreSql:
                        options.UseNpgsql(connStr, dbOpts =>
                        {
                            if (!string.IsNullOrWhiteSpace(migrationAssembly))
                            {
                                dbOpts.MigrationsAssembly(migrationAssembly);
                            }
                        });
                        break;
                    case EntityFrameworkDataSource.MySql:
                        options.UseMySql(connStr, ServerVersion.AutoDetect(connStr), dbOpts =>
                        {
                            if (!string.IsNullOrWhiteSpace(migrationAssembly))
                            {
                                dbOpts.MigrationsAssembly(migrationAssembly);
                            }
                        });
                        break;
                    case EntityFrameworkDataSource.MsSql:
                        options.UseSqlServer(connStr, dbOpts =>
                        {
                            if (!string.IsNullOrWhiteSpace(migrationAssembly))
                            {
                                dbOpts.MigrationsAssembly(migrationAssembly);
                            }
                        });
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                optionsBuilder?.Invoke(options);
            });

            AddOpenStoreEfCoreDefaults<TDbContext>(services, outBoxEnabled, assemblies);

            return services;
        }

        private static void AddOpenStoreEfCoreDefaults<TDbContext>(IServiceCollection services, bool outBoxEnabled, Assembly[] assemblies)
            where TDbContext : DbContext
        {
            // add repositories automatically

            assemblies ??= Array.Empty<Assembly>();

            var assemblyList = new List<Assembly>();
            assemblyList.AddRange(assemblies);
            assemblyList.Add(typeof(TDbContext).Assembly);

            services.Scan(scan =>
            {
                scan
                    .FromAssemblies(assemblyList)
                    //
                    .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                    ;

                scan
                    .FromAssemblies(assemblyList)
                    //
                    .AddClasses(classes => classes.AssignableTo(typeof(ICrudService<,>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                    ;

                scan
                    .FromAssemblies(assemblyList)
                    //
                    .AddClasses(classes => classes.AssignableTo(typeof(ICrudRepository<>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                    ;
            });

            //
            services
                .AddScoped<IOutBoxService, EntityFrameworkOutBoxService>()
                .AddScoped<IOutBoxStoreService, EntityFrameworkOutBoxStoreService<TDbContext>>(sp =>
                    new EntityFrameworkOutBoxStoreService<TDbContext>(outBoxEnabled, sp.GetRequiredService<TDbContext>(), sp.GetRequiredService<IOpenStoreUserContextAccessor>()))
                .AddScoped<IEntityFrameworkCoreUnitOfWork, EntityFrameworkUnitOfWork<TDbContext>>()
                .AddScoped<IUnitOfWork, EntityFrameworkUnitOfWork<TDbContext>>()
                ;
            
            services.AddHostedService(sp =>
            {
                var serviceScopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                return new OutBoxPollHost(outBoxEnabled, OutBoxFetchSize, serviceScopeFactory);
            });

            // for generic resolve
            services
                .AddScoped(typeof(IRepository<>), typeof(EntityFrameworkRepository<>))
                .AddScoped(typeof(IEntityFrameworkRepository<>), typeof(EntityFrameworkRepository<>))
                .AddScoped(typeof(ITransactionalRepository<>), typeof(EntityFrameworkRepository<>))
                .AddScoped(typeof(ICrudRepository<>), typeof(EntityFrameworkCrudRepository<>))
                ;

            // Crud services
            services.AddScoped(typeof(ICrudService<,>), typeof(EntityFrameworkCrudService<,>));
        }
    }
}