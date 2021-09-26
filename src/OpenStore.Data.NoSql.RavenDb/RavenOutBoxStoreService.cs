using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenStore.Application;
using OpenStore.Data.OutBox;
using OpenStore.Domain;

namespace OpenStore.Data.NoSql.RavenDb
{
    public class RavenOutBoxStoreService : OutBoxStoreService
    {
        private readonly IRavenUnitOfWork _uow;

        public RavenOutBoxStoreService(IRavenUnitOfWork uow, IOpenStoreUserContextAccessor openStoreUserContextAccessor) : base(openStoreUserContextAccessor)
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
    }
}