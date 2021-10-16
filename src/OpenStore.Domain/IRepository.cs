using System.Threading;
using System.Threading.Tasks;

namespace OpenStore.Domain;

public interface IRepository<TAggregateRoot>
    where TAggregateRoot : IAggregateRoot
{
    Task<TAggregateRoot> GetAsync(object id, CancellationToken token = default);
    Task SaveAsync(TAggregateRoot aggregateRoot, CancellationToken token = default);
    Task Delete(TAggregateRoot aggregateRoot, CancellationToken token = default);
}