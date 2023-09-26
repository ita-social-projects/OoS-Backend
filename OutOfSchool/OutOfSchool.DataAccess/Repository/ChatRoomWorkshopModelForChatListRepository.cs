using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using OutOfSchool.Services.Models.ChatWorkshop;

namespace OutOfSchool.Services.Repository;

public class ChatRoomWorkshopModelForChatListRepository : IChatRoomWorkshopModelForChatListRepository
{
    private readonly DbSet<ChatRoomWorkshop> dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatRoomWorkshopModelForChatListRepository"/> class.
    /// </summary>
    /// <param name="dbContext">OutOfSchoolDbContext.</param>
    public ChatRoomWorkshopModelForChatListRepository(OutOfSchoolDbContext dbContext)
    {
        dbSet = dbContext.Set<ChatRoomWorkshop>() ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ChatRoomWorkshopForChatList> GetByChatRoomIdAsync(Guid chatRoomId, bool searchMessagesForProvider = true)
    {
        Expression<Func<ChatRoomWorkshop, bool>> condition = x => x.Id == chatRoomId;

        var chatRooms = await this.GetByParametersAsync(condition, searchMessagesForProvider).ConfigureAwait(false);

        return chatRooms.SingleOrDefault();
    }

    public Task<List<ChatRoomWorkshopForChatList>> GetByParentIdAsync(Guid parentId, bool searchMessagesForProvider = false)
    {
        Expression<Func<ChatRoomWorkshop, bool>> condition = x => x.ParentId == parentId;

        return this.GetByParametersAsync(condition, searchMessagesForProvider);
    }

    public Task<List<ChatRoomWorkshopForChatList>> GetByParentIdWorkshopIdAsync(Guid parentId, Guid workshopId, bool searchMessagesForProvider = false)
    {
        Expression<Func<ChatRoomWorkshop, bool>> condition = x => x.ParentId == parentId && x.WorkshopId == workshopId;

        return this.GetByParametersAsync(condition, searchMessagesForProvider);
    }

    public Task<List<ChatRoomWorkshopForChatList>> GetByProviderIdAsync(Guid providerId, bool searchMessagesForProvider = true)
    {
        Expression<Func<ChatRoomWorkshop, bool>> condition = x => x.Workshop.ProviderId == providerId;

        return this.GetByParametersAsync(condition, searchMessagesForProvider);
    }

    public Task<List<ChatRoomWorkshopForChatList>> GetByParentIdProviderIdAsync(Guid parentId, Guid providerId, bool searchMessagesForProvider = true)
    {
        Expression<Func<ChatRoomWorkshop, bool>> condition = x => x.ParentId == parentId && x.Workshop.ProviderId == providerId;

        return this.GetByParametersAsync(condition, searchMessagesForProvider);
    }

    public Task<List<ChatRoomWorkshopForChatList>> GetByWorkshopIdAsync(Guid workshopId, bool searchMessagesForProvider = true)
    {
        Expression<Func<ChatRoomWorkshop, bool>> condition = x => x.WorkshopId == workshopId;

        return this.GetByParametersAsync(condition, searchMessagesForProvider);
    }

    /// <inheritdoc/>
    public Task<List<ChatRoomWorkshopForChatList>> GetByWorkshopIdsAsync(IEnumerable<Guid> workshopIds, bool searchMessagesForProvider = true)
    {
        Expression<Func<ChatRoomWorkshop, bool>> condition = x => workshopIds.Contains(x.WorkshopId);

        return this.GetByParametersAsync(condition, searchMessagesForProvider);
    }

    private Task<List<ChatRoomWorkshopForChatList>> GetByParametersAsync(Expression<Func<ChatRoomWorkshop, bool>> condition, bool searchMessagesForProvider)
    {
        var query = dbSet
            .Where(condition)
            .Where(x => !x.IsDeleted)
            .Select(item => new ChatRoomWorkshopForChatList()
            {
                Id = item.Id,
                WorkshopId = item.WorkshopId,
                Workshop = new WorkshopInfoForChatList()
                {
                    Id = item.Workshop.Id,
                    Title = item.Workshop.Title,
                    ProviderId = item.Workshop.ProviderId,
                    ProviderTitle = item.Workshop.ProviderTitle,
                },
                ParentId = item.ParentId,
                Parent = new ParentInfoForChatList()
                {
                    Id = item.Parent.Id,
                    UserId = item.Parent.UserId,
                    FirstName = item.Parent.User.FirstName,
                    MiddleName = item.Parent.User.MiddleName,
                    LastName = item.Parent.User.LastName,
                    Gender = item.Parent.Gender,
                    Email = item.Parent.User.Email,
                    PhoneNumber = item.Parent.User.PhoneNumber,
                },
                LastMessage = item.ChatMessages.Where(mess => !mess.IsDeleted && mess.CreatedDateTime == item.ChatMessages.Where(m => !m.IsDeleted).Max(m => m.CreatedDateTime))
                    .Select(message => new ChatMessageInfoForChatList()
                    {
                        Id = message.Id,
                        ChatRoomId = message.ChatRoomId,
                        Text = message.Text,
                        CreatedDateTime = message.CreatedDateTime,
                        SenderRoleIsProvider = message.SenderRoleIsProvider,
                        ReadDateTime = message.ReadDateTime,
                    })
                    .FirstOrDefault(),
                NotReadByCurrentUserMessagesCount = item.ChatMessages.Count(mess => !mess.IsDeleted && mess.ReadDateTime == null && (mess.SenderRoleIsProvider != searchMessagesForProvider)),
            })
            .OrderByDescending(x => x.LastMessage.CreatedDateTime);
        return query.ToListAsync();
    }
}