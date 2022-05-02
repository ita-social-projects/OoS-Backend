using System.Threading.Tasks;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public class BlockedProviderParentRepository : SensitiveEntityRepository<BlockedProviderParent>, IBlockedProviderParentRepository
    {
        private readonly OutOfSchoolDbContext db;

        public BlockedProviderParentRepository(OutOfSchoolDbContext dbContext)
            : base(dbContext)
        {
            db = dbContext;
        }

        /// <inheritdoc/>
        public Task<BlockedProviderParent> Block(BlockedProviderParent blockedProviderParent)
        {
            throw new System.NotImplementedException();
        }
    }
}
