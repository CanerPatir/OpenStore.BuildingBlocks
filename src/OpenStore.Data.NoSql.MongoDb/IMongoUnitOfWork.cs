using MongoDB.Driver;
using OpenStore.Domain;

namespace OpenStore.Data.NoSql.MongoDb;

public interface IMongoUnitOfWork : IUnitOfWork
{
    IMongoDatabase DatabaseBase { get; }
    IClientSessionHandle Session { get; }
}