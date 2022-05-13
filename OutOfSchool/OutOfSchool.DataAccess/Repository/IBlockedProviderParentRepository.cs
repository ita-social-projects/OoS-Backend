using System.Threading.Tasks;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IBlockedProviderParentRepository : ISensitiveEntityRepository<BlockedProviderParent>
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
    }
}
