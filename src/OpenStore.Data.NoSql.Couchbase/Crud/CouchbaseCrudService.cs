using OpenStore.Application;
using OpenStore.Application.Crud;
using OpenStore.Data.Crud;
using OpenStore.Domain;
using OpenStore.Shared;

namespace OpenStore.Data.NoSql.Couchbase.Crud;

public class CouchbaseCrudService<TEntity, TDto> : CrudService<TEntity, TDto>
    where TEntity : Entity<string>
{
    public CouchbaseCrudService(ICrudRepository<TEntity> repository, IOpenStoreObjectMapper mapper) : base(repository, mapper)
    {
    }

    public override Task<PagedList<TDto>> GetAll(int? pageNumber = null, int? pageSize = null, CancellationToken cancellationToken = default) => throw new NotSupportedException();
}