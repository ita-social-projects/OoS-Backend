using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.ChatWorkshop;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Defines interface for CRUD functionality for ChatMessage entity.
/// </summary>
public interface IChatMessageWorkshopService
{
    /// <summary>
    /// Create new ChatMessage.
    /// </summary>
    /// <param name="chatMessageCreateDto">ChatMessage to create.</param>
    /// <param name="userRole">The role of sender (parent/provider).</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="ChatMessageWorkshopDto"/> that was created.</returns>
    /// <exception cref="ArgumentNullException">If the parameter <see cref="ChatMessageWorkshopDto"/> was not set to instance.</exception>
    Task<ChatMessageWorkshopDto> CreateAsync(ChatMessageWorkshopCreateDto chatMessageCreateDto, Role userRole);

    /// <summary>
    /// Get a portion of all ChatMessages with specified ChatRoomId.
    /// </summary>
    /// <param name="chatRoomId">ChatRoom's key.</param>
    /// <param name="offsetFilter">Filter to take specified part of entities.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="IEnumerable{ChatMessageDTO}"/> that contains elements from the input sequence.</returns>
    Task<List<ChatMessageWorkshopDto>> GetMessagesForChatRoomAsync(Guid chatRoomId, OffsetFilter offsetFilter);

    /// <summary>
    /// Get a portion of all ChatMessages with specified ChatRoomId.
    /// Set read current date and time in UTC format in messages that are not read by the current user.
    /// </summary>
    /// <param name="chatRoomId">ChatRoom's key.</param>
    /// <param name="offsetFilter">Filter to take specified part of messages.</param>
    /// <param name="userRole">Role of current user.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="IEnumerable{ChatMessageDTO}"/> that contains elements from the input sequence.</returns>
    Task<List<ChatMessageWorkshopDto>> GetMessagesForChatRoomAndSetReadDateTimeIfItIsNullAsync(Guid chatRoomId, OffsetFilter offsetFilter, Role userRole);
}