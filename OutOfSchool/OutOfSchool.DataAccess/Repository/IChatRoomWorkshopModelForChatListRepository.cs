using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Models.ChatWorkshop;

namespace OutOfSchool.Services.Repository
{
    public interface IChatRoomWorkshopModelForChatListRepository
    {
        Task<ChatRoomWorkshopForChatList> GetByChatRoomIdAsync(long chatRoomId);

        Task<ICollection<ChatRoomWorkshopForChatList>> GetByParentIdAsync(long parentId);

        Task<ICollection<ChatRoomWorkshopForChatList>> GetByProviderIdAsync(long providerId);

        Task<ICollection<ChatRoomWorkshopForChatList>> GetByWorkshopIdAsync(long workshopId);
    }
}
