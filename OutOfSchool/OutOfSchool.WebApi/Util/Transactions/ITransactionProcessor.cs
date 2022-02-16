using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace OutOfSchool.WebApi.Util.Transactions
{
    public interface ITransactionProcessor
    {
        IDbContextTransaction CurrentTransaction { get; }

        void Commit();

        void Rollback();
    }
}
