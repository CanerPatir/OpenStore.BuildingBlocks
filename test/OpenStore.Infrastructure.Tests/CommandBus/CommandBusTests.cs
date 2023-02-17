using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Application;
using OpenStore.Infrastructure.CommandBus;
using Xunit;
using Xunit.Abstractions;

namespace OpenStore.Infrastructure.Tests.CommandBus;

public class CommandBusTests : CommonFixtures.WithIoC
{
    public static int Counter = 0;
    private readonly ITestOutputHelper _testOutputHelper;

    public CommandBusTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();
        services.AddOpenStoreCore(typeof(CommandBusTests).Assembly);
    }

    [Fact]
    public async Task NotifySuccessRequestTest()
    {
        var mediator = GetService<IMediator>();

        await mediator.Send(new NotifyingTestRequest());

        await Task.Delay(1000);

        Assert.Equal(1, Counter);
    }
}

public record NotifyingTestRequest : INotifySuccessRequest;

public class NotifyingTestRequestHandler : IRequestHandler<NotifyingTestRequest>
{
    public Task Handle(NotifyingTestRequest request, CancellationToken cancellationToken) => Task.Delay(400, cancellationToken);
}

public class RequestSuccessNotificationHandler : INotificationHandler<RequestSuccessNotification>
{
    public Task Handle(RequestSuccessNotification notification, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref CommandBusTests.Counter);
        return Task.CompletedTask;
    }
}