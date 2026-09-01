namespace Codium.Template.Domain.Shared.Querying;

public class SortRequest
{
    public string? Field { get; set; }
    public SortOrderTypes Order { get; set; }

    public SortRequest() : this(null, SortOrderTypes.Desc)
    {
    }

    public SortRequest(string? field, SortOrderTypes order)
    {
        Field = field;
        Order = order;
    }
}

public enum SortOrderTypes
{

    Asc = 0,
    Desc = 1
}