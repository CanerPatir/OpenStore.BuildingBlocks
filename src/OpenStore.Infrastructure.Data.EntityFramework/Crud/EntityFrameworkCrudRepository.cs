using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OpenStore.Application.Crud;
using OpenStore.Domain;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace OpenStore.Infrastructure.Data.EntityFramework.Crud
{
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
        {
            return await Set.FindAsync(new[] {id}, cancellationToken);
        }

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

        public virtual async Task DeleteAsync(object id, CancellationToken cancellationToken = default)
        {
            Set.Remove(await GetAsync(id, cancellationToken));
        }

        public virtual Task SaveChangesAsync(CancellationToken cancellationToken = default) => UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}