using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OpenStore.Infrastructure.Data.EntityFramework.Extensions;

namespace OpenStore.Infrastructure.Data.EntityFramework
{
    public class EntityFrameworkUnitOfWork<TDbContext> : IEntityFrameworkCoreUnitOfWork
        where TDbContext : DbContext
    {
        public DbContext Context { get; }

        public EntityFrameworkUnitOfWork(TDbContext dbContext)
        {
            Context = dbContext;
        }

        public async Task BeginTransactionAsync(CancellationToken token = default)
        {
            var tx =  Context.Database.CurrentTransaction ?? await Context.Database.BeginTransactionAsync(token);
        }

        public async Task SaveChangesAsync(CancellationToken token = default)
        {
            var result = await Context.SaveChangesWithValidationAsync(token: token);

            if (!result.IsValid)
            {
                throw new EntityValidationException(result.Message, result.Errors.Select(x => x.ErrorResult));
            }
        }
    }
}