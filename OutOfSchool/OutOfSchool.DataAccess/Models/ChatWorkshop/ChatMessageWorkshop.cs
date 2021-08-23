using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models.ChatWorkshop
{
    public class ChatMessageWorkshop
    {
        public long Id { get; set; }

        [Required]
        public long ChatRoomId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Text { get; set; }

        [Required]
        public bool SenderRoleIsProvider { get; set; }

        [Required]
        public DateTimeOffset CreatedTime { get; set; }

        [Required]
        public bool IsRead { get; set; }

        public virtual ChatRoomWorkshop ChatRoom { get; set; }
    }
}
