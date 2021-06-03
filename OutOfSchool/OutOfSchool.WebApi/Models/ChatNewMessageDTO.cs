using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace OutOfSchool.WebApi.Models
{
    public class ChatNewMessageDTO
    {
        [Required]
        [JsonProperty("WorkshopId")]
        public long WorkshopId { get; set; }

        [Required]
        [JsonProperty("Text")]
        public string Text { get; set; }

        [Required]
        [JsonProperty("ReceiverUserId")]
        public string ReceiverUserId { get; set; }

        [JsonProperty("ChatRoomId")]
        public long ChatRoomId { get; set; }
    }
}
