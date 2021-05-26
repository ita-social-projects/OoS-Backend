using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class ChatNewMessageDTO
    {
        [Required]
        public string WorkshopId { get; set; }

        [Required]
        public string Text { get; set; }

        public long ChatRoomId { get; set; }
    }
}
