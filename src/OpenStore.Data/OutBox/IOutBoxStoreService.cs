using OpenStore.Domain;

namespace OpenStore.Data.OutBox;

public interface IOutBoxStoreService
{
    Task StoreMessages(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
}