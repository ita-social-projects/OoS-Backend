using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models
{
    public class SupportInformation
    {
        public Guid Id { get; set; }

        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(200)]
        public string SectionName { get; set; }

        [MaxLength(3000)]
        public string Description { get; set; }
    }
}
