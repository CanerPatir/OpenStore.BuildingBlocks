using MongoDB.Bson.Serialization.Attributes;
using OpenStore.Domain;

namespace OpenStore.Infrastructure.Data.NoSql.MongoDb
{
    public class MongoEntity: Entity<string>
    {
        [BsonId]
        public override string Id { get; protected set; }
        internal bool FromDb { get; set; }
    }
}