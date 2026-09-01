using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Codium.Template.Domain.Shared.Querying;
using Microsoft.EntityFrameworkCore;

namespace Codium.Template.Domain.Shared.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, bool>> predicate)
    {
        return condition ? query.Where(predicate) : query;
    }
    
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> queryable,
        SortRequest? sort,
        CancellationToken cancellationToken = default
    ) where T : IEntity
    {
        return sort == null 
            ? queryable 
            : queryable.ApplySort(sort.Field, sort.Order, cancellationToken);
    }

    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> queryable,
        string? field = null,
        SortOrderTypes orderType = SortOrderTypes.Desc,
        CancellationToken cancellationToken = default
    ) where T : IEntity
    {
        return string.IsNullOrWhiteSpace(field)
            ? queryable
            : queryable.OrderBy($"{field} {orderType.ToString()}", cancellationToken);
    }

    public static async Task<PagedList<T>> ToPageableAsync<T>(
        this IQueryable<T> queryable,
        int page,
        int perPage,
        CancellationToken cancellationToken = default
    ) where T : IEntity
    {
        var count = await queryable.CountAsync(cancellationToken).ConfigureAwait(false);

        if (count == 0)
        {
            return new PagedList<T>([], 0, page, perPage);
        }

        var items = await queryable
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedList<T>(items, count, page, perPage);
    }
}