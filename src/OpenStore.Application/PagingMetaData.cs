using System;

namespace OpenStore.Application;

public class PagingMetaData
{
    public PagingMetaData()
    {
    }

    public PagingMetaData(int currentPage, int? pageSize, long totalCount)
    {
        CurrentPage = currentPage;
        if (pageSize != null)
        {
            TotalPages = (int) Math.Ceiling(totalCount / (double) pageSize.Value);
        }

        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public int CurrentPage { get; set; }
    public int? TotalPages { get; set; }
    public int? PageSize { get; set; }
    public long TotalCount { get; set; }

    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
}