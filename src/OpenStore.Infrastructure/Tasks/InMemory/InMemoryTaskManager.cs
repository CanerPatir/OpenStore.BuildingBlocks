using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using OpenStore.Infrastructure.Tasks.InMemory.Scheduled;

namespace OpenStore.Infrastructure.Tasks.InMemory
{
    public class InMemoryTaskManager : ITaskManager
    {
        internal ConcurrentBag<ScheduledJobDescriptor> ScheduledJobItems { get; } = new ConcurrentBag<ScheduledJobDescriptor>();

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
        
        public void Schedule(IBackgroundTask task, string cronExpression) => Schedule(task.Run, cronExpression);

        public void Schedule(Func<IServiceProvider, CancellationToken, Task> workItem, string cronExpression)
        {
            ScheduledJobItems.Add(new ScheduledJobDescriptor(workItem, cronExpression));
        }
    }
}