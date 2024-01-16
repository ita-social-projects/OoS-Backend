namespace OutOfSchool.WebApi.Models.ChatWorkshop;

public class ChatRoomWorkshopDto
{
    public Guid Id { get; set; }

    public Guid WorkshopId { get; set; }

    public Guid ParentId { get; set; }

    public bool IsBlocked { get; set; }

    public WorkshopInfoForChatListDto Workshop { get; set; }

    public ParentDtoWithContactInfo Parent { get; set; }
}