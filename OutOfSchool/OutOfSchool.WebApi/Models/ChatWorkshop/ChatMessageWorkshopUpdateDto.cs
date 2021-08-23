using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.ChatWorkshop
{
    public class ChatMessageWorkshopUpdateDto
    {
        [Required]
        public long Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Text { get; set; }
    }
}
