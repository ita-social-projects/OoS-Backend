using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class ChatMessageDto
    {
        public long Id { get; set; }

        public long ChatRoomId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Text { get; set; }

        [Required]
        public bool SenderRoleIsProvider { get; set; }

        public DateTimeOffset CreatedTime { get; set; }

        public bool IsRead { get; set; }
    }
}
