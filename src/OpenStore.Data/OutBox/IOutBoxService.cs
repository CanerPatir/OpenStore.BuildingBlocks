// ReSharper disable SuspiciousTypeConversion.Global

namespace OpenStore.Data.OutBox;

public interface IOutBoxService
{
    Task<IReadOnlyCollection<OutBoxMessage>> FetchPendingMessages(int take, CancellationToken cancellationToken = default);

    Task<bool> NotifyPendingMessages(int take, CancellationToken token = default);
}