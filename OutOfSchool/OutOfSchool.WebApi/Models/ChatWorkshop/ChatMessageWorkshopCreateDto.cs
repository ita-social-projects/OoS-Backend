using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace OutOfSchool.WebApi.Models.ChatWorkshop;

public class ChatMessageWorkshopCreateDto
{
    [Required]
    [JsonProperty("WorkshopId", Required = Required.Always)]
    public Guid WorkshopId { get; set; }

    [Required]
    [JsonProperty("ParentId", Required = Required.Always)]
    public Guid ParentId { get; set; }

    [Required]
    [JsonProperty("ChatRoomId", Required = Required.Always)]
    public Guid ChatRoomId { get; set; }

    [Required]
    [MaxLength(Constants.ChatMessageTextMaxLength)]
    [JsonProperty("Text", Required = Required.Always)]
    public string Text { get; set; }
}