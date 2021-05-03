using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using OpenStore.Application;
using OpenStore.Domain;

namespace OpenStore.Infrastructure.Data.NoSql.MongoDb
{
    public class MongoOutBoxStoreService : OutBoxStoreService
    {
        private readonly IMongoUnitOfWork _uow;
        private readonly IMongoCollection<OutBoxMessage> _collection;

        public MongoOutBoxStoreService(IMongoUnitOfWork uow, IOpenStoreUserContextAccessor openStoreUserContextAccessor) : base(openStoreUserContextAccessor)
        {
            _uow = uow;
            _collection = _uow.DatabaseBase.GetCollection<OutBoxMessage>(nameof(OutBoxMessage));
        }

        public override async Task StoreMessages(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
        {
            var outBoxMessages = WrapEvents(events).ToList();
            await _collection.InsertManyAsync(_uow.Session, outBoxMessages, new InsertManyOptions(), cancellationToken);
        }
    }
}