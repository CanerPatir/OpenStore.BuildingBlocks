using System;
using System.Collections.Generic;

namespace OpenStore.Application;

public class PagedList<T>
{
    public PagingMetaData PageMeta { get; set; }

    public IEnumerable<T> Items { get; set; }

    public PagedList()
    {
    }

    public PagedList(IEnumerable<T> items, long count, int pageNumber, int? pageSize)
    {
        PageMeta = new PagingMetaData(pageNumber, pageSize, count);
        Items = items ?? throw new ArgumentNullException(nameof(items));
    }
}