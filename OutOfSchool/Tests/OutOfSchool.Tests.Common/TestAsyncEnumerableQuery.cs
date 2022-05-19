using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

namespace OutOfSchool.Tests.Common
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> AsTestAsyncEnumerableQuery<T>(this IEnumerable<T> input)
        {
            return new TestAsyncEnumerableQuery<T>(input);
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
                return Execute<TResult>(expression);
            }
        }
    }
}