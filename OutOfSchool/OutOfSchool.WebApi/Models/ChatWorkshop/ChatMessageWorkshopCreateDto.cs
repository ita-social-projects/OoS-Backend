using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace OutOfSchool.WebApi.Models.ChatWorkshop
{
    public class ChatMessageWorkshopCreateDto
    {
        [Required]
        [Range(1, long.MaxValue)]
        [JsonProperty("WorkshopId", Required = Required.Always)]
        public Guid WorkshopId { get; set; }

        [Required]
        [Range(1, long.MaxValue)]
        [JsonProperty("ParentId", Required = Required.Always)]
        public Guid ParentId { get; set; }

        // TODO: max length to const in Common
        [Required]
        [MaxLength(200)]
        [JsonProperty("Text", Required = Required.Always)]
        public string Text { get; set; }
    }
}
