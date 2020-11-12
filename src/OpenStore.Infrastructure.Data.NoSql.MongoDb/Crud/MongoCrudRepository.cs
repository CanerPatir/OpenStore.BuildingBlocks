using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using OpenStore.Application.Crud;
using OpenStore.Domain;

namespace OpenStore.Infrastructure.Data.NoSql.MongoDb.Crud
{
    public class MongoCrudRepository<TEntity> : ICrudRepository<TEntity>
        where TEntity : Entity<string>
    {
        public MongoCrudRepository(IMongoUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
            MongoCollection = UnitOfWork.DatabaseBase.GetCollection<TEntity>(typeof(TEntity).Name);
        }

        public IMongoUnitOfWork UnitOfWork { get; }
        public IMongoCollection<TEntity> MongoCollection { get; }
        public IMongoQueryable<TEntity> MongoQuery => MongoCollection.AsQueryable();
        public IQueryable<TEntity> Query => MongoQuery;

        public Task<TEntity> GetAsync(object id, CancellationToken cancellationToken = default)
        {
            return MongoCollection.Find(x => x.Id == id.ToString()).SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await MongoCollection.InsertOneAsync(UnitOfWork.Session, entity, new InsertOneOptions(), cancellationToken);
            return entity;
        }

        public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var version = entity.Version;
            entity.Version++;
            await MongoCollection.ReplaceOneAsync(
                UnitOfWork.Session,
                x => x.Id == entity.Id && x.Version == version,
                entity,
                new ReplaceOptions(), cancellationToken);
        }

        public Task DeleteAsync(object id, CancellationToken cancellationToken = default)
        {
            return MongoCollection.DeleteOneAsync(UnitOfWork.Session, x => x.Id == id.ToString(), new DeleteOptions(), cancellationToken);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return UnitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}