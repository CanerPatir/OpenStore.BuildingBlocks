namespace OpenStore.Data.OutBox;

public class NullOutBoxService : IOutBoxService
{
    public Task<IReadOnlyCollection<OutBoxMessage>> FetchPendingMessages(int take, CancellationToken cancellationToken = default)
        => Task.FromResult((IReadOnlyCollection<OutBoxMessage>)new List<OutBoxMessage>());

    public Task<bool> NotifyPendingMessages(int take, CancellationToken token = default) => Task.FromResult(true);
}