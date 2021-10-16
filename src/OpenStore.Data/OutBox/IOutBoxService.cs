// ReSharper disable SuspiciousTypeConversion.Global

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenStore.Data.OutBox;

public interface IOutBoxService
{
    Task<IReadOnlyCollection<OutBoxMessage>> FetchPendingMessages(int take, CancellationToken cancellationToken = default);

    Task<bool> NotifyPendingMessages(int take, CancellationToken token = default);
}