using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using OpenStore.Domain;

namespace OpenStore.Data.NoSql.MongoDb
{
    public class MongoRepository<TAggregateRoot> : Repository<TAggregateRoot>, IMongoRepository<TAggregateRoot>
        where TAggregateRoot : AggregateRoot<string>, ISavingChanges
    {
        private readonly IOutBoxStoreService _outBoxStoreService;

        public MongoRepository(IMongoUnitOfWork uow, IOutBoxStoreService outBoxStoreService)
        {
            _outBoxStoreService = outBoxStoreService;
            MongoUow = uow;
            MongoCollection = MongoUow.DatabaseBase.GetCollection<TAggregateRoot>(typeof(TAggregateRoot).Name);
        }

        public IMongoUnitOfWork MongoUow { get; }
        public IMongoCollection<TAggregateRoot> MongoCollection { get; }
        public IUnitOfWork Uow => MongoUow;
        public IQueryable<TAggregateRoot> Query => MongoQuery;
        public IMongoQueryable<TAggregateRoot> MongoQuery => MongoCollection.AsQueryable();

        public override async Task<TAggregateRoot> GetAsync(object id, CancellationToken token = default)
        {
            var entity = await MongoCollection.Find(x => x.Id == id.ToString()).SingleOrDefaultAsync(token);
            return entity;
        }

        public override async Task SaveAsync(TAggregateRoot aggregateRoot, CancellationToken token = default)
        {
            if (aggregateRoot == null) throw new ArgumentNullException(nameof(aggregateRoot));
            if (aggregateRoot.HasUncommittedChanges())
            {
                var events = aggregateRoot.GetUncommittedChanges();
                await _outBoxStoreService.StoreMessages(events, token);
            }

            if (aggregateRoot.Version == default)
            {
                aggregateRoot.OnSavingChanges();
                await MongoCollection.InsertOneAsync(MongoUow.Session, aggregateRoot, new InsertOneOptions(), token);
            }
            else
            {
                var version = aggregateRoot.Version;
                aggregateRoot.OnSavingChanges();
                var result = await MongoCollection.ReplaceOneAsync(MongoUow.Session, x => x.Id == aggregateRoot.Id && x.Version == version, aggregateRoot, new ReplaceOptions() {IsUpsert = false}, token);
                if (!result.IsAcknowledged || result.ModifiedCount == 0)
                {
                    throw new ApplicationException("Document could not be updated");
                }
            }

            await MongoUow.SaveChangesAsync(token);

            aggregateRoot.Commit();
        }

        public override Task Delete(TAggregateRoot aggregateRoot, CancellationToken token = default)
        {
            return MongoCollection.DeleteOneAsync(MongoUow.Session, x => x.Id == aggregateRoot.Id, new DeleteOptions(), token);
        }
    }
}