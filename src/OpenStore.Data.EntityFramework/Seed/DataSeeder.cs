using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OpenStore.Data.EntityFramework.Seed;

public class DataSeeder<TDbContext>
    where TDbContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataSeeder<TDbContext>> _logger;
    protected TDbContext Context { get; }

    private DataSeeder(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Context = serviceProvider.GetRequiredService<TDbContext>();
        _logger = serviceProvider.GetRequiredService<ILogger<DataSeeder<TDbContext>>>();
    }

    public static DataSeeder<TDbContext> Create(IServiceProvider serviceProvider) => new(serviceProvider);

    public Task Seed(Func<TDbContext, CancellationToken, Task> seedAction, CancellationToken cancellationToken) => Seed((c, p, t) => seedAction(c, t), cancellationToken);

    public async Task Seed(Func<TDbContext, IServiceProvider, CancellationToken, Task> seedAction, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"EfCore data seeding starting for {GetContextName()}");
        try
        {
            await Context.Database.EnsureCreatedAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, $"EnsureCreated failed for {GetContextName()}");
        }

        var pendingMigrations = await Context.Database.GetPendingMigrationsAsync(cancellationToken);
        try
        {
            if (pendingMigrations.Any())
                await Context.Database.MigrateAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, $"Migrate failed for {GetContextName()}");
        }

        try
        {
            await seedAction(Context, _serviceProvider, cancellationToken);
            _logger.LogInformation($"EfCore data seeding completed for {GetContextName()} successfully.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"EfCore data seeding completed for {GetContextName()} with error.");
            throw;
        }
    }

    private static string GetContextName() => typeof(TDbContext).Name;
}