using OpenStore.Domain;

namespace OpenStore.Data.EntityFramework.ReadOnly;

public interface IReadOnlyRepository<out TEntity>
    where TEntity : IEntity
{
    IQueryable<TEntity> Query { get; }
}