using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models
{
    public class ChatMessage
    {
        public long Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public long ChatRoomId { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        public DateTime CreatedTime { get; set; }

        [Required]
        public bool IsRead { get; set; }

        public virtual User User { get; set; }

        public virtual ChatRoom ChatRoom { get; set; }
    }
}
