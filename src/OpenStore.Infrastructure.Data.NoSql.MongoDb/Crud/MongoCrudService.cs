using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using OpenStore.Application;
using OpenStore.Application.Crud;
using OpenStore.Domain;
using OpenStore.Infrastructure.Data.Crud;

namespace OpenStore.Infrastructure.Data.NoSql.MongoDb.Crud
{
    public class MongoCrudService<TEntity, TDto> : CrudService<TEntity, TDto>
        where TEntity : Entity<string>
    {
        public MongoCrudService(ICrudRepository<TEntity> repository, IOpenStoreObjectMapper mapper) : base(repository, mapper)
        {
        }

        public override async Task<PagedList<TDto>> GetAll(int? pageNumber = null, int? pageSize = null, CancellationToken cancellationToken = default)
        {
            var query = (IMongoQueryable<TEntity>)Repository.Query;

            var count = await query.CountAsync(cancellationToken);

            if (pageNumber != null && pageSize != null)
                query = query.Skip((pageNumber.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .OrderBy(x => x.Id);

            var items = await query.ToListAsync(cancellationToken);

            return new PagedList<TDto>(Mapper.MapAll<TDto>(items), count, pageNumber ?? 1, pageSize);
        }
    }
}