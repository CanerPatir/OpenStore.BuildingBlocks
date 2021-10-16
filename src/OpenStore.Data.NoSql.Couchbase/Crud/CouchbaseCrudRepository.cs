using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Core.Exceptions;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;
using OpenStore.Application.Crud;
using OpenStore.Domain;

namespace OpenStore.Data.NoSql.Couchbase.Crud;

public class CouchbaseCrudRepository<TEntity> : ICrudRepository<TEntity>
    where TEntity : Entity<string>, ISavingChanges
{
    private readonly ICouchbaseCollection _collection;

    public CouchbaseCrudRepository(IBucketProvider bucketProvider, CouchbaseDatabaseSettings couchbaseDatabaseSettings)
    {
        Bucket = bucketProvider.GetBucketAsync(couchbaseDatabaseSettings.BucketName).ConfigureAwait(false).GetAwaiter().GetResult();
        _collection = Bucket.DefaultCollection();
    }

    public IBucket Bucket { get; }
    public IQueryable<TEntity> Query => throw new NotSupportedException();

    public async Task<TEntity> GetAsync(object id, CancellationToken cancellationToken = default)
    {
        var result = await _collection.GetAsync(id.ToString(), new GetOptions().CancellationToken(cancellationToken));

        var entity = result.ContentAs<TEntity>();
        entity.SetVersionExplicitly(Convert.ToInt64(result.Cas));

        return entity;
    }

    public async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _collection.InsertAsync(entity.Id, entity, new InsertOptions().CancellationToken(cancellationToken));
        return entity;
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            await _collection.ReplaceAsync(entity.Id, entity, new ReplaceOptions().Cas(Convert.ToUInt64(entity.Version)).CancellationToken(cancellationToken));
        }
        catch (CasMismatchException ex)
        {
            throw new ConcurrencyException(ex.Message, ex);
        }
    }

    public Task RemoveByIdAsync(object id, CancellationToken cancellationToken = default) => _collection.RemoveAsync(id.ToString());
        
    public void Remove(TEntity entity)
    {
        throw new NotSupportedException();
        _collection.RemoveAsync(entity.Id);
    }

    public void Attach(TEntity entity) => throw new NotSupportedException();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // todo: transaction support will be added soon
        return Task.CompletedTask;
    }
}