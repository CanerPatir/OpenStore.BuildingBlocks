using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using OpenStore.Application;
using OpenStore.Data.OutBox;

namespace OpenStore.Data.NoSql.MongoDb
{
    public class MongoOutBoxService : OutBoxService
    {
        private readonly IMongoUnitOfWork _uow;
        private readonly IMongoCollection<OutBoxMessage> _collection;

        public MongoOutBoxService(
            IMongoUnitOfWork uow,
            IMediator mediator,
            ILogger<MongoOutBoxService> logger
        )
            : base(uow, mediator, logger)
        {
            _uow = uow;
            _collection = _uow.DatabaseBase.GetCollection<OutBoxMessage>(nameof(OutBoxMessage));
        }

        public override async Task<IReadOnlyCollection<OutBoxMessage>> FetchPendingMessages(int take, CancellationToken cancellationToken = default)
        {
            var messages = await _collection
                .AsQueryable()
                .Where(x => !x.Committed)
                .ToListAsync(cancellationToken: cancellationToken);

            return messages.OrderBy(x => x.Version).ToList();
        }
    }
}