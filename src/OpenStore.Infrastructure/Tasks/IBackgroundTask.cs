namespace OpenStore.Infrastructure.Tasks;

public interface IBackgroundTask
{
    Task Run(IServiceProvider serviceProvider, CancellationToken cancellationToken);
}