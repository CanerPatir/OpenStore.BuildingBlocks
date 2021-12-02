using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OpenStore.Application;
using OpenStore.Data.OutBox;
using OpenStore.Domain;

namespace OpenStore.Data.NoSql.MongoDb.OutBox;

public class MongoOutBoxStoreService : OutBoxStoreService
{
    private readonly IMongoUnitOfWork _uow;
    private readonly IMongoCollection<OutBoxMessage> _collection;
    private readonly MongoDbSettings _mongoDbSettings;

    public MongoOutBoxStoreService(IMongoUnitOfWork uow, IOptions<MongoDbSettings> mongoDbSettingsOptions,  IOpenStoreUserContextAccessor openStoreUserContextAccessor) : base(openStoreUserContextAccessor)
    {
        _uow = uow;
        _collection = _uow.DatabaseBase.GetCollection<OutBoxMessage>(nameof(OutBoxMessage));
        _mongoDbSettings = mongoDbSettingsOptions.Value;
    }

    public override async Task StoreMessages(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        if (_mongoDbSettings.OutBoxEnabled is false)
        {
            return;
        }
        var outBoxMessages = WrapEvents(events).ToList();
        await _collection.InsertManyAsync(_uow.Session, outBoxMessages, new InsertManyOptions(), cancellationToken);
    }
}