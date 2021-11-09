using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.ChatWorkshop
{
    public class ChatMessageWorkshopUpdateDto
    {
        [Required]
        [Range(1, long.MaxValue)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Text { get; set; }
    }
}
