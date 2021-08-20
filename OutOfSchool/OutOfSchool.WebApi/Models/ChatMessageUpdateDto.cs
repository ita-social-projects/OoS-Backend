using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class ChatMessageUpdateDto
    {
        public long Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Text { get; set; }
    }
}
