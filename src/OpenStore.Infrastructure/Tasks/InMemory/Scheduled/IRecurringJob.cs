using System.Threading;
using System.Threading.Tasks;

namespace OpenStore.Infrastructure.Tasks.InMemory.Scheduled
{
    public interface IRecurringJob
    {
        Task Execute(CancellationToken token);
    }
}