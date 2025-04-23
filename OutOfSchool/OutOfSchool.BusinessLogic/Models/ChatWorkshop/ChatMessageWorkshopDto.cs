using OutOfSchool.Services.Models.ChatWorkshop.ModelsForChatLists;

namespace OutOfSchool.BusinessLogic.Models.ChatWorkshop;

public class ChatMessageWorkshopDto
{
    public Guid Id { get; set; }

    public Guid ChatRoomId { get; set; }

    public string Text { get; set; }

    public bool SenderRoleIsProvider { get; set; }

    public DateTimeOffset CreatedDateTime { get; set; }

    public DateTimeOffset? ReadDateTime { get; set; }
}

public static class ChatMessageWorkshopDtoExtensions
{
    public static ChatMessageWorkshopDto ToDto(this ChatMessageWorkshop message)
        => new()
        {
            Id = message.Id,
            ChatRoomId = message.ChatRoomId,
            Text = message.Text,
            SenderRoleIsProvider = message.SenderRoleIsProvider,
            CreatedDateTime = message.CreatedDateTime,
            ReadDateTime = message.ReadDateTime,
        };

    public static ChatMessageWorkshopDto ToDto(this ChatMessageInfoForChatList message)
        => new()
        {
            Id = message.Id,
            ChatRoomId = message.ChatRoomId,
            Text = message.Text,
            SenderRoleIsProvider = message.SenderRoleIsProvider,
            CreatedDateTime = message.CreatedDateTime,
            ReadDateTime = message.ReadDateTime,
        };

    public static List<ChatMessageWorkshopDto> ToDto(this IEnumerable<ChatMessageWorkshop> list)
        => list.MapToList(ToDto);
}