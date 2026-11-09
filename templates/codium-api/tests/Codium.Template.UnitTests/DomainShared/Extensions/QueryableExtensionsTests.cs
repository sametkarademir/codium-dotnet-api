using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.Domain.Shared.Querying;

namespace Codium.Template.UnitTests.DomainShared.Extensions;

public class QueryableExtensionsTests
{
    private sealed class Item(string name, int rank) : IEntity
    {
        public string Name { get; } = name;
        public int Rank { get; } = rank;
    }

    private static IQueryable<Item> Items() =>
        new[] { new Item("b", 2), new Item("c", 3), new Item("a", 1) }.AsQueryable();

    [Fact]
    public void WhereIf_TrueCondition_AppliesThePredicate()
    {
        var result = Items().WhereIf(true, i => i.Rank > 1).Select(i => i.Name);

        Assert.Equal(["b", "c"], result);
    }

    [Fact]
    public void WhereIf_FalseCondition_LeavesTheQueryUntouched()
    {
        var result = Items().WhereIf(false, i => i.Rank > 1);

        Assert.Equal(3, result.Count());
    }

    [Theory]
    [InlineData(SortOrderTypes.Asc, new[] { "a", "b", "c" })]
    [InlineData(SortOrderTypes.Desc, new[] { "c", "b", "a" })]
    public void ApplySort_ByField_OrdersInTheRequestedDirection(SortOrderTypes order, string[] expected)
    {
        var result = Items().ApplySort("Name", order).Select(i => i.Name);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ApplySort_WithoutAField_KeepsTheOriginalOrder()
    {
        Assert.Equal(["b", "c", "a"], Items().ApplySort((string?)null).Select(i => i.Name));
        Assert.Equal(["b", "c", "a"], Items().ApplySort("  ").Select(i => i.Name));
    }

    [Fact]
    public void ApplySort_WithSortRequest_UsesItsFieldAndOrder()
    {
        var result = Items().ApplySort(new SortRequest("Rank", SortOrderTypes.Asc)).Select(i => i.Name);

        Assert.Equal(["a", "b", "c"], result);
    }

    [Fact]
    public void ApplySort_WithNullSortRequest_KeepsTheOriginalOrder()
    {
        Assert.Equal(["b", "c", "a"], Items().ApplySort((SortRequest?)null).Select(i => i.Name));
    }
}
