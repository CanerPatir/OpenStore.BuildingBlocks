using MongoDB.Driver;
using MongoDB.Driver.Linq;
using OpenStore.Application;
using OpenStore.Domain;

namespace OpenStore.Data.NoSql.MongoDb;

public interface IMongoRepository<TAggregateRoot> : ITransactionalRepository<TAggregateRoot>
    where TAggregateRoot : IAggregateRoot
{
    IMongoQueryable<TAggregateRoot> MongoQuery { get; }
    IMongoUnitOfWork MongoUow { get; }
    IMongoCollection<TAggregateRoot> MongoCollection { get; }
}