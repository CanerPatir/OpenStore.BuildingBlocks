using Microsoft.EntityFrameworkCore;
using OpenStore.Application.Crud;
using OpenStore.Domain;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace OpenStore.Data.EntityFramework.Crud;

public class EntityFrameworkCrudRepository<TEntity> : ICrudRepository<TEntity>
    where TEntity : class, IEntity
{
    public EntityFrameworkCrudRepository(IEntityFrameworkCoreUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork;
    }

    public IQueryable<TEntity> Query => Set;

    public DbSet<TEntity> Set => UnitOfWork.Context.Set<TEntity>();

    public IEntityFrameworkCoreUnitOfWork UnitOfWork { get; }

    public virtual async Task<TEntity> GetAsync(object id, CancellationToken cancellationToken = default)
        => await Set.FindAsync(new[] { id }, cancellationToken);

    public virtual async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Set.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Set.Update(entity);
        return Task.CompletedTask;
    }

    public virtual async Task RemoveByIdAsync(object id, CancellationToken cancellationToken = default) => await Remove(await GetAsync(id, cancellationToken));

    public Task Remove(TEntity entity)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }
    
    public virtual Task SaveChangesAsync(CancellationToken cancellationToken = default) => UnitOfWork.SaveChangesAsync(cancellationToken);
}