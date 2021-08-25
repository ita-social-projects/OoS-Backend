using System;

namespace OutOfSchool.Services.Models.ChatWorkshop
{
    public class ChatMessageInfoForChatList
    {
        public long Id { get; set; }

        public long ChatRoomId { get; set; }

        public string Text { get; set; }

        public bool SenderRoleIsProvider { get; set; }

        public DateTimeOffset CreatedTime { get; set; }

        public bool IsRead { get; set; }
    }
}
