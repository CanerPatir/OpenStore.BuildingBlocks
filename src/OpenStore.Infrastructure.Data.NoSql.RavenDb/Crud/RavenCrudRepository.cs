using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenStore.Application.Crud;
using OpenStore.Domain;
using Raven.Client.Documents.Linq;

namespace OpenStore.Infrastructure.Data.NoSql.RavenDb.Crud
{
    public class RavenCrudRepository<TEntity> : ICrudRepository<TEntity>
        where TEntity : IEntity
    {
        public RavenCrudRepository(IRavenUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }

        protected IRavenUnitOfWork UnitOfWork { get; }

        public IQueryable<TEntity> Query => RavenQuery();

        public async Task<TEntity> GetAsync(object id, CancellationToken cancellationToken = default)
        {
            var entity = await UnitOfWork.Session.LoadAsync<TEntity>(id.ToString(), cancellationToken);
            return entity;
        }

        public async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await UnitOfWork.Session.StoreAsync(entity, cancellationToken);
            return entity;
        }

        public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return UnitOfWork.Session.StoreAsync(entity, cancellationToken);
        }

        public Task DeleteAsync(object id, CancellationToken cancellationToken = default)
        {
            UnitOfWork.Session.Delete(id.ToString());
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return UnitOfWork.SaveChangesAsync(cancellationToken);
        }

        public IRavenQueryable<TEntity> RavenQuery(string indexName = null, string collectionName = null, bool isMapReduce = false)
        {
            return UnitOfWork.Session.Query<TEntity>(indexName, collectionName, isMapReduce);
        }
    }
}