namespace OpenStore.Domain;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken token = default);
    Task BeginTransactionAsync(CancellationToken token = default);
}