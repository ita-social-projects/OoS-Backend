using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models
{
    public class ChatRoom
    {
        public long Id { get; set; }

        [Required]
        public long WorkshopId { get; set; }

        public virtual Workshop Workshop { get; set; }

        public virtual IEnumerable<ChatMessage> ChatMessages { get; set; }

        public virtual IEnumerable<User> Users { get; set; }

        public virtual List<ChatRoomUser> ChatRoomUsers { get; set; }
    }
}
