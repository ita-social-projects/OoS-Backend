using System;

namespace OutOfSchool.Services.Models
{
    public class ChatMessage
    {
        public Guid Id { get; set; }

        public string UserId { get; set; }

        public Guid ChatRoomId { get; set; }

        public string Text { get; set; }

        public DateTimeOffset CreatedTime { get; set; }

        public bool IsRead { get; set; }

        public virtual User User { get; set; }

        public virtual ChatRoom ChatRoom { get; set; }
    }
}
