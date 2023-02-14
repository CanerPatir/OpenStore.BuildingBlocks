using OpenStore.Domain;

namespace OpenStore.Application.Crud;

public interface IRemovableRepository<in TEntity> where TEntity : IEntity
{
    Task RemoveByIdAsync(object id, CancellationToken cancellationToken = default);
    Task Remove(TEntity entity);
}