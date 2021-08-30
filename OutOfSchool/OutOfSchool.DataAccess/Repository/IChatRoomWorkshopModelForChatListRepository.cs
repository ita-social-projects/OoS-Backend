using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Models.ChatWorkshop;

namespace OutOfSchool.Services.Repository
{
    public interface IChatRoomWorkshopModelForChatListRepository
    {
        /// <summary>
        /// Get a Chat room for workshop by specified Chat room key.
        /// </summary>
        /// <param name="chatRoomId">Chat rooms key in the system.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="ChatRoomWorkshopForChatList"/> that was faund, or null.</returns>
        Task<ChatRoomWorkshopForChatList> GetByChatRoomIdAsync(long chatRoomId);

        /// <summary>
        /// Get a List of chat rooms for workshops by specified Parent key.
        /// </summary>
        /// <param name="parentId">Parent key in the system.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="List{ChatRoomWorkshopForChatList}"/> that contains faund elements.</returns>
        Task<List<ChatRoomWorkshopForChatList>> GetByParentIdAsync(long parentId);

        /// <summary>
        /// Get a List of chat rooms for workshops by specified Provider key.
        /// </summary>
        /// <param name="providerId">Provider key in the system.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="List{ChatRoomWorkshopForChatList}"/> that contains faund elements.</returns>
        Task<List<ChatRoomWorkshopForChatList>> GetByProviderIdAsync(long providerId);

        /// <summary>
        /// Get a List of chat rooms for workshops by specified Workshop key.
        /// </summary>
        /// <param name="workshopId">Workshop key in the system.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="List{ChatRoomWorkshopForChatList}"/> that contains faund elements.</returns>
        Task<List<ChatRoomWorkshopForChatList>> GetByWorkshopIdAsync(long workshopId);
    }
}
