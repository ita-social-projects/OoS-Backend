using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models.ChatWorkshop;

namespace OutOfSchool.Services.Repository
{
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

        public async Task<ChatRoomWorkshopForChatList> GetByChatRoomIdAsync(long chatRoomId)
        {
            Expression<Func<ChatRoomWorkshop, bool>> condition = x => x.Id == chatRoomId;

            var searchMessagesForProvider = true;

            var chatRooms = await this.GetByParametersAsync(condition, searchMessagesForProvider).ConfigureAwait(false);

            return chatRooms.SingleOrDefault();
        }

        public Task<List<ChatRoomWorkshopForChatList>> GetByParentIdAsync(long parentId)
        {
            Expression<Func<ChatRoomWorkshop, bool>> condition = x => x.ParentId == parentId;

            var searchMessagesForProvider = false;

            return this.GetByParametersAsync(condition, searchMessagesForProvider);
        }

        public Task<List<ChatRoomWorkshopForChatList>> GetByProviderIdAsync(long providerId)
        {
            Expression<Func<ChatRoomWorkshop, bool>> condition = x => x.Workshop.ProviderId == providerId;

            var searchMessagesForProvider = true;

            return this.GetByParametersAsync(condition, searchMessagesForProvider);
        }

        public Task<List<ChatRoomWorkshopForChatList>> GetByWorkshopIdAsync(long workshopId)
        {
            Expression<Func<ChatRoomWorkshop, bool>> condition = x => x.WorkshopId == workshopId;

            var searchMessagesForProvider = true;

            return this.GetByParametersAsync(condition, searchMessagesForProvider);
        }

        private Task<List<ChatRoomWorkshopForChatList>> GetByParametersAsync(Expression<Func<ChatRoomWorkshop, bool>> condition, bool searchMessagesForProvider)
        {
            var query = dbSet
                .Where(condition)
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
                        Email = item.Parent.User.Email,
                        PhoneNumber = item.Parent.User.PhoneNumber,
                    },
                    LastMessage = item.ChatMessages.Where(mess => mess.CreatedDateTime == item.ChatMessages.Max(m => m.CreatedDateTime))
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
                    NotReadByCurrentUserMessagesCount = item.ChatMessages.Count(mess => mess.ReadDateTime == null && (mess.SenderRoleIsProvider != searchMessagesForProvider)),
                });
            return query.ToListAsync();
        }
    }
}
