using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Util
{
    /// <summary>
    /// Enables the efficient, dynamic composition of query predicates.
    /// </summary>
    public static class PredicateBuilder
    {
        /// <summary>
        /// Creates a predicate that evaluates to true.
        /// </summary>
        /// <typeparam name="T">Type of our entity for what we creating LINQ request.</typeparam>
        /// <returns>True.</returns>
        public static Expression<Func<T, bool>> True<T>()
        {
            return param => true;
        }

        /// <summary>
        /// Creates a predicate that evaluates to false.
        /// </summary>
        /// <typeparam name="T">Type of our entity for what we creating LINQ request.</typeparam>
        /// <returns>False.</returns>
        public static Expression<Func<T, bool>> False<T>()
        {
            return param => false;
        }

        /// <summary>
        /// Creates a predicate expression from the specified lambda expression.
        /// </summary>
        /// <typeparam name="T">Type of our entity for what we creating LINQ request.</typeparam>
        /// <param name="predicate">Our predicate for entity that we want to create.</param>
        /// <returns>Created predicate.</returns>
        public static Expression<Func<T, bool>> Create<T>(Expression<Func<T, bool>> predicate)
        {
            return predicate;
        }

        /// <summary>
        /// Combines the first predicate with the second using the logical "and".
        /// </summary>
        /// <typeparam name="T">Type of our entity for what we creating LINQ request.</typeparam>
        /// <param name="first">First predicate.</param>
        /// <param name="second">Second predicate.</param>
        /// <returns>Derived predicate.</returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.AndAlso);
        }

        /// <summary>
        /// Combines the first predicate with the second using the logical "or".
        /// </summary>
        /// <typeparam name="T">Type of our entity for what we creating LINQ request.</typeparam>
        /// <param name="first">First predicate.</param>
        /// <param name="second">Second predicate.</param>
        /// <returns>Derived predicate.</returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.OrElse);
        }

        /// <summary>
        /// Negates the predicate.
        /// </summary>
        /// <typeparam name="T">Type of our entity for what we creating LINQ request.</typeparam>
        /// <param name="expression">Our expression for what we want to apply the operation.</param>
        /// <returns>Derived predicate.</returns>
        public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expression)
        {
            var negated = Expression.Not(expression.Body);
            return Expression.Lambda<Func<T, bool>>(negated, expression.Parameters);
        }

        /// <summary>
        /// Combines the first expression with the second using the specified merge function.
        /// </summary>
        /// <typeparam name="T">Type of our entity for what we creating LINQ request.</typeparam>
        /// <param name="first">First predicate.</param>
        /// <param name="second">Second predicate.</param>
        /// <param name="merge">Merge predicate.</param>
        /// <returns>Derived predicate.</returns>
        public static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
        {
            // zip parameters (map from parameters of second to parameters of first)
            var map = first.Parameters
                .Select((f, i) => new { f, s = second.Parameters[i] })
                .ToDictionary(p => p.s, p => p.f);

            // replace parameters in the second lambda expression with the parameters in the first
            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);

            // create a merged lambda expression with parameters from the first expression
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }

        private class ParameterRebinder : ExpressionVisitor
        {
            private readonly Dictionary<ParameterExpression, ParameterExpression> map;

            public ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
            {
                this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
            }

            public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
            {
                return new ParameterRebinder(map).Visit(exp);
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                ParameterExpression replacement;

                if (map.TryGetValue(node, out replacement))
                {
                    node = replacement;
                }

                return base.VisitParameter(node);
            }
        }
    }
}
