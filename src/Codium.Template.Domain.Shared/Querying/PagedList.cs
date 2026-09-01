using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.Querying;

public class PagedList<T> where T : IEntity
{
    public List<T> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PerPage { get; set; }

    public PagedList()
    {
        
    }
    
    public PagedList(List<T> data, int totalCount, int page, int perPage)
    {
        Data = data;
        TotalCount = totalCount;
        Page = page;
        PerPage = perPage;
    }
}