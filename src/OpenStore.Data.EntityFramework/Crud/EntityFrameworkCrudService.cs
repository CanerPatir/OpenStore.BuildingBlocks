using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OpenStore.Application;
using OpenStore.Application.Crud;
using OpenStore.Domain;
using OpenStore.Data.Crud;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global

namespace OpenStore.Data.EntityFramework.Crud
{
    public class EntityFrameworkCrudService<TEntity, TDto> : CrudService<TEntity, TDto>
        where TEntity : class, IEntity
    {
        public EntityFrameworkCrudService(ICrudRepository<TEntity> repository, IOpenStoreObjectMapper mapper) : base(repository, mapper)
        {
        }

        public override async Task<PagedList<TDto>> GetAll(int? pageNumber = null, int? pageSize = null, CancellationToken cancellationToken = default)
        {
            var query = Repository.Query;

            var count = await query.CountAsync(cancellationToken);

            if (pageNumber != null && pageSize != null)
            {
                query = query.Skip((pageNumber.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .OrderBy(x => x.Id);
            }

            var items = await query
                // .AsNoTracking()
                .ToListAsync(cancellationToken);

            return new PagedList<TDto>(Mapper.MapAll<TDto>(items), count, pageNumber ?? 1, pageSize);
        }
    }
}