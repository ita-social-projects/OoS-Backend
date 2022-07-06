using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public class BlockedProviderParentRepository : SensitiveEntityRepository<BlockedProviderParent>, IBlockedProviderParentRepository
{
    private readonly OutOfSchoolDbContext db;

    public BlockedProviderParentRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
        db = dbContext;
    }

    /// <inheritdoc/>
    public async Task<BlockedProviderParent> Block(BlockedProviderParent blockedProviderParent)
    {
        await SetApplicationsAndChatRoomsBlock(
            blockedProviderParent.ParentId,
            blockedProviderParent.ProviderId,
            true);

        var currentBlockedProviderParent = db.BlockedProviderParents.Add(blockedProviderParent);

        await db.SaveChangesAsync();

        return await Task.FromResult(currentBlockedProviderParent.Entity);
    }

    /// <inheritdoc/>
    public async Task<BlockedProviderParent> UnBlock(BlockedProviderParent blockedProviderParent)
    {
        await SetApplicationsAndChatRoomsBlock(
            blockedProviderParent.ParentId,
            blockedProviderParent.ProviderId,
            false);

        var currentBlock = await GetById(blockedProviderParent.Id);

        dbContext.Entry(currentBlock).CurrentValues.SetValues(blockedProviderParent);
        dbContext.Entry(currentBlock).State = EntityState.Modified;

        await db.SaveChangesAsync();

        return await Task.FromResult(blockedProviderParent);
    }

    private async Task SetApplicationsAndChatRoomsBlock(Guid parentId, Guid providerId, bool block)
    {
        var applications = db.Applications.Where(a => a.ParentId == parentId
                                                      && a.Workshop.ProviderId == providerId);
        await applications.ForEachAsync(a => a.IsBlocked = block);

        var chatRooms = db.ChatRoomWorkshops.Where(c => c.ParentId == parentId
                                                        && c.Workshop.ProviderId == providerId);
        await chatRooms.ForEachAsync(a => a.IsBlocked = block);
    }
}