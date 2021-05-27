using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class ChatRoomDTO
    {
        public long Id { get; set; }

        [Required]
        public long WorkshopId { get; set; }

        public int NotReadMessagesCount { get; set; }

        public ICollection<ChatMessageDTO> ChatMessages { get; set; }

        public ICollection<UserDto> Users { get; set; }
    }
}
