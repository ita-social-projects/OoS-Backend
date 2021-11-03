using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class ChatMessageDto
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; }

        public Guid ChatRoomId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Text { get; set; }

        public DateTimeOffset CreatedTime { get; set; }

        public bool IsRead { get; set; }
    }
}
