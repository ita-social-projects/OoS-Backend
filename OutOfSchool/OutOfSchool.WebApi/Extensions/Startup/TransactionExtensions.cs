using System;
using Microsoft.Extensions.DependencyInjection;
using OutOfSchool.WebApi.Util.Transactions;

namespace OutOfSchool.WebApi.Extensions.Startup
{
    public static class TransactionExtensions
    {
        public static IServiceCollection AddTransactionProcessors(this IServiceCollection serviceCollection)
        {
            _ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

            serviceCollection.AddScoped<ContextResolver>();
            serviceCollection.AddScoped<IExecutionStrategyHelper, ExecutionStrategyHelper>();
            serviceCollection.AddTransient<IDistributedTransactionProcessor, DistributedTransaction>();

            return serviceCollection;
        }
    }
}
