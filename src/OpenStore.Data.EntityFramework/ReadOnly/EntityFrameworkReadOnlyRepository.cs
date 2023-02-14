using Microsoft.EntityFrameworkCore;
using OpenStore.Domain;

namespace OpenStore.Data.EntityFramework.ReadOnly;

public class EntityFrameworkReadOnlyRepository<TEntity> : IReadOnlyRepository<TEntity>
    where TEntity : class, IEntity
{
    private readonly ReadOnlyDbContext _context;

    public EntityFrameworkReadOnlyRepository(ReadOnlyDbContext context)
    {
        _context = context;
    }

    public IQueryable<TEntity> Query => _context.Set<TEntity>().AsNoTracking();
}