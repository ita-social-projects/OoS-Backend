namespace OutOfSchool.BusinessLogic.Services.Gcp;

public interface IGcpStorageSynchronizationService
{
    /// <summary>
    /// Asynchronously synchronizes gcp files with a database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task SynchronizeAsync(CancellationToken cancellationToken = default);
}