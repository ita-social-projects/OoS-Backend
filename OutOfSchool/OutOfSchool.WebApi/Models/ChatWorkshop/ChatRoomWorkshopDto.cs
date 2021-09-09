namespace OutOfSchool.WebApi.Models.ChatWorkshop
{
    public class ChatRoomWorkshopDto
    {
        public long Id { get; set; }

        public long WorkshopId { get; set; }

        public long ParentId { get; set; }

        public WorkshopInfoForChatListDto Workshop { get; set; }

        public ParentDtoWithContactInfo Parent { get; set; }
    }
}
