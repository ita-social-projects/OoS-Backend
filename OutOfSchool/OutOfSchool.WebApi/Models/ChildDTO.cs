using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models
{
    public class ChildDto
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [MaxLength(40)]
        [RegularExpression(@"[\w\-\']*", ErrorMessage = "First name cannot contains digits")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(40)]
        [RegularExpression(@"[\w\-\']*", ErrorMessage = "Last name cannot contains digits")]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(40)]
        [RegularExpression(@"[\w\-\']*", ErrorMessage = "Middle name cannot contains digits")]
        public string MiddleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; } = default;

        [Required(ErrorMessage = "Gender is required")]
        public Gender Gender { get; set; } = default;

        [MaxLength(500)]
        public string PlaceOfStudy { get; set; } = string.Empty;

        public long ParentId { get; set; } = default;

        public long? SocialGroupId { get; set; } = default;

        // TODO: define if we really need this in dto
        [JsonIgnore]
        public ParentDTO Parent { get; set; }
    }
}