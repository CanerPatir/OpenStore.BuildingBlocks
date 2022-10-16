using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OpenStore.Domain;

// ReSharper disable MemberCanBeProtected.Global

namespace OpenStore.Data.EntityFramework;

public class EntityFrameworkRepository<TAggregateRoot> : Repository<TAggregateRoot>, IEntityFrameworkRepository<TAggregateRoot>
    where TAggregateRoot : class, IAggregateRoot, ISavingChanges
{
    public EntityFrameworkRepository(IEntityFrameworkCoreUnitOfWork uow)
    {
        EfUow = uow;
    }

    public IEntityFrameworkCoreUnitOfWork EfUow { get; }
    public IUnitOfWork Uow => EfUow;

    public IQueryable<TAggregateRoot> Query => EfUow.Context.Set<TAggregateRoot>();

    public override async Task<TAggregateRoot> GetAsync(object id, CancellationToken token = default)
        => await EfUow.Context.Set<TAggregateRoot>().FindAsync(new[] { id }, token);

    public override async Task SaveAsync(TAggregateRoot aggregateRoot, CancellationToken token = default)
    {
        if (IsCreating(aggregateRoot))
        {
            await EfUow.Context.Set<TAggregateRoot>().AddAsync(aggregateRoot, token);
        }

        await Uow.SaveChangesAsync(token);
    }

    public override Task Delete(TAggregateRoot aggregateRoot, CancellationToken token = default)
    {
        EfUow.Context.Set<TAggregateRoot>().Remove(aggregateRoot);
        return Task.CompletedTask;
    }

    private bool IsCreating(TAggregateRoot entity)
    {
        var entry = EfUow.Context.ChangeTracker.Entries<TAggregateRoot>().SingleOrDefault(x => x.Entity == entity);
        return entry == null || entry.State == EntityState.Added;
    }
}