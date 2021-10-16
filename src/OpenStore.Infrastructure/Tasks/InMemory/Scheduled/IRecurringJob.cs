namespace OpenStore.Infrastructure.Tasks.InMemory.Scheduled;

public interface IRecurringJob
{
    Task Execute(CancellationToken token);
}