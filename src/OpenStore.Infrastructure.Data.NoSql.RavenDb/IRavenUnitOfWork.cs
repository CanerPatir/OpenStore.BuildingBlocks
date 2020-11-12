using OpenStore.Domain;
using Raven.Client.Documents.Session;

namespace OpenStore.Infrastructure.Data.NoSql.RavenDb
{
    public interface IRavenUnitOfWork : IUnitOfWork
    {
        IAsyncDocumentSession Session { get; }
    }
}