using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models
{
    public class ChatRoomUser
    {
        public long Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; }

        [Required]
        public long ChatRoomId { get; set; }

        public virtual User User { get; set; }

        public virtual ChatRoom ChatRoom { get; set; }
    }
}
