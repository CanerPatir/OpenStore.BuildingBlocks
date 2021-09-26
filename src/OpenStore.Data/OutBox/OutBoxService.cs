using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenStore.Domain;

namespace OpenStore.Data.OutBox
{
    /// <summary>
    /// OutBox service abstraction to hold common outbox logic
    /// </summary>
    public abstract class OutBoxService : IOutBoxService
    {
        protected IUnitOfWork Uow { get; }
        protected ILogger Logger { get; }

        private readonly IMediator _mediator;
        
        protected OutBoxService(IUnitOfWork uow, IMediator mediator, ILogger logger)
        {
            _mediator = mediator;
            Uow = uow;
            Logger = logger;
        }

        public abstract Task<IReadOnlyCollection<OutBoxMessage>> FetchPendingMessages(int take, CancellationToken cancellationToken = default);

        /// <summary>
        /// Return true if all pending messages published successfully otherwise return false in case of totally or partially fails
        /// </summary>
        /// <param name="take"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<bool> NotifyPendingMessages(int take, CancellationToken token)
        {
            var messagesToPush = await FetchPendingMessages(take, token);
            Logger.LogInformation("Messages pending to push. Count: {}", messagesToPush.Count);

            await Uow.BeginTransactionAsync(token);
            var successCount = 0;

            foreach (var msg in messagesToPush)
            {
                if (await TryNotify(msg, token))
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

        private async Task<bool> TryNotify(OutBoxMessage message, CancellationToken token)
        {
            try
            {
                await NotifyMessage(message);
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

        private async Task NotifyMessage(OutBoxMessage message) => await _mediator.Publish(message);
    }
}