using System;
using System.Text.Json.Serialization;
using Nest;

namespace OpenStore.Data.Search.ElasticSearch
{
    public abstract class BaseElasticDocument
    {
        [Keyword] public string Id { get; set; }
        public long Version { get; set; }
        
        [JsonIgnore] public abstract string Collection { get; } // document type for multi type indexes
        [JsonIgnore] public abstract string IndexName { get; }

        public DateTimeOffset IndexedAt { get; set; }

        protected BaseElasticDocument()
        {
            IndexedAt = DateTimeOffset.UtcNow;
        }
    }
}