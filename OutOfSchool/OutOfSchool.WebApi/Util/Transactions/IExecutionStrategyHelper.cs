using Microsoft.EntityFrameworkCore.Storage;

namespace OutOfSchool.WebApi.Util.Transactions
{
    public interface IExecutionStrategyHelper
    {
        IExecutionStrategy CreateStrategyByDbName(DbContextName dbContextName);
    }
}
