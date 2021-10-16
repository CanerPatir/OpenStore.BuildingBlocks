using AutoMapper;
using OpenStore.Application;

namespace OpenStore.Infrastructure.Mapping.AutoMapper;

public class AutoMapperOpenStoreObjectMapper : IOpenStoreObjectMapper
{
    private readonly IMapper _mapper;

    public AutoMapperOpenStoreObjectMapper(IMapper mapper)
    {
        _mapper = mapper;
    }

    public TDestination Map<TDestination>(object source) => _mapper.Map<TDestination>(source);
    public IEnumerable<TDestination> MapAll<TDestination>(IEnumerable<object> source) => Map<IEnumerable<TDestination>>(source);
    public TDestination Map<TSource, TDestination>(TSource source, TDestination existingDestination) => _mapper.Map(source, existingDestination);
}