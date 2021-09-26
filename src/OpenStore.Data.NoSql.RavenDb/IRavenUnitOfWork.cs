using OpenStore.Domain;
using Raven.Client.Documents.Session;

namespace OpenStore.Data.NoSql.RavenDb
{
    public interface IRavenUnitOfWork : IUnitOfWork
    {
        IAsyncDocumentSession Session { get; }
    }
}