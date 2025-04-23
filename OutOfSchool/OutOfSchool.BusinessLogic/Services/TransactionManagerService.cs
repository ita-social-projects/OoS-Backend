
namespace OutOfSchool.BusinessLogic.Services;
public class TransactionManagerService : ITransactionManagerService
{
    private readonly OutOfSchoolDbContext _dbContext;
    private readonly ILogger<TransactionManagerService> _logger;

    public TransactionManagerService(OutOfSchoolDbContext dbContext, ILogger<TransactionManagerService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        await ExecuteInTransactionAsync<object>(async () =>
        {
            await action().ConfigureAwait(false);
            return null;
        });
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            _logger.LogDebug("Transaction started.");
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var result = await action().ConfigureAwait(false); ;
                await transaction.CommitAsync().ConfigureAwait(false);

                _logger.LogDebug("Transaction committed successfully.");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction failed and will be rolled back.");
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        });
    }
}

