namespace OutOfSchool.ElasticsearchData.Models;

/// <summary>
/// Interface for partial update of an entity.
/// </summary>
/// <typeparam name="TEntity">Lock type to prevent using wrong object to update different documents.</typeparam>
#pragma warning disable S2326
public interface IPartial<TEntity>
    where TEntity : class, new()
{
}
#pragma warning restore S2326
