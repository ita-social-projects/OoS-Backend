using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace OutOfSchool.WebApi.Util.Transactions
{
    public static class TransactionHelper
    {
        public static IDbContextTransaction ReturnNewTransaction(this DbContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));
            return context.Database.BeginTransaction();
        }

        public static async Task<T> RunOperationWithAutoCommitOrRollBackTransactionAsync<T>(this ITransactionProcessor transactionProcessor, Func<Task<T>> operation)
        {
            _ = transactionProcessor ?? throw new ArgumentNullException(nameof(transactionProcessor));
            _ = operation ?? throw new ArgumentNullException(nameof(operation));

            try
            {
                var result = await operation().ConfigureAwait(false);
                transactionProcessor.Commit();
                return result;
            }
            catch
            {
                transactionProcessor.Rollback();
                throw;
            }
        }

        public static async Task<T> RunTransactionWithAutoCommitOrRollBackAsync<T>(this IDistributedTransactionProcessor transactionProcessor, DbContextName[] dbContextNames, Func<Task<T>> operation)
        {
            _ = transactionProcessor ?? throw new ArgumentNullException(nameof(transactionProcessor));
            _ = operation ?? throw new ArgumentNullException(nameof(operation));

            transactionProcessor.AddTransactions(dbContextNames);

            try
            {
                var result = await operation().ConfigureAwait(false);
                transactionProcessor.Commit();
                return result;
            }
            catch
            {
                transactionProcessor.Rollback();
                throw;
            }
        }

        public static async Task RunTransactionWithAutoCommitOrRollBackAsync(this IDistributedTransactionProcessor transactionProcessor, DbContextName[] dbContextNames, Func<Task> operation)
        {
            _ = transactionProcessor ?? throw new ArgumentNullException(nameof(transactionProcessor));
            _ = operation ?? throw new ArgumentNullException(nameof(operation));

            transactionProcessor.AddTransactions(dbContextNames);

            try
            {
                await operation().ConfigureAwait(false);
                transactionProcessor.Commit();
            }
            catch
            {
                transactionProcessor.Rollback();
                throw;
            }
        }
    }
}
