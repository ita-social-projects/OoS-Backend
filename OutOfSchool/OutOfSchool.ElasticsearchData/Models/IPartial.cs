namespace OutOfSchool.ElasticsearchData.Models;

/// <summary>
/// Interface for partial update of an entity.
/// </summary>
/// <typeparam name="TEntity">Lock type to prevent using wrong object to update different documents.</typeparam>
public interface IPartial<TEntity>
    where TEntity : class, new()
{
}