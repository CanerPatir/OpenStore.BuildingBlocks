using Couchbase;
using OpenStore.Domain;

namespace OpenStore.Data.NoSql.Couchbase
{
    public interface ICouchbaseRepository<TAggregateRoot> : IRepository<TAggregateRoot>
        where TAggregateRoot : IAggregateRoot
    {
        public IBucket Bucket { get; }
    }
}