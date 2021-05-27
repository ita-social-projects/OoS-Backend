using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class ChatNewMessageDTO
    {
        [Required]
        public long WorkshopId { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        public string ReceiverUserId { get; set; }

        public long ChatRoomId { get; set; }
    }
}
