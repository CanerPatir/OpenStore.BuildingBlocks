using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace OpenStore.Infrastructure.Data.NoSql.RavenDb
{
    public class RavenUnitOfWork : IRavenUnitOfWork
    {
        public RavenUnitOfWork(IAsyncDocumentSession session)
        {
            Session = session;
            Session.Advanced.UseOptimisticConcurrency = true;
            // RavenUow.Session.Advanced.SetTransactionMode(TransactionMode.ClusterWide); // not works with UseOptimisticConcurrency todo: make configurable
        }

        public IAsyncDocumentSession Session { get; }

        public Task SaveChangesAsync(CancellationToken token = default) => Session.SaveChangesAsync(token);

        public Task BeginTransactionAsync(CancellationToken token = default) => Task.CompletedTask;
    }
}