using System.Threading;
using System.Threading.Tasks;
using OpenStore.Application;
using OpenStore.Application.Crud;
using OpenStore.Application.Exceptions;
using OpenStore.Domain;

namespace OpenStore.Data.Crud;

public abstract class CrudService<TEntity, TDto> : ICrudService<TEntity, TDto>
    where TEntity : class, IEntity
{
    protected CrudService(ICrudRepository<TEntity> repository, IOpenStoreObjectMapper mapper)
    {
        Mapper = mapper;
        Repository = repository;
    }

    protected IOpenStoreObjectMapper Mapper { get; }

    public ICrudRepository<TEntity> Repository { get; }

    public virtual async Task<object> Create(TDto model, CancellationToken cancellationToken = default)
    {
        var entity = Mapper.Map<TEntity>(model);
        await Repository.InsertAsync(entity, cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public virtual async Task Update(object id, TDto model, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new ResourceNotFoundException();
        var updatedEntity = Mapper.Map(model, entity);

        await Update(updatedEntity, cancellationToken);
    }

    public async Task Update(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Repository.UpdateAsync(entity, cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task Delete(object id, CancellationToken cancellationToken = default)
    {
        await Repository.RemoveByIdAsync(id, cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task<TDto> Get(object id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken);
        if (entity == default)
        {
            throw new ResourceNotFoundException();
        }

        return Mapper.Map<TDto>(entity);
    }

    public abstract Task<PagedList<TDto>> GetAll(int? pageNumber = null, int? pageSize = null, CancellationToken cancellationToken = default);
}