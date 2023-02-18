namespace OpenStore.Data.EntityFramework.ReadOnly;

public interface IReadOnlyDbContext
{
    public sealed int SaveChanges(bool acceptAllChangesOnSuccess) => throw new InvalidOperationException("This context is read-only.");

    public sealed Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new()) =>
        throw new InvalidOperationException("This context is read-only.");
}