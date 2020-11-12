using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using OpenStore.Application;
using OpenStore.Domain;

namespace OpenStore.Infrastructure.Data.NoSql.MongoDb
{
    public class MongoOutBoxService : OutBoxService
    {
        private readonly IMongoUnitOfWork _uow;
        private readonly IMongoCollection<OutBoxMessage> _collection;

        public MongoOutBoxService(IMongoUnitOfWork uow, IEventNotifier eventNotifier, ILogger<MongoOutBoxService> logger) : base(uow, eventNotifier, logger)
        {
            _uow = uow;
            _collection = _uow.DatabaseBase.GetCollection<OutBoxMessage>(nameof(OutBoxMessage));
        }

        public override async Task StoreMessages(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
        {
            var outBoxMessages = WrapEvents(events).ToList();
            await _collection.InsertManyAsync(_uow.Session, outBoxMessages, new InsertManyOptions(), cancellationToken);
        }

        protected override async Task<IReadOnlyCollection<OutBoxMessage>> GetPendingMessages(int take, CancellationToken cancellationToken = default)
        {
            var messages = await _collection
                .AsQueryable()
                .Where(x => !x.Committed)
                .ToListAsync(cancellationToken: cancellationToken);

            return messages.OrderBy(x => x.Version).ToList();
        }
    }
}