using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OutOfSchool.BusinessLogic.Models.ChatWorkshop;

public class ChatMessageWorkshopCreateDto
{
    [Required]
    [JsonPropertyName("WorkshopId")]
    public Guid WorkshopId { get; set; }

    [Required]
    [JsonPropertyName("ParentId")]
    public Guid ParentId { get; set; }

    [Required]
    [JsonPropertyName("ChatRoomId")]
    public Guid ChatRoomId { get; set; }

    [Required]
    [MaxLength(Constants.ChatMessageTextMaxLength)]
    [JsonPropertyName("Text")]
    public string Text { get; set; }
}