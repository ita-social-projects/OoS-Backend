using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models
{
    public class Child
    {
        public long Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(30)]
        public string MiddleName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        public DateTime DateOfBirth { get; set; }

        public Gender Gender { get; set; }

        [MaxLength(500)]
        public string PlaceOfStudy { get; set; }

        public long ParentId { get; set; }

        public long? SocialGroupId { get; set; }

        public virtual Parent Parent { get; set; }

        public virtual SocialGroup SocialGroup { get; set; }
    }
}