namespace OutOfSchool.ExternalFileStore;

public interface IObjectImagesSyncDataRepository
{
    /// <summary>
    /// Asynchronously gets an intersect between WorkshopCoverImages ids and the given collection ids.
    /// </summary>
    /// <param name="searchIds">Collection to find intersect with.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a list of <see cref="string"/>.</returns>
    Task<List<string>> GetIntersectWorkshopCoverImagesIds(IEnumerable<string> searchIds);

    /// <summary>
    /// Asynchronously gets an intersect between TeacherCoverImages ids and the given collection ids.
    /// </summary>
    /// <param name="searchIds">Collection to find intersect with.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a list of <see cref="string"/>.</returns>
    Task<List<string>> GetIntersectTeacherCoverImagesIds(IEnumerable<string> searchIds);

    /// <summary>
    /// Asynchronously gets an intersect between ProviderCoverImages ids and the given collection ids.
    /// </summary>
    /// <param name="searchIds">Collection to find intersect with.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a list of <see cref="string"/>.</returns>
    Task<List<string>> GetIntersectProviderCoverImagesIds(IEnumerable<string> searchIds);

    /// <summary>
    /// Asynchronously gets an intersect between WorkshopImages ids and the given collection ids.
    /// </summary>
    /// <param name="searchIds">Collection to find intersect with.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a list of <see cref="string"/>.</returns>
    Task<List<string>> GetIntersectWorkshopImagesIds(IEnumerable<string> searchIds);

    /// <summary>
    /// Asynchronously gets an intersect between ProviderImages ids and the given collection ids.
    /// </summary>
    /// <param name="searchIds">Collection to find intersect with.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a list of <see cref="string"/>.</returns>
    Task<List<string>> GetIntersectProviderImagesIds(IEnumerable<string> searchIds);
}