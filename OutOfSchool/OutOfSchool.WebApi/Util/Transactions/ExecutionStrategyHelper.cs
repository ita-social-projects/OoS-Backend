using Microsoft.EntityFrameworkCore.Storage;

namespace OutOfSchool.WebApi.Util.Transactions
{
    public class ExecutionStrategyHelper : IExecutionStrategyHelper
    {
        private readonly ContextResolver contextResolver;

        public ExecutionStrategyHelper(ContextResolver contextResolver)
        {
            this.contextResolver = contextResolver;
        }

        public IExecutionStrategy CreateStrategyByDbName(DbContextName dbContextName)
        {
            var dbContext = contextResolver.GetRequiredContext(dbContextName);
            return dbContext.Database.CreateExecutionStrategy();
        }
    }
}
