using OpenStore.Domain;
using OpenStore.Shared;

namespace OpenStore.Application.Crud;

public interface ICrudService<TEntity, TDto>
    where TEntity : IEntity
{
    ICrudRepository<TEntity> Repository { get; }
    Task<object> Create(TDto model, CancellationToken cancellationToken = default);
    Task Update(object id, TDto model, CancellationToken cancellationToken = default);
    Task Update(TEntity entity, CancellationToken cancellationToken = default);
    Task Delete(object id, CancellationToken cancellationToken = default);
    Task<TDto> Get(object id, CancellationToken cancellationToken = default);
    Task<PagedList<TDto>> GetAll(int? pageNumber = null, int? pageSize = null, CancellationToken cancellationToken = default);
}