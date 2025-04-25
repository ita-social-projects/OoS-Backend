namespace OutOfSchool.BusinessLogic.Models.ChatWorkshop;

public class ChatRoomWorkshopDto
{
    public Guid Id { get; set; }

    public Guid WorkshopId { get; set; }

    public Guid ParentId { get; set; }

    public bool IsBlockedByProvider { get; set; }

    public WorkshopInfoForChatListDto Workshop { get; set; }

    public ParentDtoWithContactInfo Parent { get; set; }
}

public static class ChatRoomWorkshopDtoExtensions
{
    public static ChatRoomWorkshopDto ToDto(this ChatRoomWorkshop model)
        => new()
        {
            Id = model.Id,
            WorkshopId = model.WorkshopId,
            ParentId = model.ParentId,
            IsBlockedByProvider = model.IsBlockedByProvider,
            Workshop = model.Workshop?.ToChatListDto(),
            Parent = model.Parent?.ToContactInfoDto(),
        };

    public static List<ChatRoomWorkshopDto> ToDto(this IEnumerable<ChatRoomWorkshop> list)
        => list.MapToList(ToDto);
}