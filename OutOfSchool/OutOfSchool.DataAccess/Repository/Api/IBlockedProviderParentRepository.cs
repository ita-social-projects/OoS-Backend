using System;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.Services.Repository.Api;

public interface IBlockedProviderParentRepository : ISensitiveEntityRepositorySoftDeleted<BlockedProviderParent>
{
    /// <summary>
    /// Create entity BlockedProviderParent. Update dependent entities (IsBlocked = true).
    /// </summary>
    /// <param name="blockedProviderParent">Entity to create.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<BlockedProviderParent> Block(BlockedProviderParent blockedProviderParent);

    /// <summary>
    /// Update entity BlockedProviderParent. Update dependent entities (IsBlocked = false).
    /// </summary>
    /// <param name="blockedProviderParent">Entity to update.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<BlockedProviderParent> UnBlock(BlockedProviderParent blockedProviderParent);

    /// <summary>
    /// Retrieves a queryable collection of BlockedProviderParent entities based on the provided parent and provider IDs.
    /// </summary>
    /// <param name="parentId">The unique identifier of the parent entity.</param>
    /// <param name="providerId">The unique identifier of the provider entity.</param>
    /// <returns>A <see cref="IQueryable{TResult}"/> representing the result of the operation.</returns>
    IQueryable<BlockedProviderParent> GetBlockedProviderParentEntities(Guid parentId, Guid providerId);
}