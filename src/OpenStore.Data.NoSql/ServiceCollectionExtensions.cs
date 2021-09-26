using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Data.NoSql.Couchbase;
using OpenStore.Data.NoSql.MongoDb;
using OpenStore.Data.NoSql.RavenDb;

namespace OpenStore.Data.NoSql
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// {
        ///     "DataSource" : "RavenDb|Couchbase|MongoDb",
        ///     "Settings" : {}, // Technology specific settings
        /// }
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="outboxPollEnabled"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IServiceCollection AddOpenStoreNoSql(
            this IServiceCollection services
            , IConfiguration configuration
            , params Assembly[] assemblies)
        {
            var dataSource = NoSqlDataSource.FromString(configuration.GetValue<string>("DataSource"));
            return services.AddOpenStoreNoSql(dataSource, configuration.GetSection("Settings"), assemblies);
        }

        public static IServiceCollection AddOpenStoreNoSql(
            this IServiceCollection services
            , NoSqlDataSource noSqlDataSource
            , IConfiguration configuration
            , params Assembly[] assemblies)
        {
            // todo: try to remove assemblies parameter
            switch (noSqlDataSource)
            {
                case RavenDbSource r:
                    services.AddRavenDbDataInfrastructure(configuration, assemblies);
                    break;
                case CouchbaseSource c:
                    services.AddCouchbaseDataInfrastructure(configuration, assemblies);
                    break;
                case MongoDbSource c:
                    services.AddMongoDbDataInfrastructure(configuration, assemblies);
                    break;
            }

            return services;
        }
    }
}