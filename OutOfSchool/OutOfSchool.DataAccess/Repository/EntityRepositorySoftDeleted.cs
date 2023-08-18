using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.AccessControl;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Extensions;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public class EntityRepositorySoftDeleted<TKey, TEntity> : EntityRepositoryBase<TKey, TEntity>, IEntityRepositorySoftDeleted<TKey, TEntity>
    where TEntity : class, IKeyedEntity<TKey>, ISoftDeleted, new()
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityRepositorySoftDeleted{TKey, TEntity}"/> class.
    /// </summary>
    /// <param name="dbContext">OutOfSchoolDbContext.</param>
    public EntityRepositorySoftDeleted(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
    }

    /// <inheritdoc/>
    public override async Task<IEnumerable<TEntity>> GetAll()
    {
        return await dbSet.Where(x => !x.IsDeleted).ToListAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override async Task<IEnumerable<TEntity>> GetAllWithDetails(string includeProperties = "")
    {
        IQueryable<TEntity> query = dbSet.Where(x => !x.IsDeleted);

        foreach (var includeProperty in includeProperties.Split(
                     new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        return await query.ToListAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override async Task<IEnumerable<TEntity>> GetByFilter(
        Expression<Func<TEntity, bool>> predicate,
        string includeProperties = "")
    {
        predicate = GetWhereExpression(predicate);
        return await base.GetByFilter(predicate, includeProperties).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override IQueryable<TEntity> GetByFilterNoTracking(
        Expression<Func<TEntity, bool>> predicate,
        string includeProperties = "")
    {
        predicate = GetWhereExpression(predicate);
        return base.GetByFilterNoTracking(predicate, includeProperties);
    }

    /// <inheritdoc/>
    public override Task<TEntity> GetById(TKey id) => dbSet.FirstOrDefaultAsync(x => !x.IsDeleted && x.Id.Equals(id));

    /// <inheritdoc/>
    public override Task<int> Count(Expression<Func<TEntity, bool>> where = null)
    {
        return base.Count(GetWhereExpression(where));
    }

    public override Task<bool> Any(Expression<Func<TEntity, bool>> where = null)
    {
        return base.Any(GetWhereExpression(where));
    }

    public override IQueryable<TEntity> Get(
        int skip = 0,
        int take = 0,
        string includeProperties = "",
        Expression<Func<TEntity, bool>> where = null,
        Dictionary<Expression<Func<TEntity, object>>, SortDirection> orderBy = null,
        bool asNoTracking = false)
    {
        return base.Get(skip, take, includeProperties, GetWhereExpression(where), orderBy, asNoTracking);
    }

    private Expression<Func<TEntity, bool>> GetWhereExpression(Expression<Func<TEntity, bool>> where)
    {
        if (where != null)
        {
            Expression<Func<TEntity, bool>> right = x => !x.IsDeleted;
            where = Expression.Lambda<Func<TEntity, bool>>(
                Expression.AndAlso(
                    where.Body,
                    new ExpressionParameterReplacer(right.Parameters, where.Parameters).Visit(right.Body)
                    ),
                where.Parameters);
        }
        else
        {
            where = x => !x.IsDeleted;
        }

        return where;
    }

    private class ExpressionParameterReplacer : ExpressionVisitor
    {
        private IDictionary<ParameterExpression, ParameterExpression> ParameterReplacements { get; set; }

        public ExpressionParameterReplacer(
            IList<ParameterExpression> fromParameters,
            IList<ParameterExpression> toParameters)
        {
            ParameterReplacements = new Dictionary<ParameterExpression, ParameterExpression>();

            for (int i = 0; i != fromParameters.Count && i != toParameters.Count; i++)
            { ParameterReplacements.Add(fromParameters[i], toParameters[i]); }
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            ParameterExpression replacement;

            if (ParameterReplacements.TryGetValue(node, out replacement))
            { node = replacement; }

            return base.VisitParameter(node);
        }
    }
}
