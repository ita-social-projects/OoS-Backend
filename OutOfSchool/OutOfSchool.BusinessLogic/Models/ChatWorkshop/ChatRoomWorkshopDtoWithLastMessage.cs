namespace OutOfSchool.BusinessLogic.Models.ChatWorkshop;

public class ChatRoomWorkshopDtoWithLastMessage : ChatRoomWorkshopDto
{
    public int NotReadByCurrentUserMessagesCount { get; set; }

    public ChatMessageWorkshopDto LastMessage { get; set; }
}

public static class ChatRoomWorkshopDtoWithLastMessageExtensions
{
    public static ChatRoomWorkshopDtoWithLastMessage ToDto(this OutOfSchool.Services.Models.ChatWorkshop.ModelsForChatLists.ChatRoomWorkshopForChatList model)
        => new()
        {
            Id = model.Id,
            WorkshopId = model.WorkshopId,
            ParentId = model.ParentId,
            IsBlockedByProvider = model.IsBlockedByProvider,
            Workshop = model.Workshop.ToChatListDto(),
            Parent = model.Parent.ToContactInfoDto(),
            NotReadByCurrentUserMessagesCount = model.NotReadByCurrentUserMessagesCount,
            LastMessage = model.LastMessage.ToDto(),
        };

    public static List<ChatRoomWorkshopDtoWithLastMessage> ToDto(this IEnumerable<OutOfSchool.Services.Models.ChatWorkshop.ModelsForChatLists.ChatRoomWorkshopForChatList> list)
        => list.MapToList(ToDto);
}