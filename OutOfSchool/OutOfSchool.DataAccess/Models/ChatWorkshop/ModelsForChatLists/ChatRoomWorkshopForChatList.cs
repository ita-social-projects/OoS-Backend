namespace OutOfSchool.Services.Models.ChatWorkshop
{
    public class ChatRoomWorkshopForChatList
    {
        public long Id { get; set; }

        public long WorkshopId { get; set; }

        public long ParentId { get; set; }

        public ParentInfoForChatList Parent { get; set; }

        public WorkshopInfoForChatList Workshop { get; set; }

        public int NotReadByCurrentUserMessagesCount { get; set; }

        public ChatMessageInfoForChatList LastMessage { get; set; }
    }
}
