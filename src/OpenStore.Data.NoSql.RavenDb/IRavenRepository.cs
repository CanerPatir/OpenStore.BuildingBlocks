using OpenStore.Application;
using OpenStore.Domain;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace OpenStore.Data.NoSql.RavenDb
{
    public interface IRavenRepository<TAggregateRoot> : ITransactionalRepository<TAggregateRoot>
        where TAggregateRoot : AggregateRoot<string>
    {
        IRavenQueryable<TAggregateRoot> RavenQuery(string indexName = null, string collectionName = null, bool isMapReduce = false);
        IRavenUnitOfWork RavenUow { get; }
        IAsyncDocumentSession RavenSession { get; }
    }
}