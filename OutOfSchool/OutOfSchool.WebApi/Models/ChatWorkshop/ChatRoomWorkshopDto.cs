namespace OutOfSchool.WebApi.Models.ChatWorkshop
{
    public class ChatRoomWorkshopDto : ChatRoomWorkshopShortDto
    {
        public WorkshopCard Workshop { get; set; }

        public ParentDtoWithShortUserInfo Parent { get; set; }
    }
}
