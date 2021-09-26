using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;
using Nito.AsyncEx;

namespace OpenStore.Data.Search.ElasticSearch
{
    public static class ElasticClientExtensions
    {
        public static async Task<IEnumerable<T>> SearchAllWithParallelScroll<T>(this IElasticClient elasticClient,
            Func<SearchDescriptor<T>, ISearchRequest> query,
            int shardSize = 2,
            Expression<Func<T, object>> routingPath = null,
            CancellationToken cancellationToken = default,
            string scrollTimeout = "2m", int scrollSize = 1000) where T : BaseElasticDocument
        {
            shardSize = Math.Max(shardSize, 2);
            var documents = new ConcurrentBag<IReadOnlyCollection<T>>();
            var observable = elasticClient.ScrollAll<T>(scrollTimeout, shardSize, s => s
                .MaxDegreeOfParallelism(Math.Max(shardSize / 2, 1))
                .RoutingField(routingPath)
                .Search(search =>
                    {
                        var descriptor = query(search);
                        descriptor.Size = scrollSize;
                        return descriptor;
                    }
                ));

            var seenDocuments = 0;
            var waitHandle = new AsyncManualResetEvent(false);
            Exception exception = null;
            observable.Subscribe(new ScrollAllObserver<T>(
                onNext: r =>
                {
                    documents.Add(r.SearchResponse.Documents);
                    Interlocked.Add(ref seenDocuments, r.SearchResponse.Hits.Count);
                },
                onError: e =>
                {
                    exception = e;
                    waitHandle.Set();
                },
                onCompleted: () => waitHandle.Set()
            ));

            await waitHandle.WaitAsync(cancellationToken);

            if (exception != null)
                throw exception;

            return documents.SelectMany(x => x);
        }

        public static async Task<IEnumerable<T>> SearchAllWithScroll<T>(this IElasticClient elasticClient,
            Func<SearchDescriptor<T>, ISearchRequest> selector,
            CancellationToken cancellationToken = default,
            string scrollTimeout = "2m", int scrollSize = 1000) where T : class
        {
            var initialResponse = await elasticClient.SearchAsync<T>(x =>
            {
                var descriptor = selector(x.Scroll(scrollTimeout));
                descriptor.From = 0;
                descriptor.Size = scrollSize;
                return descriptor;
            }, cancellationToken);


            var results = new List<T>();
            if (!initialResponse.IsValid || string.IsNullOrEmpty(initialResponse.ScrollId))
            {
                throw new Exception(initialResponse.ServerError.Error.Reason);
            }

            if (initialResponse.Documents.Any())
            {
                results.AddRange(initialResponse.Documents);
            }

            var scrollId = initialResponse.ScrollId;
            var isScrollSetHasData = true;
            while (isScrollSetHasData)
            {
                var loopingResponse = await elasticClient.ScrollAsync<T>(scrollTimeout, scrollId, ct: cancellationToken);
                if (loopingResponse.IsValid)
                {
                    results.AddRange(loopingResponse.Documents);
                    scrollId = loopingResponse.ScrollId;
                }

                isScrollSetHasData = loopingResponse.Documents.Any();
            }

            await elasticClient.ClearScrollAsync(new ClearScrollRequest(scrollId), cancellationToken);
            return results;
        }
        
        public static async Task CreateIndexWithMapping<T>(this IElasticClient elasticClient, string index, IndexCreateStrategy strategy = IndexCreateStrategy.CreateIfNotExists,
            CancellationToken cancellationToken = default,
            Func<TypeMappingDescriptor<T>, ITypeMapping> mappingDescriptorBuilder = null,
            Action<IndexSettingsDescriptor> indexSettingsBuilder = null,
            int numberOfShards = 3) where T : BaseElasticDocument
        {
            if (strategy == IndexCreateStrategy.CreateIfNotExists)
            {
                var indexExistsResponse = await elasticClient.Indices.ExistsAsync(index, ct: cancellationToken);
                if (indexExistsResponse.Exists)
                {
                    return;
                }
            }

            indexSettingsBuilder ??= (descriptor =>
            {
                descriptor.NumberOfShards(3)
                    .Setting("max_result_window", int.MaxValue)
                    .NumberOfReplicas(1);
            });

            mappingDescriptorBuilder ??= (descriptor => descriptor.AutoMap<T>());

            var result = await elasticClient.Indices.CreateAsync(index, descriptor => descriptor
                    .Settings(settingsDescriptor =>
                    {
                        indexSettingsBuilder(settingsDescriptor);
                        return settingsDescriptor;
                    })
                    .Map<T>(mappingsDescriptor => mappingDescriptorBuilder(mappingsDescriptor))
                , cancellationToken);
            if (!result.IsValid)
            {
                throw new Exception($"An error occured when creating index, detail: {result.DebugInformation}", result.OriginalException);
            }
        }

