using System;
using System.Collections.Specialized;
using System.Text;
using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;

namespace OpenStore.Data.Search.ElasticSearch
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddElasticSearch(this IServiceCollection services, Action<ElasticSearchOptions> configureOptions)
        {
            services.Configure(configureOptions);
            AddDefaults(services);

            return services;
        }
        
        public static IServiceCollection AddElasticSearch(this IServiceCollection services, IConfigurationSection configure)
        {
            services.Configure<ElasticSearchOptions>(configure);
            AddDefaults(services);

            return services;
        }

        private static void AddDefaults(IServiceCollection services)
        {
            services.AddSingleton<IElasticClient>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<ElasticSearchOptions>>().Value;
                var logger = sp.GetRequiredService<ILogger<ElasticSearchOptions>>();

                var connectionPool = new StaticConnectionPool(options.Nodes);
                var connectionSettings = new ConnectionSettings(connectionPool)
                    .RequestTimeout(options.RequestTimeout)
                    .GlobalHeaders(new NameValueCollection()
                    {
                        ["X-Application"] = options.AppName
                    })
                    .EnableHttpPipelining()
                    .MaximumRetries(3)
                    .EnableHttpCompression();
                if (options.TraceLog)
                {
                    connectionSettings
                        .DisableDirectStreaming()
                        .OnRequestCompleted(response =>
                        {
                            var method = response.HttpMethod.ToString();
                            if (response.RequestBodyInBytes != null)
                            {
                                logger.LogTrace($"{method} {response.Uri} \n {Encoding.UTF8.GetString(response.RequestBodyInBytes)}");
                            }
                            else
                            {
                                logger.LogTrace($"{method} {response.Uri}");
                            }

                            var statusCode = response.HttpStatusCode.ToString();
                            if (response.ResponseBodyInBytes == null)
                            {
                                logger.LogTrace($"Status: {statusCode}\n {new string('-', 30)}\n");
                            }
                            else
                            {
                                logger.LogTrace($"Status: {statusCode}\n {Encoding.UTF8.GetString(response.ResponseBodyInBytes)}\n {new string('-', 30)}\n");
                            }
                        });
                }

                return new ElasticClient(connectionSettings);
            });
            services.AddSingleton<IElasticSearchStore, ElasticSearchStore>();
        }
    }
}