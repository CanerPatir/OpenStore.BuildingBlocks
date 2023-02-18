using Microsoft.EntityFrameworkCore;

namespace OpenStore.Data.EntityFramework.ReadOnly;

public abstract class ReadOnlyDbContext : DbContext, IReadOnlyDbContext
{
    protected ReadOnlyDbContext()
    {
        ChangeTracker.AutoDetectChangesEnabled = false;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    protected ReadOnlyDbContext(DbContextOptions options) : base(options)
    {
        ChangeTracker.AutoDetectChangesEnabled = false;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public sealed override int SaveChanges(bool acceptAllChangesOnSuccess) => throw new InvalidOperationException("This context is read-only.");

    public sealed override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new()) =>
        throw new InvalidOperationException("This context is read-only.");
}