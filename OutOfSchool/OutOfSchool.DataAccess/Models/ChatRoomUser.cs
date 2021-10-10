using System;

namespace OutOfSchool.Services.Models
{
    public class ChatRoomUser
    {
        public Guid Id { get; set; }

        public Guid ChatRoomId { get; set; }

        public virtual ChatRoom ChatRoom { get; set; }

        public string UserId { get; set; }

        public virtual User User { get; set; }
    }
}
