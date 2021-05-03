using System;
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
        where TEntity : Entity<string>, ISavingChanges
    {
        public MongoCrudRepository(IMongoUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
            MongoCollection = UnitOfWork.DatabaseBase.GetCollection<TEntity>(typeof(TEntity).Name);
        }

        private IMongoUnitOfWork UnitOfWork { get; }
        private IMongoCollection<TEntity> MongoCollection { get; }
        private IMongoQueryable<TEntity> MongoQuery => MongoCollection.AsQueryable();
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
            entity.OnSavingChanges();
            await MongoCollection.ReplaceOneAsync(
                UnitOfWork.Session,
                x => x.Id == entity.Id && x.Version == version,
                entity,
                new ReplaceOptions(), cancellationToken);
        }

        public Task RemoveByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            return MongoCollection.DeleteOneAsync(UnitOfWork.Session, x => x.Id == id.ToString(), new DeleteOptions(), cancellationToken);
        }

        public void Remove(TEntity entity)
        {
            MongoCollection.DeleteOne(UnitOfWork.Session, x => x.Id == entity.Id);
        }

        public void Attach(TEntity entity) => throw new NotSupportedException();

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return UnitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}