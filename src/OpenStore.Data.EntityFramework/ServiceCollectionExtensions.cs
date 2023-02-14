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

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace OpenStore.Data.EntityFramework;

/// <summary>
/// config json example:
///  Data: {
///    "ActiveConnection": "SqLite",
///    "ConnectionStrings": {
///      "Default": "Data Source=app.db",
///      "ReadReplica": null
///    },
///    "OutBoxEnabled": true,
///    "OutBoxFetchSize": 2000
///  }
/// </summary>
public static class ServiceCollectionExtensions
{
    private const string DataConfigurationSectionKey = "Data";

    public static EntityFrameworkDataSource GetActiveDataSource(this IConfiguration configuration)
    {
        var activeConnection = configuration[$"{DataConfigurationSectionKey}:{nameof(OpenStoreEntityFrameworkSettings.ActiveConnection)}"];
        return Enum.Parse<EntityFrameworkDataSource>(activeConnection);
    }

    public static IServiceCollection AddOpenStoreEfCore<TDbContext, TDbContextImplementation, TReadDbContext, TReadDbContextImplementation>(this IServiceCollection services,
        IConfiguration configuration,
        string migrationAssembly = null,
        Action<DbContextOptionsBuilder> optionsBuilder = null,
        Assembly[] assemblies = null)
        where TDbContext : DbContext
        where TDbContextImplementation : TDbContext
        where TReadDbContext : DbContext
        where TReadDbContextImplementation : TReadDbContext
    {
        var openStoreEntityFrameworkConfiguration = configuration.GetSection(DataConfigurationSectionKey).Get<OpenStoreEntityFrameworkSettings>();

        return services.AddOpenStoreEfCore<TDbContext, TDbContextImplementation, TReadDbContext, TReadDbContextImplementation>(
            openStoreEntityFrameworkConfiguration,
            migrationAssembly,
            optionsBuilder
        );
    }

    public static IServiceCollection AddOpenStoreEfCore<TDbContext, TDbContextImplementation, TReadDbContext, TReadDbContextImplementation>(this IServiceCollection services,
        OpenStoreEntityFrameworkSettings openStoreEntityFrameworkSettings,
        string migrationAssembly = null,
        Action<DbContextOptionsBuilder> optionsBuilder = null,
        Assembly[] assemblies = null)
        where TDbContext : DbContext
        where TDbContextImplementation : TDbContext
        where TReadDbContext : DbContext
        where TReadDbContextImplementation : TReadDbContext
    {
        if (string.IsNullOrWhiteSpace(openStoreEntityFrameworkSettings.ConnectionStrings.ReadReplica))
        {
            throw new InvalidOperationException($"You are trying to configure read replicate database configuration but read replicate connection string wasn't set. " +
                                                $"Either set {DataConfigurationSectionKey}:{nameof(openStoreEntityFrameworkSettings.ConnectionStrings)}:{nameof(OpenStoreEntityFrameworkSettingsConnectionStrings.ReadReplica)}" +
                                                $" or call {nameof(AddOpenStoreEfCore)}<TDbContext, TDbContextImplementation> overload.");
        }

        services.ConfigureDbContext<TReadDbContext, TReadDbContextImplementation>(
            openStoreEntityFrameworkSettings.ConnectionStrings.ReadReplica,
            openStoreEntityFrameworkSettings.ActiveConnection,
            null,
            optionsBuilder);

        services.AddOpenStoreEfCore<TDbContext, TDbContextImplementation>(openStoreEntityFrameworkSettings, migrationAssembly, optionsBuilder, assemblies);

        return services;
    }

    public static IServiceCollection AddOpenStoreEfCore<TDbContext, TDbContextImplementation>(this IServiceCollection services,
        IConfiguration configuration,
        string migrationAssembly = null,
        Action<DbContextOptionsBuilder> optionsBuilder = null,
        Assembly[] assemblies = null)
        where TDbContext : DbContext
        where TDbContextImplementation : TDbContext
    {
        var openStoreEntityFrameworkConfiguration = configuration.GetSection(DataConfigurationSectionKey).Get<OpenStoreEntityFrameworkSettings>();

        return services.AddOpenStoreEfCore<TDbContext, TDbContextImplementation>(
            openStoreEntityFrameworkConfiguration,
            migrationAssembly,
            optionsBuilder
        );
    }

