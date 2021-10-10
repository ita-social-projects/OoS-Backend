using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class ChatRoomDto
    {
        public Guid Id { get; set; }

        [Required]
        public long WorkshopId { get; set; }

        public int NotReadMessagesCount { get; set; }

        public IEnumerable<ChatMessageDto> ChatMessages { get; set; }

        public IEnumerable<UserDto> Users { get; set; }
    }
}
