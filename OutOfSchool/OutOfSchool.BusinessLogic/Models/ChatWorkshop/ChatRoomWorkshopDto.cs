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