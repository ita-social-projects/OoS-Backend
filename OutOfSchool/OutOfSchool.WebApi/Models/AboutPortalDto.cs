using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class AboutPortalDto
    {
        public Guid Id { get; set; }

        [MaxLength(200)]
        public string Title { get; set; }
    }
}
