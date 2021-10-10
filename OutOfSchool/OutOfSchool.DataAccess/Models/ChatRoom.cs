using System;
using System.Collections.Generic;

namespace OutOfSchool.Services.Models
{
    public class ChatRoom
    {
        public Guid Id { get; set; }

        public Guid WorkshopId { get; set; }

        public virtual Workshop Workshop { get; set; }

        public virtual IEnumerable<ChatMessage> ChatMessages { get; set; }

        public virtual IEnumerable<User> Users { get; set; }

        public virtual List<ChatRoomUser> ChatRoomUsers { get; set; }
    }
}
