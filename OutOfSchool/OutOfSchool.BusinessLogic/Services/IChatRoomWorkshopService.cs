using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.ChatWorkshop;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Defines interface for CRUD operations for ChatRoom and ChatRoomUser.
/// </summary>
public interface IChatRoomWorkshopService
{
    /// <summary>
    /// Create new ChatRoom or returns existing ChatRoom in the system.
    /// </summary>
    /// <param name="workshopId">Id of Workshop.</param>
    /// <param name="parentId">Id of Parent.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatRoomWorkshopDto"/> that was created or found.</returns>
    /// <exception cref="InvalidOperationException">If the logic of creating chat was compromised.</exception>
    /// <exception cref="DbUpdateException">If trying to create entity something was wrong. For example invalid foreign keys.</exception>
    Task<ChatRoomWorkshopDto> CreateOrReturnExistingAsync(Guid workshopId, Guid parentId);

    /// <summary>
    /// Get ChatRoom by it's key, including Users and Messages.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatRoomWorkshopDto"/> that was found, or null.</returns>
    Task<ChatRoomWorkshopDto> GetByIdAsync(Guid id);

    /// <summary>
    /// Get ChatRooms by specified Parent and Provider.
    /// </summary>
    /// <param name="parentId">Parent's identifier.</param>
    /// <param name="providerId">Provider's identifier.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="IEnumerable{ChatRoomWorkshopDto}"/> that contains elements from the input sequence.</returns>
    Task<IEnumerable<ChatRoomWorkshopDto>> GetByParentIdProviderIdAsync(Guid parentId, Guid providerId);

    /// <summary>
    /// Get ChatRooms with messages by specified Parent and Provider.
    /// </summary>
    /// <param name="parentId">Parent's identifier.</param>
    /// <param name="providerId">Provider's identifier.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="IEnumerable{ChatRoomWorkshopDtoWithLastMessage}"/> that contains elements from the input sequence.</returns>
    Task<IEnumerable<ChatRoomWorkshopDtoWithLastMessage>> GetWithMessagesByParentIdProviderIdAsync(Guid parentId, Guid providerId);

    /// <summary>
    /// Get ChatRoom by specified Parent and Workshop.
    /// </summary>
    /// <param name="parentId">Parent's identifier.</param>
    /// <param name="workshopId">Workshop's identifier.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatRoomWorkshopDto"/> that contains elements from the input sequence.</returns>
    Task<ChatRoomWorkshopDto> GetByParentIdWorkshopIdAsync(Guid parentId, Guid workshopId);

    /// <summary>
    /// Get ChatRoom with messages by specified Parent and Workshop.
    /// </summary>
    /// <param name="parentId">Parent's identifier.</param>
    /// <param name="workshopId">Workshop's identifier.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatRoomWorkshopDtoWithLastMessage"/> that contains elements from the input sequence.</returns>
    Task<ChatRoomWorkshopDtoWithLastMessage> GetWithMessagesByParentIdWorkshopIdAsync(Guid parentId, Guid workshopId);

    /// <summary>
    /// Get ChatRooms with last message and count of not read messages by specified Parent.
    /// </summary>
    /// <param name="parentId">Parent's identifier.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="IEnumerable{ChatRoomWithLastMessage}"/> that contains elements from the input sequence.</returns>
    Task<IEnumerable<ChatRoomWorkshopDtoWithLastMessage>> GetByParentIdAsync(Guid parentId);

    /// <summary>
    /// Get ChatRooms with last message and count of not read messages by specified Provider.
    /// </summary>
    /// <param name="providerId">Provider's identifier.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="IEnumerable{ChatRoomWithLastMessage}"/> that contains elements from the input sequence.</returns>
    Task<IEnumerable<ChatRoomWorkshopDtoWithLastMessage>> GetByProviderIdAsync(Guid providerId);

    /// <summary>
    /// Get ChatRooms with last message and count of not read messages by specified Workshop.
    /// </summary>
    /// <param name="workshopId">Workshop's identifier.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="IEnumerable{ChatRoomWithLastMessage}"/> that contains elements from the input sequence.</returns>
    Task<IEnumerable<ChatRoomWorkshopDtoWithLastMessage>> GetByWorkshopIdAsync(Guid workshopId);

    /// <summary>
    /// Get ChatRooms with last message and count of not read messages by specified Workshop's WorkshopIds.
    /// </summary>
    /// <param name="workshopIds">Workshop's identifiers.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="IEnumerable{ChatRoomWithLastMessage}"/> that contains elements from the input sequence.</returns>
    Task<IEnumerable<ChatRoomWorkshopDtoWithLastMessage>> GetByWorkshopIdsAsync(IEnumerable<Guid> workshopIds);

    /// <summary>
    /// Get a list of ChatRoom's WorkshopIds by specified Parent.
    /// </summary>
    /// <param name="parentId">Parent's identifier.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="IEnumerable{Int64}"/> that contains elements from the input sequence.</returns>
    Task<IEnumerable<Guid>> GetChatRoomIdsByParentIdAsync(Guid parentId);

    /// <summary>
    /// Get a list of ChatRoom's WorkshopIds by specified Provider.
    /// </summary>
    /// <param name="providerId">Provider's identifier.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="IEnumerable{Int64}"/> that contains elements from the input sequence.</returns>
    Task<IEnumerable<Guid>> GetChatRoomIdsByProviderIdAsync(Guid providerId);

    /// <summary>
    /// Get a list of ChatRoom's WorkshopIds by specified Worshops.
    /// </summary>
    /// <param name="workshopIds">List of workshops ids.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="IEnumerable{Int64}"/> that contains elements from the input sequence.</returns>
    Task<IEnumerable<Guid>> GetChatRoomIdsByWorkshopIdsAsync(IEnumerable<Guid> workshopIds);

    /// <summary>
    /// Delete the ChatRoom including its messages.
    /// </summary>
    /// <param name="id">ChatRoom's key.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">If there was no one entity or more then one entity found.</exception>
    /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Get the ChatRoom by userIds and workshop. Not include ChatMessages.
    /// </summary>
    /// <param name="workshopId">Id of Workshop.</param>
    /// <param name="parentId">Id of Parent.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatRoomWorkshopDto"/> that was found, or null.</returns>
    /// <exception cref="InvalidOperationException">If the logic of creating chat was compromised.</exception>
    Task<ChatRoomWorkshopDto> GetUniqueChatRoomAsync(Guid workshopId, Guid parentId);

    /// <summary>
    /// Get all entities that matches filter's parameters.
    /// </summary>
    /// <param name="filter">Entity that represents searching parameters.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="IEnumerable{ChatRoomWorkshopDtoWithLastMessage}"/> that contains elements that were found.</returns>
    Task<SearchResult<ChatRoomWorkshopDtoWithLastMessage>> GetChatRoomByFilter(ChatWorkshopFilter filter, Guid userId, bool searchForProvider = true);

    /// <summary>
    /// Get count of unread messages for current user.
    /// </summary>
    /// <param name="parentOrProviderId">Id of parent or provider.</param>
    /// <param name="userRole">User role.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<int> GetCurrentUserUnreadMessagesCountAsync(Guid parentOrProviderId, Role userRole);
}
