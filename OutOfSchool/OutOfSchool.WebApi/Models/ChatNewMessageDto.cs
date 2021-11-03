using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace OutOfSchool.WebApi.Models
{
    public class ChatNewMessageDto
    {
        [Required]
        [JsonProperty("WorkshopId")]
        public Guid WorkshopId { get; set; }

        [Required]
        [MaxLength(200)]
        [JsonProperty("Text")]
        public string Text { get; set; }

        [Required]
        [MaxLength(450)]
        [JsonProperty("ReceiverUserId")]
        public string ReceiverUserId { get; set; }

        [JsonProperty("ChatRoomId")]
        public Guid ChatRoomId { get; set; }
    }
}
