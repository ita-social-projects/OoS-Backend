using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models
{
    public class Notification
    {
        public Guid Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Text { get; set; }
    }
}
