using System;
using System.Collections.Generic;
using System.Linq;
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

            if (await TryNotify(messagesToPush, token))
            {
                try
                {
                    await Uow.SaveChangesAsync(token);
                    return true;
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Messages commit failed");
                    return false;
                }
            }

            return false;
        }

        private async Task<bool> TryNotify(IReadOnlyCollection<OutBoxMessage> messages, CancellationToken token)
        {
            try
            {
                await _mediator.Publish(new OutBoxMessageBatch(messages), token);
                Logger.LogInformation($"Message batch published successfully Count: {messages.Count}");

                foreach (var outBoxMessage in messages)
                {
                    outBoxMessage.MarkAsCommitted();
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Message batch could not be published Count: {messages.Count}");
                return false;
            }
        }
    }
}