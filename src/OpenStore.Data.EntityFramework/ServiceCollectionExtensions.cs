using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Application;
using OpenStore.Application.Crud;
using OpenStore.Domain;
using OpenStore.Data.EntityFramework.Crud;
using OpenStore.Data.EntityFramework.OutBox;
using OpenStore.Data.EntityFramework.ReadOnly;
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

    public static IServiceCollection AddOpenStoreEfCoreWithReadReplica<TDbContext, TDbContextImplementation, TReadDbContext, TReadDbContextImplementation>(
        this IServiceCollection services,
        IConfiguration configuration,
        string migrationAssembly = null,
        Action<DbContextOptionsBuilder> optionsBuilder = null,
        Assembly[] assemblies = null)
        where TDbContext : DbContext
        where TDbContextImplementation : TDbContext
        where TReadDbContext : DbContext, IReadOnlyDbContext
        where TReadDbContextImplementation : TReadDbContext
    {
        var openStoreEntityFrameworkConfiguration = configuration.GetSection(DataConfigurationSectionKey).Get<OpenStoreEntityFrameworkSettings>();

        return services.AddOpenStoreEfCoreWithReadReplica<TDbContext, TDbContextImplementation, TReadDbContext, TReadDbContextImplementation>(
            openStoreEntityFrameworkConfiguration,
            migrationAssembly,
            optionsBuilder,
            assemblies
        );
    }

    public static IServiceCollection AddOpenStoreEfCoreWithReadReplica<TDbContext, TDbContextImplementation, TReadDbContext, TReadDbContextImplementation>(
        this IServiceCollection services,
        OpenStoreEntityFrameworkSettings openStoreEntityFrameworkSettings,
        string migrationAssembly = null,
        Action<DbContextOptionsBuilder> optionsBuilder = null,
        Assembly[] assemblies = null)
        where TDbContext : DbContext
        where TDbContextImplementation : TDbContext
        where TReadDbContext : DbContext, IReadOnlyDbContext
        where TReadDbContextImplementation : TReadDbContext
    {
        services.ConfigureReadOnlyDbContext<TReadDbContext, TReadDbContextImplementation>(openStoreEntityFrameworkSettings, optionsBuilder);
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
            optionsBuilder,
            assemblies
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
        services.ConfigureDbContext<TDbContext, TDbContextImplementation>(openStoreEntityFrameworkSettings, migrationAssembly, optionsBuilder);
        services.ConfigureDataServices<TDbContext>(assemblies);
        services.ConfigureOutBox<TDbContext>(openStoreEntityFrameworkSettings);

        return services;
    }

    #region privates

    private static void ConfigureDbContext<TDbContext, TDbContextImplementation>(this IServiceCollection services,
        OpenStoreEntityFrameworkSettings openStoreEntityFrameworkSettings,
        string migrationAssembly,
        Action<DbContextOptionsBuilder> optionsBuilder)
        where TDbContext : DbContext
        where TDbContextImplementation : TDbContext
    {
        services.AddDbContextPool<TDbContext, TDbContextImplementation>((sp, options) =>
        {
            var connectionString = openStoreEntityFrameworkSettings.ConnectionStrings.Default;
            if (connectionString is null)
            {
                throw new InvalidOperationException("Default connection string could not be null or empty.");
            }

            SelectProvider(options, connectionString, openStoreEntityFrameworkSettings.ActiveConnection, migrationAssembly);

            optionsBuilder?.Invoke(options);
        });
    }

    private static void ConfigureReadOnlyDbContext<TReadOnlyDbContext, TReadOnlyDbContextImplementation>(this IServiceCollection services,
        OpenStoreEntityFrameworkSettings openStoreEntityFrameworkSettings,
        Action<DbContextOptionsBuilder> optionsBuilder)
        where TReadOnlyDbContext : DbContext, IReadOnlyDbContext
        where TReadOnlyDbContextImplementation : TReadOnlyDbContext
    {
        services.AddDbContextPool<TReadOnlyDbContext, TReadOnlyDbContextImplementation>((sp, options) =>
        {
            var connectionString = openStoreEntityFrameworkSettings.ConnectionStrings.ReadReplica;
            if (connectionString is null)
            {
                throw new InvalidOperationException($"You are trying to configure read replicate database configuration but read replicate connection string wasn't set. " +
                                                    $"Either set {DataConfigurationSectionKey}:{nameof(openStoreEntityFrameworkSettings.ConnectionStrings)}:{nameof(OpenStoreEntityFrameworkSettingsConnectionStrings.ReadReplica)}" +
                                                    $" or call {nameof(AddOpenStoreEfCoreWithReadReplica)}<TDbContext, TDbContextImplementation> overload.");
            }

            SelectProvider(options, connectionString, openStoreEntityFrameworkSettings.ActiveConnection, null);

            optionsBuilder?.Invoke(options);
        });

        services.AddScoped<IReadOnlyDbContext>(sp => sp.GetRequiredService<TReadOnlyDbContextImplementation>());

        if (typeof(TReadOnlyDbContextImplementation).IsAssignableTo(typeof(ReadOnlyDbContext)))
        {
            services.AddScoped<ReadOnlyDbContext>(sp => sp.GetRequiredService<TReadOnlyDbContextImplementation>() as ReadOnlyDbContext);
        }
    }

    private static void SelectProvider(DbContextOptionsBuilder options, string connectionString, EntityFrameworkDataSource activeDataSource, string migrationAssembly)
    {
        switch (activeDataSource)
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

            scan
                .FromAssemblies(assemblyList)
                //
                .AddClasses(classes => classes.AssignableTo(typeof(IReadOnlyRepository<>)))
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
            .AddScoped(typeof(IReadOnlyRepository<>), typeof(EntityFrameworkReadOnlyRepository<>))
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