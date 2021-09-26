using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OpenStore.Application;
using OpenStore.Data.OutBox;
using OpenStore.Domain;

namespace OpenStore.Data.NoSql.RavenDb.OutBox
{
    public class RavenOutBoxStoreService : OutBoxStoreService
    {
        private readonly IRavenUnitOfWork _uow;
        private readonly RavenDatabaseSettings _ravenDatabaseSettings;

        public RavenOutBoxStoreService(IRavenUnitOfWork uow, IOptions<RavenDatabaseSettings> ravenDatabaseSettingsOptions, IOpenStoreUserContextAccessor openStoreUserContextAccessor) : base(openStoreUserContextAccessor)
        {
            _uow = uow;
            _ravenDatabaseSettings = ravenDatabaseSettingsOptions.Value;
        }

        public override async Task StoreMessages(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
        {
            if (_ravenDatabaseSettings.OutBoxEnabled is false)
            {
                return;
            }
            var outBoxMessages = WrapEvents(events);

            foreach (var outBoxMessage in outBoxMessages)
            {
                await _uow.Session.StoreAsync(outBoxMessage, cancellationToken);
            }
        }
    }
}