using System.Threading.Tasks;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IBlockedProviderParentRepository : ISensitiveEntityRepository<BlockedProviderParent>
    {
        /// <summary>
        /// Create entity BlockedProviderParent and update dependent entities.
        /// </summary>
        /// <param name="blockedProviderParent">User's id for notifications.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<BlockedProviderParent> Block(BlockedProviderParent blockedProviderParent);
    }
}
