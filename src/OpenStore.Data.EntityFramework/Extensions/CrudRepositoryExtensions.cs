using OpenStore.Application.Crud;
using OpenStore.Data.EntityFramework.Crud;
using OpenStore.Domain;

namespace OpenStore.Data.EntityFramework.Extensions;

public static class CrudRepositoryExtensions
{
    public static void Attach<TEntity>(this ICrudRepository<TEntity> repository, TEntity entity)
        where TEntity : class, IEntity
    {
        if (repository is EntityFrameworkCrudRepository<TEntity> entityFrameworkRepository)
        {
            entityFrameworkRepository.Set.Attach(entity);
        }
    }
}