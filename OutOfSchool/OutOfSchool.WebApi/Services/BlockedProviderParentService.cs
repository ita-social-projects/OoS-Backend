using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models.BlockedProviderParent;

namespace OutOfSchool.WebApi.Services
{
    public class BlockedProviderParentService : IBlockedProviderParentService
    {
        private readonly IBlockedProviderParentRepository blockedProviderParentRepository;
        private readonly ILogger<BlockedProviderParentService> logger;
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockedProviderParentService"/> class.
        /// </summary>
        /// <param name="blockedProviderParentRepository">Repository for the BlockedProviderParent entity.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="mapper">Mapper.</param>
        public BlockedProviderParentService(
            IBlockedProviderParentRepository blockedProviderParentRepository,
            ILogger<BlockedProviderParentService> logger,
            IMapper mapper)
        {
            this.blockedProviderParentRepository = blockedProviderParentRepository;
            this.logger = logger;
            this.mapper = mapper;
        }

        public Task<BlockedProviderParentDto> Block(BlockedProviderParentBlockDto blockedProviderParentBlockDto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsBlocked(Guid parentId, Guid providerId)
        {
            throw new NotImplementedException();
        }

        public Task<BlockedProviderParentDto> Unblock(BlockedProviderParentUnblockDto blockedProviderParentUnblockDto)
        {
            throw new NotImplementedException();
        }
    }
}
