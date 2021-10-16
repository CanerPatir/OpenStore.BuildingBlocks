using OpenStore.Application;
using OpenStore.Domain;

namespace OpenStore.Data.EntityFramework;

public interface IEntityFrameworkRepository<TAggregateRoot> : ITransactionalRepository<TAggregateRoot> where TAggregateRoot : IAggregateRoot
{
    IEntityFrameworkCoreUnitOfWork EfUow { get; }
}