using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.ChatWorkshop;

public class ChatMessageWorkshopUpdateDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Text { get; set; }
}