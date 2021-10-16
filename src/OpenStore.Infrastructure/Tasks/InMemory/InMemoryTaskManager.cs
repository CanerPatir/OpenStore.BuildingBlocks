using System.Threading.Channels;

namespace OpenStore.Infrastructure.Tasks.InMemory;

public class InMemoryTaskManager : ITaskManager
{
    private readonly ChannelWriter<Func<IServiceProvider, CancellationToken, Task>> _queuedJobItems;

    public InMemoryTaskManager(ChannelWriter<Func<IServiceProvider, CancellationToken, Task>> queuedJobItems)
    {
        _queuedJobItems = queuedJobItems;
    }

    public async void QueueTask(Func<IServiceProvider, CancellationToken, Task> workItem)
    {
        if (workItem == null) throw new ArgumentNullException(nameof(workItem));

        await _queuedJobItems.WriteAsync(workItem);
    }

    public void QueueTask(IBackgroundTask task) => QueueTask(task.Run);

    public Task<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}