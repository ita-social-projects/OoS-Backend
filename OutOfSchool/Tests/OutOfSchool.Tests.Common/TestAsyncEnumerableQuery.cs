using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

namespace OutOfSchool.Tests.Common;

public static class QueryableExtensions
{
    public static IQueryable<T> AsTestAsyncEnumerableQuery<T>(this IEnumerable<T> input)
    {
        return new TestAsyncEnumerableQuery<T>(input);
    }

    /// <summary>
    /// Creates an empty <see cref="IQueryable{T}"/> that supports async operations (e.g. <c>ToListAsync</c>, <c>CountAsync</c>).
    /// Useful for unit testing scenarios involving asynchronous LINQ queries.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <returns>An empty async-compatible <see cref="IQueryable{T}"/>.</returns>
    public static IQueryable<T> AsEmptyTestAsyncEnumerableQuery<T>()
    {
        var list = Enumerable.Empty<T>().ToList();
        return new TestAsyncEnumerableQuery<T>(list);
    }
}

public class TestAsyncEnumerableQuery<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerableQuery(IEnumerable<T> enumerable) : base(enumerable)
    {
    }

    public TestAsyncEnumerableQuery(Expression expression) : base(expression)
    {
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new InMemoryAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new AsyncQueryProvider<T>(this);



    private class InMemoryAsyncEnumerator<TEntity> : IAsyncEnumerator<TEntity>
    {
        private readonly IEnumerator<TEntity> enumerator;

        public InMemoryAsyncEnumerator(IEnumerator<TEntity> enumerator)
        {
            this.enumerator = enumerator;
        }

        public ValueTask DisposeAsync()
        {
            enumerator.Dispose();
            return new ValueTask();
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(enumerator.MoveNext());
        }

        public TEntity Current => enumerator.Current;
    }

    private class AsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider inner;

        internal AsyncQueryProvider(IQueryProvider inner)
        {
            this.inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerableQuery<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerableQuery<TElement>(expression);
        }

        public object Execute(Expression expression)
        {
            return inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return inner.Execute<TResult>(expression);
        }

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            return new TestAsyncEnumerableQuery<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            var resultType = typeof(TResult);

            // Check if TResult is a Task<T>
            if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                // Get the T from Task<T>
                var innerType = resultType.GetGenericArguments()[0];

                // Find IQueryProvider.Execute<T>(Expression) method and make it generic for the inner type
                var executeMethod = typeof(IQueryProvider)
                    .GetMethods()
                    .Single(m =>
                        m.Name == nameof(IQueryProvider.Execute) &&
                        m.IsGenericMethod &&
                        m.GetParameters().Length == 1 &&
                        m.GetParameters()[0].ParameterType == typeof(Expression)
                    )
                    .MakeGenericMethod(innerType);

                // Execute the expression synchronously to get the result (T)
                var executionResult = executeMethod.Invoke(inner, new object[] { expression });

                // Wrap the result into a Task<T> using Task.FromResult
                var taskFromResultMethod = typeof(Task)
                    .GetMethod(nameof(Task.FromResult))
                    .MakeGenericMethod(innerType);

                return (TResult)taskFromResultMethod.Invoke(null, new[] { executionResult });
            }

            throw new InvalidOperationException("TResult is not a Task<T>");
        }
    }
}