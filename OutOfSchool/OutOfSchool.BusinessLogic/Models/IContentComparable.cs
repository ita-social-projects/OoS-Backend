namespace OutOfSchool.BusinessLogic.Models;

/// <summary>
/// Defines a mechanism for content-based comparison between an implementing DTO and another object of type <typeparamref name="TEntity"/>.
/// </summary>
/// <typeparam name="TEntity">
/// The type of the entity against which the implementing object’s content will be compared.
/// </typeparam>
public interface IContentComparable<in TEntity>
{
    /// <summary>
    /// Determines whether the current object's content is equal to the specified object’s content.
    /// </summary>
    /// <param name="other">
    /// The object of type <typeparamref name="TEntity"/> to compare with the current object.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the content of the two objects is considered equal; otherwise, <see langword="false"/>.
    /// </returns>
    bool ContentEquals(TEntity other);
}