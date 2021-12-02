using System.Collections.Concurrent;
using System.Text.Json;
using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;

namespace OpenStore.Data.Search.ElasticSearch;

public class ElasticSearchStore : IElasticSearchStore
{
    public IElasticClient ElasticClient { get; }

    private readonly ILogger<ElasticSearchStore> _logger;
    private readonly ElasticSearchOptions _options;
    private readonly int[] _notRetryStatusCodes = {404, 409};

    public ElasticSearchStore(IElasticClient elasticClient, ILogger<ElasticSearchStore> logger, IOptions<ElasticSearchOptions> options)
    {
        ElasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public Task CreateIndex<TDocument>(string index, CancellationToken cancellationToken = default, Action<IndexSettingsDescriptor> indexSettings = default)
        where TDocument : BaseElasticDocument
    {
        var indexOptions = _options.Indexes[index];

        return ElasticClient.CreateIndex<TDocument>(indexOptions.Name, IndexCreateStrategy.CreateIfNotExists, cancellationToken, builder => { builder.AutoMap<TDocument>(); },
            settingsBuilder =>
            {
                settingsBuilder
                    .NumberOfShards(indexOptions.ShardCount)
                    .NumberOfReplicas(indexOptions.ReplicaCount)
                    .RefreshInterval(new Time(TimeSpan.FromSeconds(indexOptions.RefreshIntervalSecond)))
                    .RequestsCacheEnabled();

                foreach (var (key, value) in indexOptions.Settings)
                {
                    settingsBuilder.Setting(key, value);
                }

                indexSettings?.Invoke(settingsBuilder);
            });
    }

    public async Task IndexManyParallel<TDocument>(string index, IEnumerable<TDocument> documents, Action<BulkIndexDescriptor<TDocument>> indexConfiguration, CancellationToken cancellationToken = default)
        where TDocument : BaseElasticDocument
    {
        await IndexManyChunkParallel(index, documents, indexConfiguration, cancellationToken);
    }

    public Task<GetResponse<TDocument>> Get<TDocument>(string index, object id, CancellationToken cancellationToken = default)
        where TDocument : BaseElasticDocument
    {
        return ElasticClient.GetAsync<TDocument>(id.ToString(), x => x.Index(index), cancellationToken);
    }

    public async Task<DeleteResponse> Delete<TDocument>(string indexName, TDocument document, CancellationToken cancellationToken = default)
        where TDocument : BaseElasticDocument
    {
        var response = await ElasticClient.DeleteAsync<TDocument>(document.Id.ToString(), i => i
            .Version(document.Version)
            .VersionType(VersionType.ExternalGte)
            .Index(indexName)
            .Routing(new Routing(document.Id)), cancellationToken);

        if (!response.IsValid && response.Result != Result.NotFound)
        {
            _logger.LogWarning($"An error occured when deleting document: id:{document.Id}, information: {response.DebugInformation}, server error: {response.ServerError}");
        }

        return response;
    }
        
    public async Task Index<TDocument>(TDocument document, Action<IndexDescriptor<TDocument>> indexConfiguration = default, CancellationToken cancellationToken = default)
        where TDocument : BaseElasticDocument
    {
        var documentId = $"{document.Collection}/{document.Id}";
        IndexResponse response = null;
        var maxRetryAttempt = 3;
        for (var retryAttempt = 0; retryAttempt < maxRetryAttempt; retryAttempt++)
        {
            response = await ElasticClient.IndexAsync(document, i =>
            {
                i
                    .Index(document.IndexName)
                    .Version(document.Version)
                    .VersionType(VersionType.ExternalGte)
                    .Routing(document.Id);

                indexConfiguration?.Invoke(i);
                return i;
            }, cancellationToken);
            if (response.IsValid)
            {
                break;
            }

            if (response.ServerError != null && _notRetryStatusCodes.Contains(response.ServerError.Status))
            {
                _logger.LogInformation(response.OriginalException, $"Indexing failed, continue... document: '{documentId}', status code: {response.ServerError.Status}");
                return;
            }

            if (retryAttempt == maxRetryAttempt - 1)
            {
                break;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt) * 500), cancellationToken);
        }

