using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OpenStore.Domain;
using OpenStore.Domain.EventSourcing;
using CommonFixtures;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Embedded;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Data.EventSourcing.EventStore;
using OpenStore.Infrastructure;
using Xunit;

namespace OpenStore.Data.EventStore.Tests;

public class EventStoreTests : WithIoC
{
    public record StockCreate(string Id) : DomainEvent(Id), INotification;

    public class StockCreatedNotifHandler : INotificationHandler<StockCreate>
    {
        public Task Handle(StockCreate notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class Stock : EventSourcedAggregateRoot
    {
        public Stock()
        {
        }

        public static Stock Create(Guid id)
        {
            var stock = new Stock
            {
                Id = id.ToString()
            };
            stock.ApplyChange(new StockCreate(stock.Id));
                
            return stock;
        }

        [AggregateEventApplier]
        public void Create(StockCreate @event)
        {
            Id = @event.ToString();
        }
    }

    public class StockSnapshot : ISnapshot
    {
        public int Quantity { get; set; }
        public object AggregateId { get; }
        public long Version { get; }
    }

    public EventStoreTests()
    {
        // // initializing embedded event store
        // var nodeBuilder = EmbeddedVNodeBuilder.AsSingleNode()
        //     .OnDefaultEndpoints()
        //     .RunInMemory();
        // var node = nodeBuilder.Build();
        // using(var embeddedConn = EmbeddedEventStoreConnection.Create(node))
        // {
        //     embeddedConn.ConnectAsync().Wait();
        //     embeddedConn.AppendToStreamAsync("testStream", ExpectedVersion.Any,
        //         new EventData(Guid.NewGuid(), "eventType", true, Encoding.UTF8.GetBytes("{\"Foo\":\"Bar\"}"), null)).Wait();
        // }
       
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        var nodeBuilder = EmbeddedVNodeBuilder.AsSingleNode()
            .OnDefaultEndpoints()
            .RunInMemory();
        var node = nodeBuilder.Build();
            
        services.AddOpenStoreCore(typeof(EventStoreTests).Assembly);
        services.AddEventStoreDataInfrastructure(settings => { });
        services.AddSingleton<Func<IEventStoreConnection>>(sp => () => EmbeddedEventStoreConnection.Create(node));
    }

    [Fact]
    public void DiResolve()
    {
        // Arrange

        // Act

        var tRepo = GetService<IEventSourcingRepository<Stock, StockSnapshot>>();
        var snapshotStorageProvider = GetService<ISnapshotStorageProvider<Stock, StockSnapshot>>();
        var eventStorageProvider = GetService<IEventStorageProvider<Stock>>();
        var session = GetService<ISession<Stock, StockSnapshot>>();

        // Assert 
        Assert.NotNull(tRepo);
        Assert.NotNull(snapshotStorageProvider);
        Assert.NotNull(eventStorageProvider);
        Assert.NotNull(session);
    }
        
    [Fact(Skip = "todo: ")]
    public async Task Create()
    {
        // Arrange
        var repo = GetService<IEventSourcingRepository<Stock, StockSnapshot>>();

        var stockId = Guid.NewGuid();
        var stock = Stock.Create(stockId);
        await repo.SaveAsync(stock);

        // Act

        var loadedStock = await repo.GetAsync(stockId);

        // Assert
        Assert.Equal(stock, loadedStock);
    }
}