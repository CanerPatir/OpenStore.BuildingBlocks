using System;
using System.Collections.Generic;

namespace OpenStore.Data.Search.ElasticSearch;

public class ElasticSearchOptions
{
    public ElasticSearchOptions()
    {
        RequestTimeout = TimeSpan.FromSeconds(30);
    }

    public string AppName { get; set; }
    public Uri[] Nodes { get; set; }
    public bool TraceLog { get; set; }

    public TimeSpan RequestTimeout { get; set; }

    public IDictionary<string, IndexOptions> Indexes { get; set; } = new Dictionary<string, IndexOptions>();

    public class IndexOptions
    {
        public string Name { get; set; }
        public int ShardCount { get; set; }
        public int ReplicaCount { get; set; }
        public int RefreshIntervalSecond { get; set; }

        public Dictionary<string, object> Settings { get; set; }

        public IndexOptions()
        {
            ShardCount = 1;
            ReplicaCount = 1;
            RefreshIntervalSecond = 15;
            Settings = new Dictionary<string, object>();
        }
    }
}