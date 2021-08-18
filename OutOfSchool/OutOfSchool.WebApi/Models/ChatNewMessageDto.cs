using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace OutOfSchool.WebApi.Models
{
    public class ChatNewMessageDto
    {
        [Required]
        [JsonProperty("WorkshopId")]
        public long WorkshopId { get; set; }

        [Required]
        [JsonProperty("ParentId")]
        public long ParentId { get; set; }

        [Required]
        [MaxLength(200)]
        [JsonProperty("Text")]
        public string Text { get; set; }

        [Required]
        [JsonProperty("SenderRoleIsProvider")]
        public bool SenderRoleIsProvider { get; set; }

        [JsonProperty("ChatRoomId")]
        public long ChatRoomId { get; set; }
    }
}
