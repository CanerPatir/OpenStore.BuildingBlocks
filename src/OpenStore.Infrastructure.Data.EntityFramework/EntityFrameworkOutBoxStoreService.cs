using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OpenStore.Domain;

namespace OpenStore.Infrastructure.Data.EntityFramework
{
    public class EntityFrameworkOutBoxStoreService<TDbContext> : OutBoxStoreService
        where TDbContext : DbContext
    {
        private readonly DbContext _context;

        public EntityFrameworkOutBoxStoreService(TDbContext context)
        {
            _context = context;
        }

        public override async Task StoreMessages(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
        {
            if (_context is IOutBoxDbContext eventStoreContext)
            {
                await eventStoreContext.OutBoxMessages.AddRangeAsync(WrapEvents(events), cancellationToken);
            }
        }
    }
}