using MongoDB.Bson.Serialization.Attributes;
using OpenStore.Domain;

namespace OpenStore.Data.NoSql.MongoDb;

public class MongoEntity : Entity<string>
{
    [BsonId] public override string Id { get; protected set; }
    internal bool FromDb { get; set; }
}