        if (response != null && response.IsValid == false)
        {
            throw new ElasticsearchClientException(PipelineFailure.BadResponse, $"Indexing failed. document: '{documentId}'", response.OriginalException);
        }
    }

    private Task IndexManyChunkParallel<TDocument>(string indexName, IEnumerable<TDocument> documents, Action<BulkIndexDescriptor<TDocument>> indexConfiguration, CancellationToken cancellationToken = default)
        where TDocument : BaseElasticDocument
    {
        var docs = documents.ToArray();
        if (!docs.Any())
        {
            return Task.CompletedTask;
        }

        int size = 1;
        if (docs.Length > 1)
        {
            size = Math.Max(docs.Length / EnvironmentHelper.ProcessorCount, 10);
        }

        var droppedItems = new ConcurrentDictionary<string, int>();

        ElasticClient.BulkAll(docs, d => d
                .Index(indexName)
                .BackOffTime("30s")
                .BackOffRetries(2)
                .MaxDegreeOfParallelism(EnvironmentHelper.ProcessorCount)
                .Size(size)
                .RefreshOnCompleted()
                .ContinueAfterDroppedDocuments()
                .BufferToBulk((descriptor, list) =>
                {
                    foreach (var item in list)
                    {
                        // var version = configuration.VersionExtractor(item);
                        descriptor
                            .Index<TDocument>(xx =>
                            {
                                xx
                                    .Document(item)
                                    .Routing(item.Id)
                                    .Version(item.Version).VersionType(VersionType.ExternalGte);

                                indexConfiguration?.Invoke(xx);
                                return xx;
                            });

                        ;
                    }
                })
                .RetryDocumentPredicate((response, document) => !_notRetryStatusCodes.Contains(response.Status))
                .DroppedDocumentCallback((response, document) =>
                {
                    var key = $"{document.Collection}/{document.Id}";
                    _logger.LogDebug($"Unable to bulk index: {response} {key}");
                    if (!_notRetryStatusCodes.Contains(response.Status))
                    {
                        droppedItems.AddOrUpdate(key, response.Status, (s, i) => response.Status);
                    }
                })
            )
            .Wait(TimeSpan.FromMinutes(10),
                response => { _logger.LogDebug($"Index bulk completed: Total page: {response.Page.ToString()}, Total retry: {response.Retries.ToString()}"); });

        if (droppedItems.Any())
        {
            throw new ElasticsearchClientException(PipelineFailure.BadResponse, $"Some documents not indexed: {string.Join(",", droppedItems.Keys)}", (Exception) null);
        }

        return Task.CompletedTask;
    }

    private QueryContainer BuildAliasFilter(string documentType)
    {
        return new QueryContainerDescriptor<BaseElasticDocument>()
            .Bool(b => b
                .Filter(fi => fi
                    .Term(t => t
                        .Field(x => x.Collection)
                        .Value(documentType)
                    )
                )
            );
    }

    public async Task SwitchIndex(string newIndexName, string mainAlias, bool deleteOldIndex, Dictionary<string, string> aliasMapping, CancellationToken cancellationToken = default)
    {
        var bulkAliasDescriptor = new BulkAliasDescriptor();
        var indexOfAlias = await ElasticClient.Indices.GetAliasAsync(null, ad => ad.Name(mainAlias), cancellationToken);
        var previousIndexName = "";
        if (indexOfAlias.IsValid)
        {
            previousIndexName = indexOfAlias.Indices.FirstOrDefault().Key.ToString();
            var result = await ElasticClient.LowLevel.Indices.GetAliasAsync<StringResponse>(previousIndexName, null, cancellationToken);
            if (result.Success)
            {
                var aliases = JsonSerializer.Deserialize<Dictionary<string, AliasDto>>(result.Body);
                foreach (var (aliasName, _) in aliases.FirstOrDefault().Value.Aliases)
                {
                    bulkAliasDescriptor.Remove(a => a.Index(previousIndexName).Alias(aliasName));
                }
            }
        }

        foreach (var (alias, documentType) in aliasMapping)
        {
            bulkAliasDescriptor.Add(new AliasAddAction
            {
                Add = new AliasAddOperation
                {
                    Alias = alias,
                    Filter = BuildAliasFilter(documentType),
                    Index = newIndexName,
                    IsWriteIndex = true
                }
            });
        }

        var response = await ElasticClient.Indices.BulkAliasAsync(bulkAliasDescriptor, cancellationToken);
        if (response.IsValid)
        {
            if (deleteOldIndex && string.IsNullOrEmpty(previousIndexName) == false)
            {
                await ElasticClient.Indices.DeleteAsync(previousIndexName, ct: cancellationToken);
            }
        }
    }

    private class AliasDto
    {
        public Dictionary<string, object> Aliases { get; set; }
    }
}