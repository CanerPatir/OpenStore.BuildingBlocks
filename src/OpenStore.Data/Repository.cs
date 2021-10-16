using OpenStore.Domain;

namespace OpenStore.Data;

/// <summary>
/// Base repository abstraction that implements
/// </summary>
/// <typeparam name="TAggregateRoot">Entity type</typeparam>
public abstract class Repository<TAggregateRoot> : IRepository<TAggregateRoot>
    where TAggregateRoot : class, IAggregateRoot
{
    public abstract Task<TAggregateRoot> GetAsync(object id, CancellationToken token = default);

    public abstract Task SaveAsync(TAggregateRoot aggregateRoot, CancellationToken token = default);
    public abstract Task Delete(TAggregateRoot aggregateRoot, CancellationToken token = default);
}