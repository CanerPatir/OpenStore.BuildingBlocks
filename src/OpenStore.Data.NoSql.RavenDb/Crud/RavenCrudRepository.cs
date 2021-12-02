using OpenStore.Application.Crud;
using OpenStore.Domain;
using Raven.Client.Documents.Linq;

namespace OpenStore.Data.NoSql.RavenDb.Crud;

public class RavenCrudRepository<TEntity> : ICrudRepository<TEntity>
    where TEntity : IEntity
{
    public RavenCrudRepository(IRavenUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork;
    }

    private IRavenUnitOfWork UnitOfWork { get; }

    public IQueryable<TEntity> Query => RavenQuery();

    public async Task<TEntity> GetAsync(object id, CancellationToken cancellationToken = default)
    {
        var entity = await UnitOfWork.Session.LoadAsync<TEntity>(id.ToString(), cancellationToken);
        return entity;
    }

    public async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await UnitOfWork.Session.StoreAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) => UnitOfWork.Session.StoreAsync(entity, cancellationToken);

    public Task RemoveByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        UnitOfWork.Session.Delete(id.ToString());
        return Task.CompletedTask;
    }

    public void Remove(TEntity entity) => UnitOfWork.Session.Delete(entity);

    public void Attach(TEntity entity) => throw new NotSupportedException();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => UnitOfWork.SaveChangesAsync(cancellationToken);

    private IRavenQueryable<TEntity> RavenQuery(string indexName = null, string collectionName = null, bool isMapReduce = false)
        => UnitOfWork.Session.Query<TEntity>(indexName, collectionName, isMapReduce);
}