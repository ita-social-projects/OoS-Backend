
namespace OutOfSchool.BusinessLogic.Services;
/// <summary>
/// Provides methods to execute actions within a database transaction scope.
/// </summary>
public interface ITransactionManagerService
{
    /// <summary>
    /// Executes the specified asynchronous action within a transaction.
    /// If the action completes successfully, the transaction is committed;
    /// if an exception occurs, the transaction is rolled back.
    /// </summary>
    /// <param name="action">The asynchronous action to execute inside the transaction.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ExecuteInTransactionAsync(Func<Task> action);

    /// <summary>
    /// Executes the specified asynchronous action within a transaction and returns a result.
    /// If the action completes successfully, the transaction is committed and the result is returned;
    /// if an exception occurs, the transaction is rolled back.
    /// </summary>
    /// <typeparam name="T">The type of the result returned by the action.</typeparam>
    /// <param name="action">The asynchronous action that returns a result to execute inside the transaction.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing the result of the action.</returns>
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action);
}

