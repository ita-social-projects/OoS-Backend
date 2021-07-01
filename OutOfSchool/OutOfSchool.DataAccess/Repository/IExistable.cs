namespace OutOfSchool.Services.Repository
{
    public interface IExistable<T>
    {
        /// <summary>
        /// Checks entity elements for uniqueness.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <returns>Bool.</returns>
        bool SameExists(T entity);
    }
}
