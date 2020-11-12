using System;
using System.Threading;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Core.Exceptions;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;
using OpenStore.Domain;
using ICouchbaseCollection = Couchbase.KeyValue.ICouchbaseCollection;

namespace OpenStore.Infrastructure.Data.NoSql.Couchbase
{
    /// <summary>
    /// Minimal couchbase repository that not supports transactions and outbox messages
    /// </summary>
    /// <typeparam name="TAggregateRoot"></typeparam>
    public class CouchbaseRepository<TAggregateRoot> : Repository<TAggregateRoot>, ICouchbaseRepository<TAggregateRoot>
        where TAggregateRoot : AggregateRoot<string>
    {
        public IBucket Bucket { get; }

        private readonly ICouchbaseCollection _collection;

        public CouchbaseRepository(IBucketProvider bucketProvider, CouchbaseDatabaseSettings couchbaseDatabaseSettings)
        {
            Bucket = bucketProvider.GetBucketAsync(couchbaseDatabaseSettings.BucketName).ConfigureAwait(false).GetAwaiter().GetResult();
            _collection = Bucket.DefaultCollection();
        }

        public override async Task<TAggregateRoot> GetAsync(object id, CancellationToken token = default)
        {
            var result = await _collection.GetAsync(id.ToString(), new GetOptions().CancellationToken(token));

            var entity = result.ContentAs<TAggregateRoot>();
            entity.Version = result.Cas;

            return entity;
        }

        public override async Task SaveAsync(TAggregateRoot aggregateRoot, CancellationToken token = default)
        {
            if (aggregateRoot == null) throw new ArgumentNullException(nameof(aggregateRoot));

            if (aggregateRoot.Version == default)
            {
                await _collection.InsertAsync(aggregateRoot.Id, aggregateRoot, new InsertOptions().CancellationToken(token));
            }
            else
            {
                try
                {
                    await _collection.ReplaceAsync(aggregateRoot.Id, aggregateRoot, new ReplaceOptions().Cas(aggregateRoot.Version).CancellationToken(token));
                }
                catch (CasMismatchException ex)
                {
                    throw new ConcurrencyException(ex.Message, ex);
                }
            }
        }

        public override async Task Delete(TAggregateRoot aggregateRoot, CancellationToken token = default)
        {
            await _collection.RemoveAsync(aggregateRoot.Id, new RemoveOptions().Cas(aggregateRoot.Version).CancellationToken(token));
        }
    }
}