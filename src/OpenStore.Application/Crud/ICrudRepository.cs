using OpenStore.Domain;

namespace OpenStore.Application.Crud;

public interface ICrudRepository<TEntity>
    where TEntity : IEntity
{
    IQueryable<TEntity> Query { get; }
    Task<TEntity> GetAsync(object id, CancellationToken cancellationToken = default);
    Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task RemoveByIdAsync(object id, CancellationToken cancellationToken = default);
    Task Remove(TEntity entity);
    void Attach(TEntity entity);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}