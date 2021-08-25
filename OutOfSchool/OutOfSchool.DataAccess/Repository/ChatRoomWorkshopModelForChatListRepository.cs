using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models.ChatWorkshop;

namespace OutOfSchool.Services.Repository
{
    public class ChatRoomWorkshopModelForChatListRepository : IChatRoomWorkshopModelForChatListRepository
    {
        private readonly OutOfSchoolDbContext dbContext;
        private readonly DbSet<ChatRoomWorkshop> dbSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatRoomWorkshopModelForChatListRepository"/> class.
        /// </summary>
        /// <param name="dbContext">OutOfSchoolDbContext.</param>
        public ChatRoomWorkshopModelForChatListRepository(OutOfSchoolDbContext dbContext)
        {
            this.dbContext = dbContext;
            dbSet = this.dbContext.Set<ChatRoomWorkshop>();
        }

        public async Task<ChatRoomWorkshopForChatList> GetByChatRoomIdAsync(long chatRoomId)
        {
            var query = dbSet
                .Where(x => x.Id == chatRoomId)
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
                    NotReadByCurrentUserMessagesCount = item.ChatMessages.Where(mess => mess.ReadDateTime == null).Count(),
                });
            var res = await query.SingleOrDefaultAsync();
            return res;
        }

        public async Task<ICollection<ChatRoomWorkshopForChatList>> GetByParentIdAsync(long parentId)
        {
            var query = dbSet
                .Where(x => x.ParentId == parentId)
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
                    NotReadByCurrentUserMessagesCount = item.ChatMessages.Where(mess => mess.ReadDateTime == null && mess.SenderRoleIsProvider).Count(),
                });
            var res = await query.ToListAsync();
            return res;
        }

        public async Task<ICollection<ChatRoomWorkshopForChatList>> GetByProviderIdAsync(long providerId)
        {
            var query = dbSet
                .Where(x => x.Workshop.ProviderId == providerId)
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
                    NotReadByCurrentUserMessagesCount = item.ChatMessages.Where(mess => mess.ReadDateTime == null && !mess.SenderRoleIsProvider).Count(),
                });
            var res = await query.ToListAsync();
            return res;
        }

        public async Task<ICollection<ChatRoomWorkshopForChatList>> GetByWorkshopIdAsync(long workshopId)
        {
            var query = dbSet
                .Where(x => x.WorkshopId == workshopId)
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
                    NotReadByCurrentUserMessagesCount = item.ChatMessages.Where(mess => mess.ReadDateTime == null && !mess.SenderRoleIsProvider).Count(),
                });
            var res = await query.ToListAsync();
            return res;
        }
    }
}
