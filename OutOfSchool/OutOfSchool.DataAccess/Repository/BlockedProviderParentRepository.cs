using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;

namespace OutOfSchool.Services.Repository;

public class BlockedProviderParentRepository : SensitiveEntityRepositorySoftDeleted<BlockedProviderParent>, IBlockedProviderParentRepository
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

        dbContext.Entry(blockedProviderParent).CurrentValues.SetValues(blockedProviderParent);
        dbContext.Entry(blockedProviderParent).State = EntityState.Modified;

        await db.SaveChangesAsync();

        return await Task.FromResult(blockedProviderParent);
    }

    /// <inheritdoc/>
    public IQueryable<BlockedProviderParent> GetBlockedProviderParentEntities(Guid parentId, Guid providerId)
    {
        return Get(
           whereExpression: b =>
           b.ParentId == parentId
           && b.ProviderId == providerId
           && b.DateTimeTo == null);
    }

    private async Task SetApplicationsAndChatRoomsBlock(Guid parentId, Guid providerId, bool block)
    {
        var applications = db.Applications.Where(a => a.ParentId == parentId
                                                      && a.Workshop.ProviderId == providerId);
        await applications.ForEachAsync(a => a.IsBlockedByProvider = block);

        var chatRooms = db.ChatRoomWorkshops.Where(c => c.ParentId == parentId
                                                        && c.Workshop.ProviderId == providerId);
        await chatRooms.ForEachAsync(a => a.IsBlockedByProvider = block);
    }
}