namespace OutOfSchool.Services.Repository.Api;

public interface IExistable<in T>
{
    /// <summary>
    /// Checks entity elements for uniqueness.
    /// </summary>
    /// <param name="entity">Entity.</param>
    /// <returns>Bool.</returns>
    bool SameExists(T entity);
}