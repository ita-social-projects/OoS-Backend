using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class ChatMessageDto
    {
        public long Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public long ChatRoomId { get; set; }

        [Required]
        public string Text { get; set; }

        public DateTime CreatedTime { get; set; }

        public bool IsRead { get; set; }
    }
}
