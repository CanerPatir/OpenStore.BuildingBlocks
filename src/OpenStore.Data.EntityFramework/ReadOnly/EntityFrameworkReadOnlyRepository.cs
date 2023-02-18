using Microsoft.EntityFrameworkCore;
using OpenStore.Domain;

namespace OpenStore.Data.EntityFramework.ReadOnly;

public class EntityFrameworkReadOnlyRepository<TEntity> : IReadOnlyRepository<TEntity>
    where TEntity : class, IEntity
{
    private readonly DbContext _readonlyContext;

    public EntityFrameworkReadOnlyRepository(IReadOnlyDbContext context)
    {
        _readonlyContext = (DbContext)context;
    }

    public IQueryable<TEntity> Query => _readonlyContext.Set<TEntity>().AsNoTracking();
}