    public static IServiceCollection AddOpenStoreEfCore<TDbContext, TDbContextImplementation>(this IServiceCollection services,
        OpenStoreEntityFrameworkSettings openStoreEntityFrameworkSettings,
        string migrationAssembly = null,
        Action<DbContextOptionsBuilder> optionsBuilder = null,
        Assembly[] assemblies = null)
        where TDbContext : DbContext
        where TDbContextImplementation : TDbContext
    {
        services.ConfigureDbContext<TDbContext, TDbContextImplementation>(openStoreEntityFrameworkSettings.ConnectionStrings.Default,
            openStoreEntityFrameworkSettings.ActiveConnection, migrationAssembly, optionsBuilder);
        services.ConfigureDataServices<TDbContext>(assemblies);
        services.ConfigureOutBox<TDbContext>(openStoreEntityFrameworkSettings);

        return services;
    }

    #region privates

    private static void ConfigureDbContext<TDbContext, TDbContextImplementation>(this IServiceCollection services,
        string connectionString,
        EntityFrameworkDataSource activeConnection,
        string migrationAssembly,
        Action<DbContextOptionsBuilder> optionsBuilder)
        where TDbContext : DbContext
        where TDbContextImplementation : TDbContext
    {
        services.AddDbContextPool<TDbContext, TDbContextImplementation>((sp, options) =>
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Default connection string could not be null or empty.");
            }

            switch (activeConnection)
            {
                case EntityFrameworkDataSource.SqLite:
                    options.UseSqlite(connectionString, dbOpts =>
                    {
                        if (!string.IsNullOrWhiteSpace(migrationAssembly))
                        {
                            dbOpts.MigrationsAssembly(migrationAssembly);
                        }
                    });
                    break;
                case EntityFrameworkDataSource.PostgreSql:
                    options.UseNpgsql(connectionString, dbOpts =>
                    {
                        if (!string.IsNullOrWhiteSpace(migrationAssembly))
                        {
                            dbOpts.MigrationsAssembly(migrationAssembly);
                        }
                    });
                    break;
                case EntityFrameworkDataSource.MySql:
                    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), dbOpts =>
                    {
                        if (!string.IsNullOrWhiteSpace(migrationAssembly))
                        {
                            dbOpts.MigrationsAssembly(migrationAssembly);
                        }
                    });
                    break;
                case EntityFrameworkDataSource.MsSql:
                    options.UseSqlServer(connectionString, dbOpts =>
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
    }

    private static void ConfigureDataServices<TDbContext>(this IServiceCollection services, Assembly[] assemblies)
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

        services
            .AddScoped<IEntityFrameworkCoreUnitOfWork, EntityFrameworkUnitOfWork<TDbContext>>()
            .AddScoped<IUnitOfWork, EntityFrameworkUnitOfWork<TDbContext>>()
            ;

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

    private static void ConfigureOutBox<TDbContext>(this IServiceCollection services,
        OpenStoreEntityFrameworkSettings openStoreEntityFrameworkSettings)
        where TDbContext : DbContext
    {
        var outBoxEnabled = openStoreEntityFrameworkSettings.OutBoxEnabled;
        if (outBoxEnabled)
        {
            services
                .AddScoped<IOutBoxService, EntityFrameworkOutBoxService>()
                .AddScoped<IOutBoxStoreService, EntityFrameworkOutBoxStoreService>(sp =>
                    new EntityFrameworkOutBoxStoreService(
                        sp.GetRequiredService<TDbContext>(),
                        sp.GetRequiredService<IOpenStoreUserContextAccessor>()
                    )
                );

            services.AddHostedService(sp =>
            {
                var serviceScopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                return new OutBoxPollHost(openStoreEntityFrameworkSettings.OutBoxFetchSize, serviceScopeFactory);
            });
        }
        else
        {
            services
                .AddScoped<IOutBoxStoreService, NullOutBoxStoreService>()
                .AddScoped<IOutBoxService, NullOutBoxService>();
        }
    }

    #endregion
}