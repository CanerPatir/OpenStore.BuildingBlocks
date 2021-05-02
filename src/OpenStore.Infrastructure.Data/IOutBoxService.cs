using System.Threading;
using System.Threading.Tasks;

// ReSharper disable SuspiciousTypeConversion.Global

namespace OpenStore.Infrastructure.Data
{
    public interface IOutBoxService
    {
        Task<bool> PushPendingMessages(int take, CancellationToken token = default);
    }
}