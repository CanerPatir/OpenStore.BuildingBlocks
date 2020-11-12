using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenStore.Application;
using OpenStore.Domain;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;

namespace OpenStore.Infrastructure.Data.NoSql.RavenDb
{
    public class RavenOutBoxService : OutBoxService
    {
        private readonly IRavenUnitOfWork _uow;

        public RavenOutBoxService(IRavenUnitOfWork uow, IEventNotifier eventNotifier, ILogger<RavenOutBoxService> logger) : base(uow, eventNotifier, logger)
        {
            _uow = uow;
        }

        public override async Task StoreMessages(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
        {
            var outBoxMessages = WrapEvents(events);
            
            foreach (var outBoxMessage in outBoxMessages)
            {
                await _uow.Session.StoreAsync(outBoxMessage, cancellationToken);
            }
        }

        protected override async Task<IReadOnlyCollection<OutBoxMessage>> GetPendingMessages(int take, CancellationToken cancellationToken = default)
        {
            var messages = await _uow.Session
                .Query<OutBoxMessage, GetPendingOutBoxMessages>()
                .ToListAsync(cancellationToken);

            return messages;
        }
        
        public class GetPendingOutBoxMessages : AbstractIndexCreationTask<OutBoxMessage>
        {
            public override string IndexName => "OutBoxMessages/GetPendingOutBoxMessages";

            public GetPendingOutBoxMessages()
            {
               Map = events => 
                    from e in events
                    where e.Committed == false
                    select e;
                
                Reduce = inputs =>
                    from input in inputs
                    orderby input.Version
                    select input;
            }
        }
    }
}