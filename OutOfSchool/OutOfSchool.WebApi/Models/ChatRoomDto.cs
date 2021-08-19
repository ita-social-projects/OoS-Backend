using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class ChatRoomDto
    {
        public long Id { get; set; }

        [Required]
        public long WorkshopId { get; set; }

        [Required]
        public long ParentId { get; set; }

        public WorkshopCard Workshop { get; set; }

        public ParentDtoWithShortUserInfo Parent { get; set; }
    }
}
