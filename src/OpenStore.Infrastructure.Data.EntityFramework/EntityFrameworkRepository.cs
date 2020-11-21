using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OpenStore.Domain;

// ReSharper disable MemberCanBeProtected.Global

namespace OpenStore.Infrastructure.Data.EntityFramework
{
    public class EntityFrameworkRepository<TAggregateRoot> : Repository<TAggregateRoot>, IEntityFrameworkRepository<TAggregateRoot>
        where TAggregateRoot : class, IAggregateRoot
    {
        private readonly IOutBoxService _outBoxService;

        public EntityFrameworkRepository(IEntityFrameworkCoreUnitOfWork uow, IOutBoxService outBoxService)
        {
            _outBoxService = outBoxService;
            EfUow = uow;
        }

        public IEntityFrameworkCoreUnitOfWork EfUow { get; }
        public IUnitOfWork Uow => EfUow;

        public IQueryable<TAggregateRoot> Query => EfUow.Context.Set<TAggregateRoot>();

        public override async Task<TAggregateRoot> GetAsync(object id, CancellationToken token = default)
        {
            return await EfUow.Context.Set<TAggregateRoot>().FindAsync(new[] {id}, token);
        }

        public override async Task SaveAsync(TAggregateRoot aggregateRoot, CancellationToken token = default)
        {
            if (aggregateRoot.HasUncommittedChanges())
            {
                var events = aggregateRoot.GetUncommittedChanges();
                await _outBoxService.StoreMessages(events, token);
            }

            if (IsCreating(aggregateRoot)) await EfUow.Context.Set<TAggregateRoot>().AddAsync(aggregateRoot, token);

            try
            {
                aggregateRoot.Version++;
                await Uow.SaveChangesAsync(token);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ConcurrencyException(ex.Message, ex);
            }

            aggregateRoot.Commit();
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
}