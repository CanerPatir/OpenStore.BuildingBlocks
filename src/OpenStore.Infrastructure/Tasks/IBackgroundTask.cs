using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenStore.Infrastructure.Tasks
{
    public interface IBackgroundTask
    {
        Task Run(IServiceProvider serviceProvider, CancellationToken cancellationToken);
    }
}