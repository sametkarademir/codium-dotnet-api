using System.Linq.Expressions;

namespace Codium.Template.Domain.Shared.Extensions;

public static class LinqExtensions
{
    public static IEnumerable<T> WhereIf<T>(
        this IEnumerable<T> source,
        bool condition,
        Func<T, bool> predicate)
    {
        return condition ? source.Where(predicate) : source;
    }
}