using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenStore.Application;
using OpenStore.Data.OutBox;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;

namespace OpenStore.Data.NoSql.RavenDb
{
    public class RavenOutBoxService : OutBoxService
    {
        private readonly IRavenUnitOfWork _uow;

        public RavenOutBoxService(IRavenUnitOfWork uow, IOpenStoreOutBoxMessageNotifier outBoxMessageNotifier, ILogger<RavenOutBoxService> logger) : base(uow, outBoxMessageNotifier, logger)
        {
            _uow = uow;
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