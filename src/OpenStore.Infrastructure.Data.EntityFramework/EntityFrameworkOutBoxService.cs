using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenStore.Application;

// ReSharper disable SuspiciousTypeConversion.Global

namespace OpenStore.Infrastructure.Data.EntityFramework
{
    public class EntityFrameworkOutBoxService : OutBoxService
    {
        private readonly DbContext _context;

        public EntityFrameworkOutBoxService(IEntityFrameworkCoreUnitOfWork uow,
            IOpenStoreDomainEventNotifier domainEventNotifier, 
            ILogger<EntityFrameworkOutBoxService> logger) : base(uow,
            domainEventNotifier, logger)
        {
            _context = uow.Context;
        }

        protected override async Task<IReadOnlyCollection<OutBoxMessage>> GetPendingMessages(int take, CancellationToken cancellationToken = default)
        {
            if (_context is not IOutBoxDbContext eventStoreContext) return new List<OutBoxMessage>();

            IQueryable<OutBoxMessage> q = eventStoreContext
                .OutBoxMessages
                .Where(x => !x.Committed)
                .OrderBy(x => x.Version);

            if (take != int.MaxValue)
            {
                q = q.Take(take);
            }

            return await q
                .ToListAsync(cancellationToken);
        }
    }
}