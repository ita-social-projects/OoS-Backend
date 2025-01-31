namespace OutOfSchool.ExternalFileStore;

public interface IObjectStorageSynchronizationService
{
    /// <summary>
    /// Asynchronously synchronizes gcp files with a database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task SynchronizeAsync(CancellationToken cancellationToken = default);
}