        public static async Task CreateIndex<T>(this IElasticClient elasticClient,
            string index, 
            IndexCreateStrategy strategy = IndexCreateStrategy.CreateIfNotExists,
            CancellationToken cancellationToken = default,
            Action<TypeMappingDescriptor<T>> mappingDescriptorBuilder = null,
            Action<IndexSettingsDescriptor> indexSettingsBuilder = null,
            int numberOfShards = 3) where T : BaseElasticDocument
        {
            if (strategy == IndexCreateStrategy.CreateIfNotExists)
            {
                var indexExistsResponse = await elasticClient.Indices.ExistsAsync(index, ct: cancellationToken);
                if (indexExistsResponse.Exists)
                {
                    return;
                }
            }

            indexSettingsBuilder ??= (descriptor =>
            {
                descriptor.NumberOfShards(numberOfShards)
                    .Setting("max_result_window", int.MaxValue)
                    .NumberOfReplicas(1);
            });

            mappingDescriptorBuilder ??= (descriptor => descriptor.AutoMap<T>());

            var result = await elasticClient.Indices.CreateAsync(index, descriptor => descriptor
                .Settings(settingsDescriptor =>
                {
                    indexSettingsBuilder(settingsDescriptor);
                    return settingsDescriptor;
                })
                .Map<T>(mappingsDescriptor =>
                {
                    mappingDescriptorBuilder(mappingsDescriptor);
                    return mappingsDescriptor;
                }), cancellationToken);
            if (!result.IsValid)
            {
                throw new Exception($"An error occured when creating index, detail: {result.DebugInformation}", result.OriginalException);
            }
        }

        public static async Task AddOrUpdate<TDocument>(this IElasticClient client, string id,
            Func<TDocument> addFactory,
            Func<TDocument, object> updateFactory,
            CancellationToken cancellationToken = default)
            where TDocument : BaseElasticDocument, new()
        {
            var response = await client.GetAsync<TDocument>(id, ct: cancellationToken).ConfigureAwait(false);
            var doc = response?.Source;
            if (response == null || response.Found == false)
            {
                doc = addFactory();
                await client.IndexAsync(doc, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var updateDoc = updateFactory == null ? new { } : updateFactory(doc);

                await client.UpdateAsync<TDocument, object>(id, updateDoc, 3, cancellationToken).ConfigureAwait(false);
            }
        }

        public static Task IndexAsync<TDocument>(this IElasticClient client, TDocument document, CancellationToken cancellationToken = default) where TDocument : BaseElasticDocument
        {
            return client.IndexDocumentAsync(document, cancellationToken);
        }

        public static Task IndexAsync<TDocument>(this IElasticClient client, TDocument[] documents, CancellationToken cancellationToken = default) where TDocument : BaseElasticDocument
        {
            if (documents.Length == 0)
            {
                return Task.CompletedTask;
            }

            return client.IndexManyAsync(documents, cancellationToken: cancellationToken);
        }

        public static Task DeleteAsync<TDocument>(this IElasticClient client, string id, CancellationToken cancellationToken = default) where TDocument : BaseElasticDocument
        {
            return client.DeleteAsync<TDocument>(id, f => f.Refresh(Refresh.WaitFor), cancellationToken);
        }

        public static Task UpdateAsync<TDocument, TUpdate>(this IElasticClient client, string id, TUpdate updateObject, int retryLevel = 3, CancellationToken cancellationToken = default) 
            where TDocument : class 
            where TUpdate : class
        {
            return client.UpdateAsync<TDocument, TUpdate>(DocumentPath<TDocument>.Id(id), up => up
                .DocAsUpsert()
                .Doc(updateObject)
                .RetryOnConflict(retryLevel)
                .Refresh(Refresh.WaitFor), cancellationToken);
        }
    }
}