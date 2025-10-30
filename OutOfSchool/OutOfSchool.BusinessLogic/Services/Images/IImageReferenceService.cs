using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.BusinessLogic.Services.Images;

public interface IImageReferenceService<in TEntity>
    where TEntity : class, IKeyedEntity, IImageDependentEntity<TEntity>, new()
{
    /// <summary>
    /// Asynchronously counting references to the image.
    /// </summary>
    /// <param name="externalStorageId">Image Id in the external storage.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the count of the references to the image.</returns>
    Task<int> CountReferencesAsync(string externalStorageId, CancellationToken ct = default);

    /// <summary>
    /// Asynchronously counting references to the images.
    /// </summary>
    /// <param name="externalStorageIds">The image IDs for which we should count references.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the count of the references to the images in the dictionary.</returns>
    Task<IDictionary<string, int>> CountReferencesAsync(IEnumerable<string> externalStorageIds, CancellationToken ct = default);
}
