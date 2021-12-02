using OpenStore.Domain;

namespace OpenStore.Data.OutBox;

public class NullOutBoxStoreService : IOutBoxStoreService
{
    public Task StoreMessages(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default) => Task.CompletedTask;
}