namespace OpenStore.Infrastructure.Tasks;

public interface ITaskManager
{
    void QueueTask(Func<IServiceProvider, CancellationToken, Task> workItem);
    void QueueTask(IBackgroundTask task);
    Task<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
}