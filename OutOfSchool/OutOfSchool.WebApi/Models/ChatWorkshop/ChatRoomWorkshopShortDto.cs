using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.ChatWorkshop
{
    public class ChatRoomWorkshopShortDto
    {
        public long Id { get; set; }

        [Required]
        public long WorkshopId { get; set; }

        [Required]
        public long ParentId { get; set; }
    }
}
