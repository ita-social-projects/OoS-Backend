using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models.ChatWorkshop
{
    public class ChatMessageWorkshop
    {
        public long Id { get; set; }

        // TODO: consider to remove this property as we already have a navigation property 'ChatRoom'.
        [Required]
        public long ChatRoomId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Text { get; set; }

        [Required]
        public bool SenderRoleIsProvider { get; set; }

        [Required]
        public DateTimeOffset CreatedDateTime { get; set; }

        public DateTimeOffset? ReadDateTime { get; set; }

        public virtual ChatRoomWorkshop ChatRoom { get; set; }
    }
}
