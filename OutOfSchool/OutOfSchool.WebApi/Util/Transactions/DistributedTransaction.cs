using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace OutOfSchool.WebApi.Util.Transactions
{
    public class DistributedTransaction : IDistributedTransactionProcessor
    {
        private readonly TransactionPoolManager transactionPoolManager = new TransactionPoolManager();
        private readonly ContextResolver contextReceiver;

        public DistributedTransaction(ContextResolver contextReceiver)
        {
            this.contextReceiver = contextReceiver;
        }

        public IDbContextTransaction CurrentTransaction => transactionPoolManager.Peek();

        public void AddTransaction(DbContextName dbContextName)
        {
            var dbContext = contextReceiver.GetRequiredContext(dbContextName);
            transactionPoolManager.AddTransaction(dbContext);
        }

        public void AddTransactions(params DbContextName[] dbContextNames)
        {
            foreach (var dbContextName in dbContextNames)
            {
                AddTransaction(dbContextName);
            }
        }

        public void Commit()
        {
            transactionPoolManager.CommitAll();
        }

        public void Rollback()
        {
            transactionPoolManager.RollbackAll();
        }
    }
}
