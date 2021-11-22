using System;

namespace OutOfSchool.Services.Models.ChatWorkshop
{
    public class ChatRoomWorkshopForChatList
    {
        public Guid Id { get; set; }

        public Guid WorkshopId { get; set; }

        public Guid ParentId { get; set; }

        public ParentInfoForChatList Parent { get; set; }

        public WorkshopInfoForChatList Workshop { get; set; }

        public int NotReadByCurrentUserMessagesCount { get; set; }

        public ChatMessageInfoForChatList LastMessage { get; set; }
    }
}
