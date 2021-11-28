using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Application;
using OpenStore.Application.Crud;
using OpenStore.Domain;
using OpenStore.Shared;

namespace OpenStore.Infrastructure.Web;

public abstract class BaseCrudApiController<TEntity, TKey, TDto> : BaseApiController
    where TEntity : IEntity
{
    protected ICrudService<TEntity, TDto> CrudService { get; }

    protected BaseCrudApiController(IServiceProvider serviceProvider) : this(serviceProvider.GetRequiredService<ICrudService<TEntity, TDto>>())
    {
    }

    protected BaseCrudApiController(ICrudService<TEntity, TDto> crudService)
    {
        CrudService = crudService;
    }

    [HttpGet]
    public virtual Task<PagedList<TDto>> GetAll([FromQuery] int? pageNumber, [FromQuery] int? pageSize) 
        => CrudService.GetAll(pageNumber, pageSize, CancellationToken);

    [HttpGet("{id}")]
    public virtual Task<TDto> Get(TKey id) => CrudService.Get(id, CancellationToken);

    [HttpPut("{id}")]
    public virtual Task Update(TKey id, [FromBody] TDto dto)
    {
        ThrowIfModelInvalid();
        return CrudService.Update(id, dto, CancellationToken);
    }

    [HttpPost]
    public virtual Task<object> Create([FromBody] TDto dto)
    {
        ThrowIfModelInvalid();
        return CrudService.Create(dto, CancellationToken);
    }

    [HttpDelete("{id}")]
    public virtual Task Delete(TKey id) => CrudService.Delete(id, CancellationToken);
}