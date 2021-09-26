using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenStore.Data.OutBox;

// ReSharper disable SuspiciousTypeConversion.Global

namespace OpenStore.Data.EntityFramework.OutBox
{
    public class EntityFrameworkOutBoxService : OutBoxService
    {
        private readonly DbContext _context;

        public EntityFrameworkOutBoxService(IEntityFrameworkCoreUnitOfWork uow,
            IMediator mediator, 
            ILogger<EntityFrameworkOutBoxService> logger) : base(uow,
            mediator, logger)
        {
            _context = uow.Context;
        }

        public override async Task<IReadOnlyCollection<OutBoxMessage>> FetchPendingMessages(int take, CancellationToken cancellationToken = default)
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