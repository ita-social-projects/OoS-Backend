namespace OutOfSchool.WebApi.Models
{
    public class ChatRoomWithLastMessage : ChatRoomDto
    {
        public int NotReadMessagesCount { get; set; }

        public ChatMessageDto LastMessage { get; set; }
    }
}
