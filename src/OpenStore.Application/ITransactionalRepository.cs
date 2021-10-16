using OpenStore.Domain;

namespace OpenStore.Application;

/// <summary>
/// Abstraction for repository that supports transactional stores
/// </summary>
/// <typeparam name="TAggregateRoot"></typeparam>
public interface ITransactionalRepository<TAggregateRoot> : IRepository<TAggregateRoot>
    where TAggregateRoot : IAggregateRoot
{
    IUnitOfWork Uow { get; }
}