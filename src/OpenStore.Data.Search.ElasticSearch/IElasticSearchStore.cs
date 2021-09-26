using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nest;

namespace OpenStore.Data.Search.ElasticSearch
{
    public interface IElasticSearchStore
    {
        IElasticClient ElasticClient { get; }

        Task CreateIndex<TDocument>(string index, CancellationToken cancellationToken = default, Action<IndexSettingsDescriptor> indexSettings = default)
            where TDocument : BaseElasticDocument;

        Task IndexManyParallel<TDocument>(string index, IEnumerable<TDocument> documents, Action<BulkIndexDescriptor<TDocument>> indexConfiguration, CancellationToken cancellationToken = default)
            where TDocument : BaseElasticDocument;

        Task<GetResponse<TDocument>> Get<TDocument>(string index, object id, CancellationToken cancellationToken = default)
            where TDocument : BaseElasticDocument;

        Task<DeleteResponse> Delete<TDocument>(string indexName, TDocument document, CancellationToken cancellationToken = default)
            where TDocument : BaseElasticDocument;

        Task Index<TDocument>(TDocument document, Action<IndexDescriptor<TDocument>> indexConfiguration = default, CancellationToken cancellationToken = default)
            where TDocument : BaseElasticDocument;

        Task SwitchIndex(string newIndexName, string mainAlias, bool deleteOldIndex, Dictionary<string, string> aliasMapping, CancellationToken cancellationToken = default);
    }
}