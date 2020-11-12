using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenStore.Application;
using OpenStore.Domain;

namespace OpenStore.Infrastructure.Data
{
    /// <summary>
    /// OutBox service abstraction to hold common outbox logic
    /// </summary>
    public abstract class OutBoxService : IOutBoxService
    {
        protected IEventNotifier EventNotifier { get; }
        protected IUnitOfWork Uow { get; }

        protected ILogger Logger { get; }

        protected OutBoxService(IUnitOfWork uow, IEventNotifier eventNotifier, ILogger logger)
        {
            EventNotifier = eventNotifier;
            Uow = uow;
            Logger = logger;
        }

        protected static IEnumerable<OutBoxMessage> WrapEvents(IEnumerable<IDomainEvent> events) => events.Select(e => new OutBoxMessage(e));

        public abstract Task StoreMessages(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);

        protected abstract Task<IReadOnlyCollection<OutBoxMessage>> GetPendingMessages(int take, CancellationToken cancellationToken = default);

        /// <summary>
        /// Return true if all pending messages published successfully otherwise return false in case of totally or partially fails
        /// </summary>
        /// <param name="take"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<bool> PushPendingMessages(int take, CancellationToken token)
        {
            var messagesToPush = await GetPendingMessages(take, token);
            Logger.LogInformation("Messages pending to push. Count: {}", messagesToPush.Count);

            await Uow.BeginTransactionAsync(token);
            var successCount = 0;

            foreach (var msg in messagesToPush)
            {
                if (await TryPush(msg, token))
                {
                    successCount++;
                }
            }

            if (successCount > 0)
            {
                try
                {
                    await Uow.SaveChangesAsync(token);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Messages commit failed");
                    return false;
                }
            }

            if (successCount == messagesToPush.Count) 
                return true;
            
            Logger.LogWarning($"Some of outbox messages not committed success: {successCount}, total: {messagesToPush.Count}");
            return false;

        }
        
        private async Task<bool> TryPush(OutBoxMessage message, CancellationToken token)
        {
            try
            {
                await PublishMessage(message);
                message.MarkAsCommitted();
                
                Logger.LogInformation($"Message published successfully {message}");
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Message publish failed {message}");
                return false;
            }
        }

        private async Task PublishMessage(OutBoxMessage message)
        {
            var recreate = message.RecreateMessage();

            if (recreate is IDomainEvent domainEvent)
            {
                await EventNotifier.Notify(domainEvent);
            }
            else
            {
                Logger.LogError( "Message could not be parsed to IDomainEvent {}", message);
            }
        }
    }
}