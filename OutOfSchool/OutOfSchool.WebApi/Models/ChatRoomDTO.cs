using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class ChatRoomDto
    {
        public long Id { get; set; }

        [Required]
        public long WorkshopId { get; set; }

        public int NotReadMessagesCount { get; set; }

        public ICollection<ChatMessageDto> ChatMessages { get; set; }

        public ICollection<UserDto> Users { get; set; }
    }
}
