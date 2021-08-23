namespace OutOfSchool.Services.Models.ChatWorkshop
{
    public class ChatRoomWorkshopWithLastMessage
    {
        public long Id { get; set; }

        public long WorkshopId { get; set; }

        public long ParentId { get; set; }

        public Workshop Workshop { get; set; }

        public Parent Parent { get; set; }

        public int NotReadByCurrentUserMessagesCount { get; set; }

        public ChatMessageWorkshop LastMessage { get; set; }
    }
}
