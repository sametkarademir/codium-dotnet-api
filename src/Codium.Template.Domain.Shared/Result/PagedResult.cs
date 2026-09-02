using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.Result;

public class PagedResult<T> : IEntityDto where T : IEntityDto
{
    public List<T> Data { get; set; }
    public PageableMeta Meta { get; set; }

    public PagedResult()
    {
        
    }
    
    public PagedResult(List<T> data, int totalCount, int page, int perPage)
    {
        Data = data;
        Meta = new PageableMeta(totalCount, page, perPage);
    }
}

public record PageableMeta
{
    public int CurrentPage { get; set; }
    public int? PreviousPage { get; set; }
    public int? NextPage { get; set; }
    public int PerPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool IsFirstPage { get; set; }
    public bool IsLastPage { get; set; }

    public PageableMeta()
    {
        
    }
    
    public PageableMeta(int totalCount, int page, int perPage)
    {
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / perPage);
        var previousPage = page - 1 < 1 ? null : page - 1 > totalPages ? (int?)null : page - 1;
        var nextPage = page + 1 > totalPages ? (int?)null : page + 1;
        var isFirstPage = page == 1;
        var isLastPage = page == totalPages || totalPages == 0;

        CurrentPage = page;
        PreviousPage = previousPage;
        NextPage = nextPage;
        PerPage = perPage;
        TotalPages = totalPages;
        TotalCount = totalCount;
        IsFirstPage = isFirstPage;
        IsLastPage = isLastPage;
    }
}