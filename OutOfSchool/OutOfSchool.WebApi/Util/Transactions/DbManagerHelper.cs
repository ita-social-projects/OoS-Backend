using System;
using Microsoft.EntityFrameworkCore.Storage;

namespace OutOfSchool.WebApi.Util.Transactions
{
    public static class DbManagerHelper
    {
        public static void CommitTransaction(IDbContextTransaction transaction)
        {
            _ = transaction ?? throw new ArgumentNullException(nameof(transaction));
            transaction.Commit();
            transaction.Dispose();
        }

        public static void RollbackTransaction(IDbContextTransaction transaction)
        {
            _ = transaction ?? throw new ArgumentNullException(nameof(transaction));
            transaction.Rollback();
            transaction.Dispose();
        }
    }
}
