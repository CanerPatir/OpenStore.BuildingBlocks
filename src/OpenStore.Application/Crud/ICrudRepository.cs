using OpenStore.Domain;

namespace OpenStore.Application.Crud;

public interface ICrudRepository<TEntity> : IReadableRepository<TEntity>, IRemovableRepository<TEntity>, IMutableRepository<TEntity> where TEntity : IEntity
{
}