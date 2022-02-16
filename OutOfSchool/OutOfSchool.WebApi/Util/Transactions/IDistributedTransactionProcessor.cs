using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace OutOfSchool.WebApi.Util.Transactions
{
    public interface IDistributedTransactionProcessor : ITransactionProcessor
    {
        void AddTransaction(DbContextName dbContextName);

        void AddTransactions(params DbContextName[] dbContextNames);
    }
}
