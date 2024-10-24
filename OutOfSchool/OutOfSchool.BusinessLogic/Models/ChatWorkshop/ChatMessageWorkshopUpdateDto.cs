using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.ChatWorkshop;

public class ChatMessageWorkshopUpdateDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(Constants.ChatMessageTextMaxLength)]
    public string Text { get; set; }
}