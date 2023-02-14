using OpenStore.Domain;

namespace OpenStore.Application.Crud;

public interface IReadableRepository<TEntity> where TEntity : IEntity
{
    IQueryable<TEntity> Query { get; }
    Task<TEntity> GetAsync(object id, CancellationToken cancellationToken = default);
}