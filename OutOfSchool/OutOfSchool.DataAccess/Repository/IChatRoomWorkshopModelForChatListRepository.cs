using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using OutOfSchool.Services.Models.ChatWorkshop;

namespace OutOfSchool.Services.Repository;

public interface IChatRoomWorkshopModelForChatListRepository
{
    /// <summary>
    /// Get a Chat room for workshop by specified Chat room key.
    /// </summary>
    /// <param name="chatRoomId">Chat rooms key in the system.</param>
    /// <param name="searchMessagesForProvider">Destination side.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="ChatRoomWorkshopForChatList"/> that was found, or null.</returns>
    Task<ChatRoomWorkshopForChatList> GetByChatRoomIdAsync(Guid chatRoomId, bool searchMessagesForProvider = true);

    /// <summary>
    /// Get a List of chat rooms for workshops by specified Parent key.
    /// </summary>
    /// <param name="parentId">Parent key in the system.</param>
    /// <param name="searchMessagesForProvider">Destination side.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="List{ChatRoomWorkshopForChatList}"/> that contains found elements.</returns>
    Task<List<ChatRoomWorkshopForChatList>> GetByParentIdAsync(Guid parentId, bool searchMessagesForProvider = false);

    /// <summary>
    /// Get a List of chat rooms for workshops by specified Parent key.
    /// </summary>
    /// <param name="parentId">Parent key in the system.</param>
    /// <param name="workshopId">Workshop key in the system.</param>
    /// <param name="searchMessagesForProvider">Destination side.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="List{ChatRoomWorkshopForChatList}"/> that contains found elements.</returns>
    Task<List<ChatRoomWorkshopForChatList>> GetByParentIdWorkshopIdAsync(Guid parentId, Guid workshopId, bool searchMessagesForProvider = false);

    /// <summary>
    /// Get a List of chat rooms for workshops by specified Provider key.
    /// </summary>
    /// <param name="providerId">Provider key in the system.</param>
    /// <param name="searchMessagesForProvider">Destination side.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="List{ChatRoomWorkshopForChatList}"/> that contains found elements.</returns>
    Task<List<ChatRoomWorkshopForChatList>> GetByProviderIdAsync(Guid providerId, bool searchMessagesForProvider = true);

    /// <summary>
    /// Get a List of chat rooms for workshops by specified Provider key.
    /// </summary>
    /// <param name="parentId">Parent key in the system.</param>
    /// <param name="providerId">Provider key in the system.</param>
    /// <param name="searchMessagesForProvider">Destination side.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="List{ChatRoomWorkshopForChatList}"/> that contains found elements.</returns>
    Task<List<ChatRoomWorkshopForChatList>> GetByParentIdProviderIdAsync(Guid parentId, Guid providerId, bool searchMessagesForProvider = true);

    /// <summary>
    /// Get a List of chat rooms for workshops by specified Workshop key.
    /// </summary>
    /// <param name="workshopId">Workshop key in the system.</param>
    /// <param name="searchMessagesForProvider">Destination side.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="List{ChatRoomWorkshopForChatList}"/> that contains found elements.</returns>
    Task<List<ChatRoomWorkshopForChatList>> GetByWorkshopIdAsync(Guid workshopId, bool searchMessagesForProvider = true);

    /// <summary>
    /// Get a List of chat rooms for workshops by specified Workshop's keys.
    /// </summary>
    /// <param name="workshopIds">Workshop's keys in the system.</param>
    /// <param name="searchMessagesForProvider">Destination side.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="List{ChatRoomWorkshopForChatList}"/> that contains found elements.</returns>
    Task<List<ChatRoomWorkshopForChatList>> GetByWorkshopIdsAsync(IEnumerable<Guid> workshopIds, bool searchMessagesForProvider = true);
}