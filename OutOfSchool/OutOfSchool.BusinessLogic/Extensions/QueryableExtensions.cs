using System.Linq.Expressions;

namespace OutOfSchool.BusinessLogic.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> IncludeContactsWithCodeficatorHierarchy<T>(this IQueryable<T> queryable)
        where T : BusinessEntity, IHasContacts 
        => queryable.Include(e => e.Contacts)
            .ThenInclude(c => c.Address)
            .ThenInclude(a => a.CATOTTG)
            .ThenInclude(c => c.Parent)
            .ThenInclude(c => c.Parent)
            .ThenInclude(c => c.Parent)
            .ThenInclude(c => c.Parent);

    public static IQueryable<TEntity> IncludeNavigationPropertyContactsWithCodeficatorHierarchy<TEntity, TProperty>(
        this IQueryable<TEntity> queryable,
        Expression<Func<TEntity, TProperty>> navigationPropertyPath)
        where TEntity : class
        where TProperty : BusinessEntity, IHasContacts 
        => queryable.Include(navigationPropertyPath)
            .ThenInclude(e => e.Contacts)
            .ThenInclude(c => c.Address)
            .ThenInclude(a => a.CATOTTG)
            .ThenInclude(c => c.Parent)
            .ThenInclude(c => c.Parent)
            .ThenInclude(c => c.Parent)
            .ThenInclude(c => c.Parent);

    public static List<TOut> MapNonDeletedToList<TIn, TOut>(this IEnumerable<TIn> list, Func<TIn, TOut> map)
        where TIn : ISoftDeleted
        => list.Where(x => !x.IsDeleted).Select(map).ToList();
}