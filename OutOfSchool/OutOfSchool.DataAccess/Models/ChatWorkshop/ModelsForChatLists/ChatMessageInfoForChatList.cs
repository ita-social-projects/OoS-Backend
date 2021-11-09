using System;

namespace OutOfSchool.Services.Models.ChatWorkshop
{
    public class ChatMessageInfoForChatList
    {
        public Guid Id { get; set; }

        public Guid ChatRoomId { get; set; }

        public string Text { get; set; }

        public bool SenderRoleIsProvider { get; set; }

        public DateTimeOffset CreatedDateTime { get; set; }

        public DateTimeOffset? ReadDateTime { get; set; }
    }
}
