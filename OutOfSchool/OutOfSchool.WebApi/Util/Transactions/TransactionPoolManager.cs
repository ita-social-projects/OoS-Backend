using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace OutOfSchool.WebApi.Util.Transactions
{
    /// <summary>
    /// Transaction pool helper.
    /// </summary>
    public sealed class TransactionPoolManager
    {
        #region Private fields

        /// <summary>
        /// Holds a pool of transactions.
        /// </summary>
        private readonly List<IDbContextTransaction> transactions = new List<IDbContextTransaction>();

        #endregion

        #region Public methods

        /// <summary>
        /// Add a new transaction to the pool if not exists, otherwise use existing one.
        /// </summary>
        /// <param name="db">Database context.</param>
        public void AddTransaction(
            DbContext db)
        {
            var transaction = transactions.FirstOrDefault(c => c.GetDbTransaction().Connection.Equals(db?.Database.GetDbConnection()));

            if (transaction != null)
            {
                transactions.Remove(transaction);
                transactions.Insert(0, transaction);
            }
            else
            {
                var newTransaction = db.ReturnNewTransaction();
                transactions.Insert(0, newTransaction);
            }
        }

        /// <summary>
        /// Get the first top transaction from stack without removing it.
        /// </summary>
        /// <returns>The instance of <see cref="IDbContextTransaction"/>.</returns>
        public IDbContextTransaction Peek()
        {
            return transactions.First();
        }

        /// <summary>
        /// Commits all transaction that are currently in the pool.
        /// </summary>
        public void CommitAll()
        {
            foreach (var dbTransaction in transactions)
            {
                DbManagerHelper.CommitTransaction(dbTransaction);
            }

            transactions.Clear();
        }

        /// <summary>
        /// Rollbacks all transaction that are currently in the pool.
        /// </summary>
        public void RollbackAll()
        {
            foreach (var dbTransaction in transactions)
            {
                DbManagerHelper.RollbackTransaction(dbTransaction);
            }

            transactions.Clear();
        }

        #endregion
    }
}
