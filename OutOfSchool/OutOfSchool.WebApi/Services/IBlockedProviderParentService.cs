using System;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models.BlockedProviderParent;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for BlockedProviderParent entity.
    /// </summary>
    public interface IBlockedProviderParentService
    {
        /// <summary>
        /// Create entity.
        /// </summary>
        /// <param name="blockedProviderParentBlockDto">BlockedProviderParent entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<BlockedProviderParentDto> Block(BlockedProviderParentBlockDto blockedProviderParentBlockDto);

        /// <summary>
        /// Update BlockedProviderParent entity.
        /// </summary>
        /// <param name="blockedProviderParentUnblockDto">Entity for updating BlockedProviderParent.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<BlockedProviderParentDto> Unblock(BlockedProviderParentUnblockDto blockedProviderParentUnblockDto);

        /// <summary>
        /// Check existing block.
        /// </summary>
        /// <param name="parentId">Key of the Parent in table.</param>
        /// <param name="providerId">Key of the Provider in table.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<bool> IsBlocked(Guid parentId, Guid providerId);
    }
}
