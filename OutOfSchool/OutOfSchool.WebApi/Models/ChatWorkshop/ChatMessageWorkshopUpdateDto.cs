using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.ChatWorkshop
{
    public class ChatMessageWorkshopUpdateDto
    {
        [Required]
        [Range(1, long.MaxValue)]
        public long Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Text { get; set; }
    }
}
