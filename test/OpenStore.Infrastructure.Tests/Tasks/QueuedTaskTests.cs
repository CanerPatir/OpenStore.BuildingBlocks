using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Infrastructure.Tasks;
using OpenStore.Infrastructure.Tasks.InMemory;
using CommonFixtures;
using Xunit;

namespace OpenStore.Infrastructure.Tests.Tasks;

public class QueuedTaskTests : WithHost
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddOpenStoreInMemoryBackgroundTasks();
    }

    [Fact]
    public async Task It_should_execute_queued_tasks_sequentially()
    {
        // Arrange
        var taskManager = GetService<ITaskManager>();

        var forAssertion = new ConcurrentQueue<string>();
        var counter = 0;

        taskManager.QueueTask(async (sp, token) =>
        {
            forAssertion.Enqueue("First");
            await Task.Delay(200, token);
            Interlocked.Increment(ref counter);
        });
            
        taskManager.QueueTask( (sp, token) =>
        {
            forAssertion.Enqueue("Second");
            Interlocked.Increment(ref counter);
            return Task.CompletedTask;
        });

        // Assert
        await Task.Delay(300);
        Assert.Equal(2, counter);
        forAssertion.TryDequeue(out var first);
        forAssertion.TryDequeue(out var second);
        Assert.Equal("First", first);
        Assert.Equal("Second", second);
    }
        
}