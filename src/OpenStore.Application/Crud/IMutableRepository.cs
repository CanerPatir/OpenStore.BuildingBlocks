using OpenStore.Domain;

namespace OpenStore.Application.Crud;

public interface IMutableRepository<TEntity> where TEntity : IEntity
{
    Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Attach(TEntity entity);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}