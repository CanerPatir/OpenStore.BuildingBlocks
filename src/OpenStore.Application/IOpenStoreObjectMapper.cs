using System.Collections.Generic;

namespace OpenStore.Application
{
    public interface IOpenStoreObjectMapper
    {
        TDestination Map<TDestination>(object source);
        IEnumerable<TDestination> MapAll<TDestination>(IEnumerable<object> source);
        TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
    }
}