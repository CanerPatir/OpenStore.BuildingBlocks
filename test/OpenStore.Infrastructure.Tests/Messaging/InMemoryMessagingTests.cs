using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommonFixtures;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Infrastructure.Messaging;
using OpenStore.Infrastructure.Messaging.InMemory;
using Xunit;

namespace OpenStore.Infrastructure.Tests.Messaging;

public class InMemoryMessagingTests : WithHost
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.Register(this);
        services.AddInMemoryProducer();
        services.AddInMemoryConsumer<TestMessageConsumer, TestMessage>();
        services.AddInMemoryConsumer<TestMessageConsumerWithRetry, TestMessageWithRetry>(1);
        base.ConfigureServices(services);
    }

    public class TestMessage
    {
        public TestMessage(string message)
        {
            Message = message;
        }

        public string Message { get;}
    }

    class TestMessageConsumer : IOpenStoreConsumer<TestMessage>
    {
        private readonly InMemoryMessagingTests _testInstance;

        public TestMessageConsumer(InMemoryMessagingTests testInstance)
        {
            _testInstance = testInstance;
        }
        public Task Consume(TestMessage message, CancellationToken cancellationToken)
        {
            _testInstance.assertionItems.Add(message);
            Interlocked.Increment(ref _testInstance.assertionCounter);
                
            return Task.CompletedTask;
        }
    }
        
    public class TestMessageWithRetry
    {
        public TestMessageWithRetry(string message)
        {
            Message = message;
        }

        public string Message { get;}
    }
        
    class TestMessageConsumerWithRetry : IOpenStoreConsumer<TestMessageWithRetry>
    {
        private readonly InMemoryMessagingTests _testInstance;

        public TestMessageConsumerWithRetry(InMemoryMessagingTests testInstance)
        {
            _testInstance = testInstance;
        }
        public Task Consume(TestMessageWithRetry message, CancellationToken cancellationToken)
        {
                
            Interlocked.Increment(ref _testInstance.assertionCounter);
            throw new Exception();
        }
    }
        
    public int assertionCounter = 0;
    public List<TestMessage> assertionItems = new List<TestMessage>();

    [Fact]
    public async Task It_should_process_messages_with_single_consumer()
    {
        // Arrange
        var producer = GetService<IOpenStoreProducer>();
            
        // Act
        await producer.Produce("", new TestMessage("First"), CancellationToken.None);
        await producer.Produce("", new TestMessage("Second"), CancellationToken.None);

        // Assert
        await Task.Delay(100);
        Assert.Equal(2, assertionCounter);
        var first = assertionItems[0];
        var second = assertionItems[1];
        Assert.Equal("First", first.Message);
        Assert.Equal("Second", second.Message);
    }
        
    [Fact]
    public async Task It_should_process_messages_with_retry_single_consumer()
    {
        // Arrange
        var producer = GetService<IOpenStoreProducer>();
            
        // Act
        await producer.Produce("", new TestMessageWithRetry("First"), CancellationToken.None);

        // Assert
        await Task.Delay(3000);
        Assert.Equal(2, assertionCounter);
    }
        
    [Fact]
    public async Task It_should_process_batch_messages_with_single_consumer()
    {
        // Arrange
        var producer = GetService<IOpenStoreProducer>();
            
        // Act
        for (var i = 0; i < 2000; i++)
        {
            await producer.Produce("", new TestMessage(i.ToString()), CancellationToken.None);
        }
            
        // Assert
        await Task.Delay(200);
        Assert.Equal(2000, assertionCounter);
        Assert.Equal(2000, assertionItems.Count);
        Assert.Equal("0", assertionItems[0].Message);
        Assert.Equal("1999", assertionItems[1999].Message);
    }
}