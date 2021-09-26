using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OpenStore.Application;
using OpenStore.Data.OutBox;
using OpenStore.Domain;

namespace OpenStore.Data.EntityFramework
{
    public class EntityFrameworkOutBoxStoreService<TDbContext> : OutBoxStoreService
        where TDbContext : DbContext
    {
        private readonly DbContext _context;

        public EntityFrameworkOutBoxStoreService(TDbContext context, IOpenStoreUserContextAccessor openStoreUserContextAccessor) : base(openStoreUserContextAccessor)
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