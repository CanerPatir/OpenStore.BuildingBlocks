using MongoDB.Driver;

namespace OpenStore.Data.NoSql.MongoDb;

public class MongoDbSettings
{
    public MongoClientSettings MongoClientSettings { get; set; }
    public bool OutBoxEnabled { get; set; }
    public int OutBoxFetchSize { get; set; } = 2000;
}